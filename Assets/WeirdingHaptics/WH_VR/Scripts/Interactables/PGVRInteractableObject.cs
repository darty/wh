using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PGLibrary.PGVR
{
    /// <summary>
    /// Make an object interactable.
    /// Use PGVRInteractableController on the VR hands.
    /// 
    /// Based on Valve.VR.InteractionSystem.Interactable
    /// </summary>
    public class PGVRInteractableObject : MonoBehaviour
    {
        [Header("General")]
        public PGVRInteractableType interactableType = PGVRInteractableType.OneHanded;

        //highlight on over
        [Header("Object highlight")]
        [Tooltip("Set whether or not you want this interactible to highlight when hovering over it")]
        [SerializeField]
        private PGVRInteractableObjectHighlight interactableObjectHighlight;
        //hover
        public bool IsHovered { get; protected set; }
        public bool WasHovered { get; protected set; }
        private int hoverAmount = 0;
        //in range
        public bool IsInGrabRange { get; protected set; }
        public bool WasInGrabRange { get; protected set; }
        private int grabRangeAmount = 0;

        //interaction
        [Header("Interaction")]
        [SerializeField]
        private bool isInteractable = false;

        //interaction
        [Header("Selection")]
        [SerializeField]
        private bool isSelectable = false;

        //gabbing
        [Header("Grabbing")]
        [SerializeField]
        private bool isGrabbable = false;
        public bool releaseOnEndGrab = true;                   //release this object when grab ends

        public bool IsGrabbed { get; protected set; }
        public PGVRInteractableObject_Grabbable InteractableObjectGrabbable { get; private set; }



        [Header("Debug")]
        public bool showDebugLog;


        //events
        public event EventHandler<InteractEventArgs> OnInteracted;


        #region lifecycle

        private void Awake()
        {
            if(interactableObjectHighlight == null)
                interactableObjectHighlight = this.GetComponent<PGVRInteractableObjectHighlight>();
            InteractableObjectGrabbable = this.GetComponent<PGVRInteractableObject_Grabbable>();
        }


        #endregion


        #region  Hover

        /// <summary>
        /// Called when a PGVRInteractionController starts hovering over this object
        /// </summary>
        public virtual void OnHoverBegin(PGVRInteractableController interactableController)
        {
            //Debug.Log("OnHoverBegin: " + this.name + " time " + Time.time.ToString());

            hoverAmount += 1;

            WasHovered = IsHovered;
            IsHovered = true;

            if (!WasHovered && IsHovered)
            {
                interactableObjectHighlight?.StartHighlight();
            }
        }


        /// <summary>
        /// Called when a PGVRInteractionController stops hovering over this object
        /// </summary>
        public virtual void OnHoverEnd(PGVRInteractableController interactableController)
        {
            //Debug.Log("OnHoverEnd: " + this.name + " time " + Time.time.ToString());

            hoverAmount -= 1;
            hoverAmount = Mathf.Max(hoverAmount, 0);

            if (hoverAmount <= 0)
            {
                WasHovered = IsHovered = false;

                interactableObjectHighlight?.StopHighlight(IsInGrabRange);
            }


        }


        /// <summary>
        /// Called when a PGVRInteractionController is hovering over this object
        /// </summary>
        public virtual void OnHoverUpdate(PGVRInteractableController interactableController)
        {
            interactableObjectHighlight?.UpdateHighlight(IsGrabbed);
        }

        #endregion


        #region Grabrange

        /// <summary>
        /// Called when this object enters the grabrange of a PGVRInteractionController
        /// </summary>
        public virtual void OnEnterGrabRange(PGVRInteractableController interactableController)
        {
            //Debug.Log("OnEnterGrabRange: " + this.name + " time " + Time.time.ToString());

            if (IsGrabbed)
                return;

            grabRangeAmount += 1;

            WasInGrabRange = IsInGrabRange;
            IsInGrabRange = true;

            if (!WasInGrabRange && IsInGrabRange)
            {
                interactableObjectHighlight?.StartInRangeHighlight();
            }
        }


        /// <summary>
        ///  Called when this object exits the grabrange of a PGVRInteractionController
        /// </summary>
        public virtual void OnExitGrabRange(PGVRInteractableController interactableController)
        {
            //Debug.Log("OnExitGrabRange: " + this.name + " time " + Time.time.ToString());

            if (IsGrabbed)
                return;

            grabRangeAmount -= 1;
            grabRangeAmount = Mathf.Max(grabRangeAmount, 0);

            if (grabRangeAmount <= 0)
            {
                WasInGrabRange = IsInGrabRange = false;

                interactableObjectHighlight?.StopInRangeHighlight();
            }
        }


        /// <summary>
        /// Called when a PGVRInteractionController is hovering over this object
        /// </summary>
        public virtual void OnInGrabRangeUpdate(PGVRInteractableController interactableController)
        {
            if (IsGrabbed)
                return;

            interactableObjectHighlight?.UpdateInRangeHighlight(IsGrabbed);
        }


        #endregion


        #region Interaction

        public bool IsInteractable()
        {
            return isInteractable;
        }

        public void Interact(PGVRInteractableController interactableController, PGVRGrabType grabType)
        {
            if (!isInteractable)
                return;

            OnInteracted?.Invoke(this, new InteractEventArgs(this, interactableController, grabType));
        }

        #endregion



        #region Grabbing

        public void SetIsGrabbable(bool enabled)
        {
            //Debug.Log("SetIsGrabbable " + name + ": enabled = " + enabled.ToString());
            isGrabbable = enabled;
        }


        public bool IsGrabbable()
        {
            if (isGrabbable && InteractableObjectGrabbable == null)
                Debug.LogWarning("PGVRInteractableObject: isGrabbable is activated but not Grabbable behaviour was added. Please add PGVRInteractableObject_Grabbable");

            return isGrabbable && InteractableObjectGrabbable != null;
        }


        public PGVRGrabbingFlags GetGrabbingFlags()
        {
            return InteractableObjectGrabbable.grabbingFlags;
        }


        public void StartGrab(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity, PGVRGrabType grabType)
        {
            IsGrabbed = true;
            InteractableObjectGrabbable?.OnGrabStart(interactableController, velocity, angularVelocity, grabType);
        }


        public void UpdateGrab(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            InteractableObjectGrabbable?.OnGrabUpdate(interactableController, velocity, angularVelocity);
        }


        public void EndGrab(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity, bool grabbedByOtherController)
        {
            IsGrabbed = false;
            InteractableObjectGrabbable?.OnGrabEnd(interactableController, velocity, angularVelocity, grabbedByOtherController);
        }

        #endregion



        #region Debug

        private void InteractableObjectDebugLog(string msg)
        {
            if (showDebugLog)
            {
                Debug.Log("<b>[Polygoat PGVR]</b> InteractableObject (" + this.name + "): " + msg);
            }
        }

        #endregion
    }
}
