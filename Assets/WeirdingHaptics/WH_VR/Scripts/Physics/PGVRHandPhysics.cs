using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRHandPhysics : MonoBehaviour
    {
        [Tooltip("Hand collider prefab to instantiate")]
        public PGVRHandCollider handColliderPrefab;
        public Vector3 handColliderOffset;
        public Vector3 handColliderRotationOffset;

        [Tooltip("Reset Hand collider if its more than this distance away from its targetposition")]
        public float resetDistance = 0.3f;

        private PGVRHandCollider handCollider;
        private PGVRInteractableController interactableController;


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(GetTargetPosition(), 0.01f);
        }


        public void InitializeHandPhysics(PGVRInteractableController interactableController)
        {
            this.interactableController = interactableController;
            InstantiateHandCollider();
        }



        private void InstantiateHandCollider()
        {
            handCollider = Instantiate(handColliderPrefab);
            handCollider.transform.position = GetTargetPosition();
            handCollider.InitializeHandcollider(interactableController);
        }


        private void FixedUpdate()
        {
            if (handCollider == null)
                return;

            UpdateColliderPosition();

            //TODO: update colliders based on finger bones
        }


        private void UpdateColliderPosition()
        {
            //if we are grabbing something, disable?

            Vector3 targetPosition = GetTargetPosition();
            Quaternion targetRotation = GetTargetRotation();
            handCollider.UpdatePosition(targetPosition, targetRotation);

            //if handcollider lags to far behind (stuck behind something), teleport it
            if ((handCollider.transform.position - targetPosition).sqrMagnitude > resetDistance * resetDistance)
                handCollider.ResetPosition(targetPosition, targetRotation);
        }


        private Vector3 GetTargetPosition()
        {
            return this.transform.TransformPoint(handColliderOffset);
        }

        private Quaternion GetTargetRotation()
        {
            return this.transform.rotation * Quaternion.Euler(handColliderRotationOffset);
        }
    }
}
