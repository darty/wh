using PGLibrary.Helpers;
using PGLibrary.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace PGLibrary.PGVR
{
    /// <summary>
    /// This class makes it possible to interact with PGVRInteractableObjects. 
    /// Usually this should be added to the hands.
    /// 
    /// Based on Valve.VR.InteractionSystem.Hand
    /// </summary>
    public class PGVRInteractableController : MonoBehaviour
    {
        public struct GrabbedInteractableObject
        {
            //general
            public GameObject attachedGO;
            public PGVRInteractableObject interactableObject;
            public PGVRGrabbingFlags grabbingFlags;
            public PGVRGrabType grabType;
            public float attachStartTime;

            //easing grab
            public bool easeInCompleted;
            public Vector3 easeSourcePosition;
            public Quaternion easeSourceRotation;
            public Vector3 endPositionOffset;         //local offset in attachmentPointTransform space
            public Quaternion endRotationOffset;      //local offset in attachmentPointTransform space

            //physics
            public Rigidbody attachedRigidbody;
            public CollisionDetectionMode collisionDetectionMode;
            public bool attachedRigidbodyWasKinematic;
            public bool attachedRigidbodyUsedGravity;

            //parent
            public Transform originalParent;
            public Transform attachmentPointTransform;
            public bool isParentedToController;

            //controller
            public bool controllerWasVisible;
        }

        //input
        [Header("Input")]
        public PGVRInteractableControllerInput controllerInput;
        public PGVRInteractableController otherController;
        public bool grabWithGrip = true;
        public bool grabWithPinch = true;

        [Header("Interactables")]
        public LayerMask interactablesLayerMask;

        [Header("Hover")]
        //hovering
        public Transform hoverSphereCenter;
        public float hoverSphereRadius = 0.1f;
        public float hoverUpdateInterval = 0.1f;
        private WaitForSeconds hoverWait;
        private CoroutinePlus checkHoverRoutine;
        private Collider[] hoverOverlappingColliders;
        private const int ColliderArraySize = 16;

        private PGVRInteractableObject hoveringInteractableObject;
        public PGVRInteractableObject HoveringInteractableObject
        {
            get { return hoveringInteractableObject; }
            set
            {
                //same as before?
                if (hoveringInteractableObject == value)
                    return;

                if(hoveringInteractableObject != null)
                {
                    //send hover ended
                    ControllerDebugLog("HoverEnd: " + hoveringInteractableObject.gameObject.name);
                    hoveringInteractableObject.OnHoverEnd(this);
                }

                hoveringInteractableObject = value;

                if(hoveringInteractableObject != null)
                {
                    //send hover begin
                    ControllerDebugLog("HoverBegin: " + hoveringInteractableObject.gameObject.name);
                    hoveringInteractableObject.OnHoverBegin(this);
                }
            }
        }

        //distance checking
        /*
        [Header("Distancegrabber")]
        public bool grabFromDistance = false;
        public float maxDistance = 1f;
        public PGVRInteractableControllerDistanceGrabber distanceGrabber;
        */

        //grabbing
        [Header("Grab")]
        [Tooltip("A transform on the hand to center attached objects on")]
        public Transform objectAttachmentPoint;
        private GrabbedInteractableObject grabbedInteractableObject;
        private PGVRInteractableObject previousGrabbedInteractableObject;
        private float previousGrabbedInteractableObjectReleaseTime;
        private const float ignorePreviousGrabbelInteractaleDuration = 0.5f;
        private bool canInteract = true;
        public bool CanInteract
        {
            get { return canInteract; }
            set
            {
                canInteract = value;
                if (!canInteract)
                {
                    StopCheckingHover();
                    ReleaseCurrentGrab();
                }
                else
                {
                    StartCheckingHover();
                }
            }
        }

        [Header("Rendering")]
        public PGVRHandRenderer handRenderer;
        public PGVRHandSkeleton handSkeleton;

        [Header("Physics")]
        public PGVRHandPhysics handPhysics;

        [Header("Debug")]
        public bool showDebugLog;

        //velocities
        public Vector3 EstimatedVelocity { get; private set; }
        public Vector3 EstimatedAngularVelocity { get; private set; }

        //events
        //public event EventHandler<EventArgs> OnRenderModelLoaded;

        //constants
        //numbers from steamVR
        /*
        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;
        */

        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 200f;
        protected const float MaxAngularVelocityChange = 40f;





        private void OnDrawGizmos()
        {
            if(HasGrabbedInteractableObject())
            {
                Vector3 objectFinalPos = handSkeleton.GetFinalObjectPosition(grabbedInteractableObject);
                Gizmos.DrawSphere(objectFinalPos, 0.05f);
                Gizmos.DrawLine(this.transform.position, objectFinalPos);
            }
        }


        protected virtual void Awake()
        {
            hoverWait = new WaitForSeconds(hoverUpdateInterval);
            // allocate array for colliders
            hoverOverlappingColliders = new Collider[ColliderArraySize];

            if (objectAttachmentPoint == null)
            {
                ControllerDebugLog("objectAttachmentPoint not set, use interactablecontroller transform as default.");
                objectAttachmentPoint = this.transform;
            }

            if (otherController == null)
            {
                ControllerDebugLog("otherController not set.");
            }

            /*
            if(grabFromDistance)
            {
                distanceGrabber.Initialize(this);
            }
            */
        }



        protected virtual void Start()
        {
            InitHand();
        }

        protected virtual void OnEnable()
        {
            if(CanInteract)
                StartCheckingHover();
        }


        protected virtual void OnDisable()
        {
            StopCheckingHover();
        }

        protected virtual void Update()
        {
            if (!canInteract)
                return;

            /*
            if (grabFromDistance)
                distanceGrabber?.UpdateInteractablesInRange();
            */

            UpdateEstimatedVelocities();
            UpdatedHovered();
            UpdateInteraction();
            UpdateGrab();
        }


        protected void FixedUpdate()
        {
            UpdateGrabbedObjectPhysics();
        }



        #region Hover

        private void StartCheckingHover()
        {
            checkHoverRoutine = CoroutineManager.Instance.CreateUniqueCoroutine(ref checkHoverRoutine, CheckHoverRoutine());
        }


        private void StopCheckingHover()
        {
            checkHoverRoutine?.Stop();
            HoveringInteractableObject = null;
        }


        private void UpdatedHovered()
        {
            if (HoveringInteractableObject == null)
                return;

            HoveringInteractableObject.OnHoverUpdate(this);
        }


        private void CheckHover()
        {

            //search closest interactable
            PGVRInteractableObject closestInteractableObject = null;
            bool foundInteractableObject = FindHoveredInteractableObject(hoverSphereCenter.position, hoverSphereRadius, ref closestInteractableObject);
        
            HoveringInteractableObject = closestInteractableObject;
        }


        private IEnumerator CheckHoverRoutine()
        {
            while (true)
            {
                CheckHover();
                yield return hoverWait;
            }
        }


        private bool FindHoveredInteractableObject(Vector3 position, float radius, ref PGVRInteractableObject hoveredInteractableObject)
        {
            bool foundInteractableObject = false;
            int overlappingCount = Physics.OverlapSphereNonAlloc(position, radius, hoverOverlappingColliders, interactablesLayerMask);

            if (overlappingCount == ColliderArraySize)
                Debug.LogWarning("<b>[Polygoat PGVR]</b> This controller is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on PGVRInteractableController.cs");

            float closestDistance = float.MaxValue;

            for (int colliderIndex = 0; colliderIndex < overlappingCount; colliderIndex++)
            {
                Collider collider = hoverOverlappingColliders[colliderIndex];
                PGVRInteractableObject interactableObject = GetInteractableObjectFromCollider(collider); 

                if(!IsHoverCandidate(interactableObject))
                    continue;

                float distance = Vector3.Distance(interactableObject.transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    hoveredInteractableObject = interactableObject;
                    foundInteractableObject = true;
                }
            }

            //if we didn't find an interactableObject with hoversphere, check the distanceGrabber
            /*
            if(!foundInteractableObject && grabFromDistance)
            {
                foundInteractableObject = distanceGrabber.FindHoveredInteractableObject(IsHoverCandidate, ref hoveredInteractableObject);
            }
            */

            return foundInteractableObject;
        }


        private bool ShouldIgnoreInteractable(PGVRInteractableObject interactableObject)
        {
            //if this was the previous grabbed interactable, check if ignore time is over
            if (previousGrabbedInteractableObject != null && previousGrabbedInteractableObject == interactableObject)
            {
                if (Time.time < previousGrabbedInteractableObjectReleaseTime + ignorePreviousGrabbelInteractaleDuration)
                {
                    //Debug.Log("---------------------------------------- ShouldIgnoreInteractable " + interactableObject.name  + " ---------------------------------");
                    return true;
                }
            }
                
            return false;
        }


        public bool IsHoveringInteractable()
        {
            return HoveringInteractableObject != null;
        }


        public bool IsHoverCandidate(PGVRInteractableObject interactableObject)
        {
            if (interactableObject == null)
                return false;

            //if not grabbable or interactable, ignore?
            if (!interactableObject.IsGrabbable() && !interactableObject.IsInteractable())
                return false;

            //if this was the previous grabbed interactable, check if ignore time is over
            if (ShouldIgnoreInteractable(interactableObject))
                return false;

            return true;
        }


        public static PGVRInteractableObject GetInteractableObjectFromCollider(Collider collider)
        {
            PGVRInteractableObject interactableObject = collider.GetComponent<PGVRInteractableObject>();

            if (interactableObject == null)
            {
                //no interactable attached to collider, check attachedRigidbody
                interactableObject = collider.attachedRigidbody?.GetComponent<PGVRInteractableObject>();
            }

            return interactableObject;
        }


        #endregion


        #region Interaction

        private void UpdateInteraction()
        {
            if (HoveringInteractableObject == null)
                return;

            //interact?
            PGVRGrabType grabType = GetStartGrabType();
            if (grabType != PGVRGrabType.None)
            {
                InteractWithObject(HoveringInteractableObject, grabType);
            }
            
        }

        private void InteractWithObject(PGVRInteractableObject interactableObject, PGVRGrabType grabType)
        {
            ControllerDebugLog("InteractWithObject interactableObject: " + interactableObject.gameObject.name);

            interactableObject.Interact(this, grabType);
        }


        #endregion


        #region grab

        private void UpdateGrab()
        {
            /*
            if (hoveringInteractableObject == null)
                Debug.Log("no hoveringInteractableObject");

            if (hoveringInteractableObject != null && !hoveringInteractableObject.IsGrabbable())
                Debug.Log("hoveringInteractableObject set, but not grabbable");
                */

            //start grabbing new object??
            if (HoveringInteractableObject != null && !HoveringInteractableObject.IsGrabbed && HoveringInteractableObject.IsGrabbable() && !ShouldIgnoreInteractable(HoveringInteractableObject))
            {
                PGVRGrabType grabType = GetStartGrabType();
                if (grabType != PGVRGrabType.None)
                {
                    if (HoveringInteractableObject.IsGrabbed && !HoveringInteractableObject.GetGrabbingFlags().HasFlag(PGVRGrabbingFlags.DetachFromOtherHand))
                    {
                        //Debug.Log("is already grabbed!!!");
                    }
                    else
                    {
                        //Debug.Log("start grab");
                        GrabInteractableObject(HoveringInteractableObject, grabType);
                    }
                }
            }

            //already grabbed an object?
            if(HasGrabbedInteractableObject())
            {
                //update grab
                UpdateGrabbedInteractable();

                //update skeleton
                handSkeleton.UpdateGrab();

                //end grab?
                //Debug.Log("IsGrabbingWithType(grabbedInteractableObject.grabType)" + IsGrabbingWithType(grabbedInteractableObject.grabType).ToString());
                if (grabbedInteractableObject.interactableObject.releaseOnEndGrab && IsGrabbingWithType(grabbedInteractableObject.grabType) == false)
                {
                    ReleaseGrab(grabbedInteractableObject.interactableObject);
                }
            }
        }

     
        private PGVRGrabType GetStartGrabType()
        {
            if (grabWithGrip && controllerInput.IsGrabGripDown())
                return PGVRGrabType.Grip;

            if (grabWithPinch && controllerInput.IsGrabPinchDown())
                return PGVRGrabType.Pinch;

            return PGVRGrabType.None;
        }


        public PGVRGrabType GetGrabType()
        {
            if (controllerInput.IsGrabbingGrip())
                return PGVRGrabType.Grip;

            if (controllerInput.IsGrabbingPinch())
                return PGVRGrabType.Pinch;

            return PGVRGrabType.None;
        }


        public bool IsGrabbingWithType(PGVRGrabType type)
        {
            //Debug.Log("IsGrabbingWithType: " + type.ToString());
            switch (type)
            {
                case PGVRGrabType.Pinch:
                    return controllerInput.IsGrabbingPinch();

                case PGVRGrabType.Grip:
                    return controllerInput.IsGrabbingGrip();

                default:
                    return false;
            }
        }


        public bool HasGrabbedInteractableObject()
        {
            return grabbedInteractableObject.interactableObject != null;
        }

        public bool HasGrabbedInteractableObject(PGVRInteractableObject interactableObject)
        {
            return grabbedInteractableObject.interactableObject != null && grabbedInteractableObject.interactableObject == interactableObject;
        }



        /// <summary>
        /// Grab an interactableObject with this controller
        /// </summary>
        /// <param name="interactableObject"></param>
        /// <param name="grabType"></param>
        public void GrabInteractableObject(PGVRInteractableObject interactableObject, PGVRGrabType grabType)
        {
            ControllerDebugLog("Grab interactableObject: " + interactableObject.gameObject.name + " with type " + grabType.ToString());

            //already has something grabbed? Release it
            ReleaseCurrentGrab();

            //detach from other hand
            if (interactableObject.IsGrabbed && interactableObject.GetGrabbingFlags().HasFlag(PGVRGrabbingFlags.DetachFromOtherHand))
            {
                otherController.ReleaseGrab(interactableObject, true);
            }

            //setup GrabbedInteractableObject
            //general
            grabbedInteractableObject = new GrabbedInteractableObject();
            grabbedInteractableObject.interactableObject = interactableObject;
            grabbedInteractableObject.grabbingFlags = interactableObject.GetGrabbingFlags();
            grabbedInteractableObject.grabType = grabType;
            grabbedInteractableObject.attachedGO = interactableObject.gameObject;
            grabbedInteractableObject.attachStartTime = Time.time;
            //parent
            grabbedInteractableObject.attachmentPointTransform = objectAttachmentPoint;
            grabbedInteractableObject.originalParent = interactableObject.transform.parent;
            //easing
            grabbedInteractableObject.easeSourcePosition = grabbedInteractableObject.attachedGO.transform.position;
            grabbedInteractableObject.easeSourceRotation = grabbedInteractableObject.attachedGO.transform.rotation;

            //rigidbody settings
            grabbedInteractableObject.attachedRigidbody = interactableObject.GetComponent<Rigidbody>();
            if (grabbedInteractableObject.attachedRigidbody != null)
            {
                grabbedInteractableObject.attachedRigidbodyWasKinematic = grabbedInteractableObject.attachedRigidbody.isKinematic;
                grabbedInteractableObject.attachedRigidbodyUsedGravity = grabbedInteractableObject.attachedRigidbody.useGravity;
                Debug.Log("PGVRController GrabInteractableObject attachedRigidbodyWasKinematic = " + grabbedInteractableObject.attachedRigidbodyWasKinematic.ToString());

                //turn on kinematic
                if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.TurnOnKinematic))
                {
                    //continuous collision detection is not supported with kinematic (only discrete or speculative continuous)
                    grabbedInteractableObject.collisionDetectionMode = grabbedInteractableObject.attachedRigidbody.collisionDetectionMode;
                    if (grabbedInteractableObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                        grabbedInteractableObject.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                        
                    grabbedInteractableObject.attachedRigidbody.isKinematic = true;
                }

                //turn off gravity
                if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.TurnOffGravity))
                {
                    grabbedInteractableObject.attachedRigidbody.useGravity = false;
                }

                if(grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.VelocityMovement))
                {
                    grabbedInteractableObject.attachedRigidbody.maxAngularVelocity = MaxAngularVelocityChange;
                }
            }
          
            //parent to hand?
            if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.ParentToHand))
            {
                ControllerDebugLog("Parent interactableObject to hand: " + interactableObject.gameObject.name);
                interactableObject.transform.parent = this.transform;
                grabbedInteractableObject.isParentedToController = true;
            }
            else
            {
                grabbedInteractableObject.isParentedToController = false;
            }

            //Ask Skeleton to grab the object
            handSkeleton.GrabObject(grabbedInteractableObject);
            //skeleton will evaluete SnapOnAttach itsels
            /*
            if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.SnapOnAttach))
            {
                //snap object to handskeleton
                handSkeleton.GrabObject(grabbedInteractableObject);
            }
            else
            {
                //what to do without SnapOnAttach?
                //???
            }*/

            //set end offsets
            grabbedInteractableObject.endPositionOffset = grabbedInteractableObject.attachmentPointTransform.InverseTransformPoint(grabbedInteractableObject.attachedGO.transform.position);
            grabbedInteractableObject.endRotationOffset = Quaternion.Inverse(grabbedInteractableObject.attachmentPointTransform.rotation) * grabbedInteractableObject.attachedGO.transform.rotation;

            //ease in grab?
            if (grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.attachEaseIn)
            {
                grabbedInteractableObject.attachedGO.transform.position = grabbedInteractableObject.easeSourcePosition;
                grabbedInteractableObject.attachedGO.transform.rotation = grabbedInteractableObject.easeSourceRotation;
            }

            //graphics
            if(grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.hideControllerOnAttach)
            {
                grabbedInteractableObject.controllerWasVisible = IsControllerVisible();
                HideController();
            }


            //get estimated velocities
            Vector3 velocity, angularVelocity;
            GetEstimatedPeakVelocities(out velocity, out angularVelocity);

            //send event to interactableObject
            interactableObject.StartGrab(this, velocity, angularVelocity, grabType);

            ControllerDebugLog("Grab interactableObject END: " + interactableObject.gameObject.name + " with type " + grabType.ToString());

        }


        private void UpdateGrabbedInteractable()
        {
            //get estimated velocities
            Vector3 velocity, angularVelocity;
            GetEstimatedPeakVelocities(out velocity, out angularVelocity);

            //Debug.Log("PGVRInteractableController UpdateGrabbedInteractable: " + Time.time);

            grabbedInteractableObject.interactableObject.UpdateGrab(this, velocity, angularVelocity);
        }


        protected void ReleaseGrab(PGVRInteractableObject interactableObject, bool grabbedByOtherController = false)
        {
            ControllerDebugLog("ReleaseGrab interactableObject: " + interactableObject.gameObject.name);

            //if this isn't the current grabbed interactable object, stop here
            if(interactableObject == null || grabbedInteractableObject.interactableObject != interactableObject)
            {
                ControllerDebugLog("Nothing to release.");
                return;
            }

            PGVRInteractableType interactableType = grabbedInteractableObject.interactableObject.interactableType;

            //restore orinal parent
            if (grabbedInteractableObject.isParentedToController)
            {
                ControllerDebugLog("ReleaseGrab restore original parent");
                //if (grabbedInteractableObject.originalParent != null)
                //{
                    grabbedInteractableObject.attachedGO.transform.parent = grabbedInteractableObject.originalParent;
                //because the steam VR setup is flagged as Dontdestroyonload, these (previous- childonject will be in the dontdestroyonload scene, move them
                if(grabbedInteractableObject.originalParent == null)
                    SceneManager.MoveGameObjectToScene(grabbedInteractableObject.attachedGO, SceneManager.GetActiveScene());
       
                //}
            }

            //physics
            if (grabbedInteractableObject.attachedRigidbody != null)
            {
                //turn off kinematic
                if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.TurnOnKinematic))
                {
                    Debug.Log("PGVRController ReleaseGrab attachedRigidbodyWasKinematic = " + grabbedInteractableObject.attachedRigidbodyWasKinematic.ToString());
                    grabbedInteractableObject.attachedRigidbody.isKinematic = grabbedInteractableObject.attachedRigidbodyWasKinematic;
                    grabbedInteractableObject.attachedRigidbody.collisionDetectionMode = grabbedInteractableObject.collisionDetectionMode;
                }

                //turn on gravity
                if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.TurnOffGravity))
                {
                    grabbedInteractableObject.attachedRigidbody.useGravity = grabbedInteractableObject.attachedRigidbodyUsedGravity;
                }
            }

            //restore pose
            handSkeleton.ReleaseObject();
            //not needed anymore, let Skeleton evaluate SnapOnAttach itself
            /*
            if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.SnapOnAttach))
            {
                handSkeleton.ReleaseObject();
            }
            */

            //graphics
            if (grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.hideControllerOnAttach)
            {
                //if controller was visible before grabbig, show it again
                if(grabbedInteractableObject.controllerWasVisible)
                    ShowController();
            }

            //send event to interactableObject
            Vector3 velocity, angularVelocity;
            GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            interactableObject.EndGrab(this, velocity, angularVelocity, grabbedByOtherController);

            //keep reference as previous
            previousGrabbedInteractableObject = grabbedInteractableObject.interactableObject;
            previousGrabbedInteractableObjectReleaseTime = Time.time;

            //cleanup grabbedInteractableObject
            grabbedInteractableObject.attachedGO = null;
            grabbedInteractableObject.interactableObject = null;
            grabbedInteractableObject.attachedRigidbody = null;
            grabbedInteractableObject.originalParent = null;
            grabbedInteractableObject.attachmentPointTransform = null;

            //if this was a twohanded interactable, also release other hand
            if (interactableType == PGVRInteractableType.TwoHanded)
            {
                ControllerDebugLog("ReleaseGrab interactableObject was TwoHanded, also release other hand.");
                otherController.ReleaseCurrentGrab();
            }
        }


        public void ReleaseCurrentGrab()
        {

            if (HasGrabbedInteractableObject())
            {
                //PGVRInteractableType interactableType = grabbedInteractableObject.interactableObject.interactableType; 
                ReleaseGrab(grabbedInteractableObject.interactableObject);

                /*
                if(interactableType == PGVRInteractableType.TwoHanded)
                {

                }*/
            }
        }



        public void ReleaseInteractables(PGVRInteractableType interactableType)
        {
            if (interactableType == PGVRInteractableType.OneHanded)
            {
                //remove interactableObject
                ReleaseCurrentGrab();
            }
            else if (interactableType == PGVRInteractableType.TwoHanded)
            {
                //remove if current interactableObject is part of twohanded package
                if (HasGrabbedInteractableObject())
                {
                    if(grabbedInteractableObject.interactableObject.interactableType == PGVRInteractableType.TwoHanded)
                        ReleaseCurrentGrab();
                }
            }

        }


        private void UpdateGrabbedObjectPhysics()
        {
            if (!HasGrabbedInteractableObject())
                return;

            //grabbed object movement
            if(grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.VelocityMovement))
            {
                //velocity movement
                Debug.Log("UpdateGrabbedObjectPhysics VelocityMovement");
                if(!grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.attachEaseIn || grabbedInteractableObject.easeInCompleted)
                {
                    UpdateGrabbedObjectVelocity();
                }
            }
            else
            {
                //non velocity movement
                if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.ParentToHand))
                {
                    //update object based on skeleton pose
                    
                    grabbedInteractableObject.attachedGO.transform.position = handSkeleton.GetTargetItemPosition();
                    grabbedInteractableObject.attachedGO.transform.rotation = handSkeleton.GetTargetItemRotation();
                    
                }
            }

            //ease in attachment
            if(grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.attachEaseIn && !grabbedInteractableObject.easeInCompleted)
            {
                float t = MathHelper.Remap(Time.time, grabbedInteractableObject.attachStartTime, grabbedInteractableObject.attachStartTime + grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.attachEaseInDuration, 0f, 1f);

                if(t < 1f)
                {
                    if(grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.VelocityMovement))
                    {
                        //cancel out physics movement during grab
                        grabbedInteractableObject.attachedRigidbody.velocity = Vector3.zero;
                        grabbedInteractableObject.attachedRigidbody.angularVelocity = Vector3.zero;
                    }

                    //convert t to value on snapAttachEaseInCurve
                    t = grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.snapAttachEaseInCurve.Evaluate(t);

                    //set pos and rot
                    grabbedInteractableObject.attachedGO.transform.position = Vector3.Lerp(grabbedInteractableObject.easeSourcePosition, GetGrabbedObjectTargetPosition(), t);
                    grabbedInteractableObject.attachedGO.transform.rotation = Quaternion.Lerp(grabbedInteractableObject.easeSourceRotation, GetGrabbedObjectTargetRotation(), t);
                }
                else
                {
                    grabbedInteractableObject.easeInCompleted = true;
                    //could send an event here
                }

            }
        }


        
        private Vector3 GetGrabbedObjectTargetPosition()
        {
            //works but not with blending poses
            //return grabbedInteractableObject.attachmentPointTransform.TransformPoint(grabbedInteractableObject.endPositionOffset);

            return handSkeleton.GetTargetItemPosition();

            //no need to get handSkeleton.GetGrabFinalPosition each frame
            /*
            if (handSkeleton != null && grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.HasGrabPose)
                return handSkeleton.GetGrabFinalPosition();
            else
                return grabbedInteractableObject.attachmentPointTransform.TransformPoint(grabbedInteractableObject.endPositionOffset);
                */
        }
        
        
        private Quaternion GetGrabbedObjectTargetRotation()
        {
            return grabbedInteractableObject.attachmentPointTransform.rotation * grabbedInteractableObject.endRotationOffset;

            //no need to get handSkeleton.GetGrabFinalRotation each frame
            /*
            if (handSkeleton != null && grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.HasGrabPose)
                return handSkeleton.GetGrabFinalRotation();
            else
                return grabbedInteractableObject.attachmentPointTransform.rotation * grabbedInteractableObject.endRotationOffset;
                */
        }




        #endregion


        #region Controller velocity


        private void UpdateEstimatedVelocities()
        {
            Vector3 velocity, angularVelocity;
            GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            EstimatedVelocity = velocity;
            EstimatedAngularVelocity = angularVelocity;
        }


        protected void UpdateGrabbedObjectVelocity()
        {
            Vector3 targetVelocity, targetAngularVelocity;
            bool success = GetAttachedVelocities(grabbedInteractableObject, out targetVelocity, out targetAngularVelocity);

            //set new velocities to rigidbody
            grabbedInteractableObject.attachedRigidbody.velocity = Vector3.MoveTowards(grabbedInteractableObject.attachedRigidbody.velocity, targetVelocity, MaxVelocityChange);
            //grabbedInteractableObject.attachedRigidbody.angularVelocity = Vector3.MoveTowards(grabbedInteractableObject.attachedRigidbody.angularVelocity, targetAngularVelocity, MaxAngularVelocityChange);
            grabbedInteractableObject.attachedRigidbody.angularVelocity = targetAngularVelocity;

        }


        protected bool GetAttachedVelocities(GrabbedInteractableObject grabbedInteractableObject, out Vector3 targetVelocity, out Vector3 targetAngularVelocity)
        {
            //velocity
            Vector3 targetPosition = handSkeleton.GetFinalObjectPosition(grabbedInteractableObject);
            Vector3 positionDelta = (targetPosition - grabbedInteractableObject.attachedRigidbody.position);
            targetVelocity = (positionDelta * VelocityMagic * Time.deltaTime);

            //angular velocity
            Quaternion targetItemRotation = handSkeleton.GetFinalObjectRotation(grabbedInteractableObject);
            Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(grabbedInteractableObject.attachedRigidbody.rotation);

            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (float.IsNaN(axis.x))
                targetAngularVelocity = Vector3.zero;
            else
                targetAngularVelocity = axis * angle * AngularVelocityMagic * Time.deltaTime;
    

            return true;
        }


        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            controllerInput.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            velocity = PGVRPlayer.Instance.trackingOriginTransform.TransformVector(velocity);
            angularVelocity = PGVRPlayer.Instance.trackingOriginTransform.TransformDirection(angularVelocity);
        }






        #endregion


        #region haptics

        public void TriggerHapticPulse()
        {
            controllerInput.Vibrate(0.005f, 1f, 1f);
        }

        public void TriggerHapticPulseMid()
        {
            controllerInput.Vibrate(0.25f, 1f, 4f);
        }

        public void TriggerHapticPulse(float duration, float frequency, float amplitude)
        {
            controllerInput.Vibrate(duration, frequency, amplitude);
        }

        #endregion


        #region graphics

        protected void InitHand()
        {
            ControllerDebugLog("InitHandRenderer " + name);

            handRenderer.InitHandRenderer();
            handSkeleton.InitHandSkeleton(handRenderer);
            handPhysics?.InitializeHandPhysics(this);

            /*
            GameObject renderModelInstance = GameObject.Instantiate(renderModelPrefab);
            renderModelInstance.layer = gameObject.layer;
            renderModelInstance.tag = gameObject.tag;
            renderModelInstance.transform.parent = this.transform;
            renderModelInstance.transform.localPosition = Vector3.zero;
            renderModelInstance.transform.localRotation = Quaternion.identity;
            renderModelInstance.transform.localScale = renderModelPrefab.transform.localScale;

            MainRenderModel = renderModelInstance.GetComponent<RenderModel>();
            MainRenderModel.onControllerLoaded += MainRenderModel_onControllerLoaded;
            */
        }


        protected void HandFollowObject()
        {

        }


        protected bool IsControllerVisible()
        {
            if (handRenderer)
                return handRenderer.IsVisible;
            else
                return false;
        }


        protected void HideController(bool permanent = false)
        {
            handRenderer?.HideController();
        }


        protected void ShowController(bool permanent = false)
        {
            handRenderer?.ShowController();
        }

        #endregion


        #region Hand information

        public bool IsLeftHand()
        {
            return controllerInput.IsLeftHand(); ;
        }

        public bool IsRightHand()
        {
            return !IsLeftHand();
        }

        #endregion

        #region Debug

        private void ControllerDebugLog(string msg)
        {
            if (showDebugLog)
            {
                Debug.Log("<b>[Polygoat PGVR]</b> InteractableController (" + this.name + "): " + msg);
            }
        }

        #endregion
    }
}
