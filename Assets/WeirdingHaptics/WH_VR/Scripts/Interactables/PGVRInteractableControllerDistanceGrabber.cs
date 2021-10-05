using PGLibrary.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{

    [RequireComponent(typeof(Rigidbody))]
    public class PGVRInteractableControllerDistanceGrabber : MonoBehaviour
    {
        [Header("Visibility cone settings")]
        public Vector3 rayLocalDirection;

        protected PGVRInteractableController controller;
        protected Dictionary<PGVRInteractableObject, int> interactableCandidates = new Dictionary<PGVRInteractableObject, int>();



        private void OnDrawGizmos()
        {
            //draw ray
            //Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.TransformDirection(rayLocalDirection));
        }


        public void Initialize(PGVRInteractableController controller)
        {
            this.controller = controller;
        }



        #region hovered



        public bool FindHoveredInteractableObject(Func<PGVRInteractableObject, bool> isHoverCandidate, ref PGVRInteractableObject hoveredInteractableObject)
        {
            bool foundInteractableObject = false;
            float closestDistance = float.MaxValue;
            Vector3 rayWorldDirection = this.transform.TransformDirection(rayLocalDirection);

            //find closest 
            foreach (PGVRInteractableObject interactableObject in interactableCandidates.Keys)
            {
                if (!isHoverCandidate(interactableObject))
                    continue;

                //Debug.Log("FindHoveredInteractableObject check " + interactableObject.gameObject.name);

                //find interactable closest to the ray cast in direction
                Vector3 projectedPosition = MathHelper.ProjectPointOnLine(this.transform.position, rayWorldDirection, interactableObject.transform.position);

                //Debug.DrawLine(this.transform.position, (this.transform.position + rayWorldDirection), Color.red);
                Debug.DrawLine(this.transform.position, interactableObject.transform.position, Color.blue);
                Debug.DrawLine(this.transform.position, projectedPosition, Color.magenta);

                float distance = Vector3.Distance(this.transform.position, projectedPosition);
                float distanceFromCenter = Vector3.Distance(interactableObject.transform.position, projectedPosition);

                //Debug.Log("FindHoveredInteractableObject distance " + distance.ToString());

                if (distanceFromCenter < closestDistance)
                {
                    closestDistance = distanceFromCenter;
                    hoveredInteractableObject = interactableObject;
                    foundInteractableObject = true;
                }
            }

            return foundInteractableObject;
        }


        #endregion


        #region visibility


        private void SetInteractableObjectInRange(PGVRInteractableObject interactableObject)
        {
            // Add the interactableObject
            int refCount = 0;
            interactableCandidates.TryGetValue(interactableObject, out refCount);
            interactableCandidates[interactableObject] = refCount + 1;

            if (refCount == 0)
                interactableObject.OnEnterGrabRange(this.controller);
        }


        private void SetInteractableObjectOutOfRange(PGVRInteractableObject interactableObject)
        {
            // Remove the interactableObject if needed
            int refCount = 0;
            bool found = interactableCandidates.TryGetValue(interactableObject, out refCount);
            if (!found)
                return;

            if (refCount > 1)
                interactableCandidates[interactableObject] = refCount - 1;
            else
            {
                interactableCandidates.Remove(interactableObject);
                interactableObject.OnExitGrabRange(this.controller);
            }
        }


        public void UpdateInteractablesInRange()
        {
            foreach (PGVRInteractableObject interactableObject in interactableCandidates.Keys)
            {
                interactableObject.OnInGrabRangeUpdate(controller);
            }
        }


        #endregion


        #region Trigger

        private void OnTriggerEnter(Collider otherCollider)
        {
            // Get the interactableObject
            PGVRInteractableObject interactableObject = PGVRInteractableController.GetInteractableObjectFromCollider(otherCollider);

            if (interactableObject == null)
                return;

            // Add the interactableObject
            SetInteractableObjectInRange(interactableObject);
        }


        private void OnTriggerExit(Collider otherCollider)
        {
            // Get the interactableObject
            PGVRInteractableObject interactableObject = PGVRInteractableController.GetInteractableObjectFromCollider(otherCollider);

            if (interactableObject == null)
                return;

            // Remove the interactableObject
            SetInteractableObjectOutOfRange(interactableObject);
        }


        #endregion



    }
}
