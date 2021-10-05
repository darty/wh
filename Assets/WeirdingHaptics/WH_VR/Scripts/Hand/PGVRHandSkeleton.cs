using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class PGVRHandSkeleton : MonoBehaviour
    {
        protected PGVRHandRenderer handRenderer;

        protected GameObject currentGrabbedGO;
        protected Transform attachmentPointTransform;
        protected bool handFollowTransform = false;

        protected Vector3 initialPositionOffset;
        protected Quaternion initialRotiationOffset;


        public void InitHandSkeleton(PGVRHandRenderer handRenderer)
        {
            if (this.handRenderer != null)
                this.handRenderer.OnModelLoaded -= HandRenderer_OnModelLoaded;

            this.handRenderer = handRenderer;

            if (this.handRenderer != null)
            {
                this.handRenderer.OnModelLoaded -= HandRenderer_OnModelLoaded;
                this.handRenderer.OnModelLoaded += HandRenderer_OnModelLoaded;
            }
        }


        protected virtual void OnEnable()
        {
            if (this.handRenderer != null)
            {
                this.handRenderer.OnModelLoaded -= HandRenderer_OnModelLoaded;
                this.handRenderer.OnModelLoaded += HandRenderer_OnModelLoaded;
            }
        }


        protected virtual void OnDisable()
        {
            if (this.handRenderer != null)
                this.handRenderer.OnModelLoaded -= HandRenderer_OnModelLoaded;
        }



        protected virtual void HandRenderer_OnModelLoaded(object sender, System.EventArgs e)
        {

        }


        public virtual void GrabObject(PGVRInteractableController.GrabbedInteractableObject grabbedInteractableObject)
        {
            //set values for current grab
            currentGrabbedGO = grabbedInteractableObject.attachedGO;
            attachmentPointTransform = grabbedInteractableObject.attachmentPointTransform;
            handFollowTransform = grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.handFollowTransform;
            initialPositionOffset = attachmentPointTransform.InverseTransformPoint(currentGrabbedGO.transform.position);
            initialRotiationOffset = Quaternion.Inverse(attachmentPointTransform.rotation) * currentGrabbedGO.transform.rotation;

            Debug.Log("PGVRHandSkeleton GrabObject, handFollowTransform = " + handFollowTransform.ToString());
        }


        public virtual void ReleaseObject()
        {
            //unset current grab values
            currentGrabbedGO = null;
            attachmentPointTransform = null;
        }



        public virtual void UpdateGrab()
        {

        }


        public virtual Vector3 GetFinalObjectPosition(PGVRInteractableController.GrabbedInteractableObject grabbedInteractableObject)
        {
            return Vector3.zero;
        }

        public virtual Quaternion GetFinalObjectRotation(PGVRInteractableController.GrabbedInteractableObject grabbedInteractableObject)
        {
            return Quaternion.identity; ;
        }


        public virtual Vector3 GetTargetItemPosition()
        {
            return attachmentPointTransform.TransformPoint(initialPositionOffset);
        }


        public virtual Quaternion GetTargetItemRotation()
        {
            return attachmentPointTransform.rotation * initialRotiationOffset;
        }

    }
}
