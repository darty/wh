using Polygoat.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    /// <summary>
    /// Makes a PGVRInteractableObject grabbable.
    /// Grabbable will snap back to its original position on release.
    /// </summary>
    public class PGVRInteractableObject_Grabbable : MonoBehaviour
    {
        public const PGVRGrabbingFlags defaultAttachmentFlags = PGVRGrabbingFlags.ParentToHand |
                                                           PGVRGrabbingFlags.DetachOthers |
                                                           PGVRGrabbingFlags.DetachFromOtherHand |
                                                           PGVRGrabbingFlags.TurnOnKinematic |
                                                           PGVRGrabbingFlags.SnapOnAttach;

        [Header("Grabbing settings")]
        [EnumFlags]
        public PGVRGrabbingFlags grabbingFlags = defaultAttachmentFlags;
        [Tooltip("Hide the controller part of the hand on attachment and show on detach")]
        public bool hideControllerOnAttach = false;

        [Tooltip("Should object move smoothly to the hand")]
        public bool attachEaseIn = false;                       //smooth attach to controller
        public float attachEaseInDuration = 0.15f;              //smooth attach duration
        [HideInInspector]
        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

        [Tooltip("Should the rendered hand lock on to and follow the object")]
        public bool handFollowTransform = false;

        [Header("Release - Reset")]
        public bool resetOnRelease = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        [Header("Release - Physics")]
        public bool startRigidbodyOnRelease = false;
        public bool startSubRigidbodies = false;
        protected new Rigidbody rigidbody;
        public float velocityMultiplier = 1f;
        public float angularVelocityMultiplier = 1f;
        protected RigidbodyInterpolation originalInterpolation = RigidbodyInterpolation.None;

        //GrabPose
        public PGVRInteractableObject_GrabPose GrabPose { get; private set; }
        public bool HasGrabPose {
            get { return GrabPose != null; }
        }

        //events
        public event EventHandler<GrabStartEventArgs> OnGrabStarted;
        public event EventHandler<GrabEventArgs> OnGrabUpdated;
        public event EventHandler<GrabEndEventArgs> OnGrabEnded;


        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            GrabPose = this.GetComponent<PGVRInteractableObject_GrabPose>();
        }


        public virtual void OnGrabStart(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity, PGVRGrabType grabType)
        {
            originalPosition = this.transform.position;
            originalRotation = this.transform.rotation;

            if(rigidbody != null)
            {
                originalInterpolation = rigidbody.interpolation;
            }

            //event
            OnGrabStarted?.Invoke(this, new GrabStartEventArgs(this, interactableController, velocity, angularVelocity, grabType));
        }


        public virtual void OnGrabUpdate(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            //event
            OnGrabUpdated?.Invoke(this, new GrabEventArgs(this, interactableController, velocity, angularVelocity));
        }


        public virtual void OnGrabEnd(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity, bool grabbedByOtherController)
        {
            if (resetOnRelease)
                ResetPosition();

            if (startRigidbodyOnRelease)
                StartRigidbody(velocity, angularVelocity);

            //event
            OnGrabEnded?.Invoke(this, new GrabEndEventArgs(this, interactableController, velocity, angularVelocity, grabbedByOtherController));
        }


        public bool HasGrabbingFlag(PGVRGrabbingFlags flag)
        {
            return (grabbingFlags & flag) == flag;
        }

        #region Reset

        private void ResetPosition()
        {
            // Restore position/rotation
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }

        #endregion


        #region Physics

        public void StartRigidbody(Vector3 velocity, Vector3 angularVelocity)
        {
            if (rigidbody == null)
                Debug.LogWarning("PGVRInteractableObject_Grabbable StartRigidbody without rigidbody");

            //reset interpolation
            rigidbody.interpolation = originalInterpolation;

            //throw?
            rigidbody.velocity = velocity * velocityMultiplier;
            rigidbody.angularVelocity = angularVelocity * angularVelocityMultiplier;

            if(startSubRigidbodies)
            {
                Rigidbody[] subRigidbodies = rigidbody.gameObject.GetComponentsInChildrenOnly<Rigidbody>(false);
                foreach(Rigidbody rb in subRigidbodies)
                {
                    rb.velocity = velocity * velocityMultiplier;
                    rb.angularVelocity = angularVelocity * angularVelocityMultiplier;
                }
            }
        }

        #endregion

    }
}
