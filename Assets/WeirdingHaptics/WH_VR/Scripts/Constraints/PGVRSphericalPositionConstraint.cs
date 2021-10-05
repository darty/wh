using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class PGVRSphericalPositionConstraint : MonoBehaviour
    {
        [Header("General settings")]
        public Transform anchorPoint;
        public float rangeRadius = 0.5f;
        public float maxYawAngle = 45f;
        public float maxPitchAngle = 45f;
        public float minPullDistance = 0.15f;
        public float positionSpeed = 10f;
        public float rotationSpeed = 10f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialOffset;
        public bool rotateToAnchor = false;


        Vector3 anchorOffsetTest;
        Vector3 test;


        private void OnDrawGizmos()
        {
            if(anchorPoint != null)
                Gizmos.DrawWireSphere(anchorPoint.position, rangeRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(test, 0.1f);
            Gizmos.DrawLine(this.transform.position, this.transform.position + anchorOffsetTest);
        }

        #region lifecycle

        private void Awake()
        {
            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;
        }


        private void OnEnable()
        {
            AddEventHandlers();
        }


        private void OnDisable()
        {
            RemoveEventHandlers();
        }


        private void AddEventHandlers()
        {
            PGVRInteractableObject_Grabbable grabbable = this.GetComponent<PGVRInteractableObject_Grabbable>();
            if (grabbable == null)
                Debug.LogWarning("InteractableProjectile: cannot find PGVRInteractableObject_Grabbable");
            else
            {
                grabbable.OnGrabStarted += Grabbable_OnGrabStarted;
                grabbable.OnGrabUpdated += Grabbable_OnGrabUpdated;
                grabbable.OnGrabEnded += Grabbable_OnGrabEnded;
            }
        }


        private void RemoveEventHandlers()
        {
            PGVRInteractableObject_Grabbable grabbable = this.GetComponent<PGVRInteractableObject_Grabbable>();
            if (grabbable == null)
                Debug.LogWarning("InteractableProjectile: cannot find PGVRInteractableObject_Grabbable");
            else
            {
                grabbable.OnGrabStarted -= Grabbable_OnGrabStarted;
                grabbable.OnGrabUpdated -= Grabbable_OnGrabUpdated;
                grabbable.OnGrabEnded -= Grabbable_OnGrabEnded;
            }
        }

        #endregion


        #region events


        private void Grabbable_OnGrabStarted(object sender, GrabStartEventArgs e)
        {
            Debug.Log("PGVRSphericalPositionConstraint: Grabbable_OnGrabStarted");

            initialOffset = this.transform.position - e.InteractableController.transform.position;


            this.transform.position = initialPosition;
            this.transform.rotation = initialRotation;
        }


        private void Grabbable_OnGrabUpdated(object sender, GrabEventArgs e)
        {
            Debug.Log("Grabbable_OnGrabUpdated " + Time.time.ToString());

            UpdatePosition(e.InteractableController.transform.position);
        }


        private void Grabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
        {
            Debug.Log("PGVRSphericalPositionConstraint: Grabbable_OnGrabEnded");


        }

        #endregion


        #region constraint


        private void UpdatePosition(Vector3 controllerPosition)
        {
            Vector3 newPosition = controllerPosition + initialOffset;

            //apply constraint
            Vector3 anchorOffset = newPosition - anchorPoint.transform.position;
            float anchorDistance = anchorOffset.magnitude;
            anchorDistance = Mathf.Min(anchorDistance, rangeRadius);
           // newPosition = anchorPoint.transform.position + (anchorOffset.normalized * anchorDistance);

            //rotation
            Vector3 anchorOffsetLocal = anchorPoint.InverseTransformDirection(-anchorOffset.normalized);
            Quaternion lookRotationLocal = Quaternion.LookRotation(anchorOffsetLocal, Vector3.up);

            Vector3 lookRotationLocalEuler = lookRotationLocal.eulerAngles;
            if (lookRotationLocalEuler.x > 180f)
                lookRotationLocalEuler.x -= 360f;
            if (lookRotationLocalEuler.y > 180f)
                lookRotationLocalEuler.y -= 360f;
            lookRotationLocalEuler.y += 90f;

            //cap rotation
            lookRotationLocalEuler.x = Mathf.Clamp(lookRotationLocalEuler.x, -maxPitchAngle, maxPitchAngle);
            lookRotationLocalEuler.y = Mathf.Clamp(lookRotationLocalEuler.y, -maxYawAngle, maxYawAngle);

            lookRotationLocalEuler.x = -lookRotationLocalEuler.x;
            lookRotationLocalEuler.y += 90f;

            //get direction from lookRotationLocalEuler
            Vector3 newAnchorOffset = Quaternion.Euler(lookRotationLocalEuler) * Vector3.forward;
            Quaternion newRotation = this.transform.rotation;

            //set new position
            if (anchorDistance >= minPullDistance)
            {
                newPosition = anchorPoint.transform.position + anchorPoint.TransformDirection(newAnchorOffset * anchorDistance);

                if (rotateToAnchor)
                {
                    newRotation = Quaternion.LookRotation(anchorPoint.transform.position - this.transform.position, Vector3.up);
                }
            }
            else
            {
                newPosition = initialPosition;
                newRotation = initialRotation;
            }

            this.transform.position = Vector3.Lerp(this.transform.position, newPosition, Time.deltaTime * positionSpeed);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
        }

        public void ResetPosition()
        {
            this.transform.position = initialPosition;
            this.transform.rotation = initialRotation;
        }

        #endregion
    }
}
