using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRLinearConstraint : MonoBehaviour
    {
        public Transform startPosition;
        public Transform endPosition;
        public float positionSpeed = 1f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialOffset;
        
        public float LinearMapping { get; private set; }
        public float PreviousLinearMapping { get; private set; }

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
            //Debug.Log("PGVRSphericalPositionConstraint: Grabbable_OnGrabStarted");

            initialOffset = this.transform.position - e.InteractableController.transform.position;


            //this.transform.position = initialPosition;
            //this.transform.rotation = initialRotation;
        }


        private void Grabbable_OnGrabUpdated(object sender, GrabEventArgs e)
        {
            //Debug.Log("Grabbable_OnGrabUpdated " + Time.time.ToString());

            UpdatePosition(e.InteractableController.transform.position);
        }


        private void Grabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
        {
            //Debug.Log("PGVRSphericalPositionConstraint: Grabbable_OnGrabEnded");


        }

        #endregion


        #region constraint


        protected float CalculateLinearMapping(Vector3 controllerPosition)
        {
            Vector3 direction = endPosition.position - startPosition.position;
            float length = direction.magnitude;
            direction.Normalize();

            Vector3 displacement = controllerPosition - startPosition.position;

            return Vector3.Dot(displacement, direction) / length;
        }


        private void UpdatePosition(Vector3 controllerPosition)
        {
            PreviousLinearMapping = LinearMapping;
            LinearMapping = CalculateLinearMapping(controllerPosition);
            LinearMapping = Mathf.Clamp01(LinearMapping);

            Vector3 newPosition = Vector3.Lerp(startPosition.position, endPosition.position, LinearMapping);
            this.transform.position = Vector3.Lerp(this.transform.position, newPosition, Time.deltaTime * positionSpeed);
        }


        public void ResetPosition()
        {
            this.transform.position = initialPosition;
            this.transform.rotation = initialRotation;
        }

        #endregion
    }
}
