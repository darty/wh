using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class PGVRHandCollider : MonoBehaviour
    {
        public PhysicMaterial lowFrictionPhysicMaterial;

        private new Rigidbody rigidbody;



        public float minRepeatTimeSec = 5f;

        private Dictionary<PGVRTouchable, float> touchTimesDict = new Dictionary<PGVRTouchable, float>();

        public Dictionary<PGVRTouchable, TouchData> touchDict = new Dictionary<PGVRTouchable, TouchData>();

        public struct TouchData
        {
            public PGVRTouchable touchable;
            public float touchStartTime;
            public float touchEndedTime;
            public int contacts;
            public Coroutine touchRoutine;

            public TouchData(PGVRTouchable touchable, float startTime): this()
            {
                this.touchable = touchable;
                this.touchStartTime = startTime;
                this.touchEndedTime = 0f;
                this.contacts = 1;
            }

            public void AddContact()
            {
                this.contacts += 1;
            }

            public void RemoveContact()
            {
                this.contacts -= 1;
            }
        }

        private PGVRInteractableController interactableController;

        //magic numbers (from SteamVR)
        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;





        public void InitializeHandcollider(PGVRInteractableController interactableController)
        {
            this.interactableController = interactableController;
            rigidbody = GetComponent<Rigidbody>();
            SetPhysicMaterials();
        }


        private void SetPhysicMaterials()
        {
            Collider[] colliders = this.GetComponentsInChildren<Collider>();
            foreach(Collider collider in colliders)
            {
                collider.material = lowFrictionPhysicMaterial;
            }
        }


        public void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            //if we are grabbing something, disable?



            Vector3 positionDelta = (targetPosition - rigidbody.position);
            Vector3 targetVelocity = positionDelta * VelocityMagic * Time.deltaTime;

            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(rigidbody.rotation);

            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            Vector3 targetAngularVelocity = angle * axis * AngularVelocityMagic * Time.deltaTime;


            // Vector3 targetAngularVelocity = rotationDelta.eulerAngles * AngularVelocityMagic * Time.deltaTime;


            rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, targetVelocity, MaxVelocityChange);
            rigidbody.angularVelocity = Vector3.MoveTowards(rigidbody.angularVelocity, targetAngularVelocity, MaxAngularVelocityChange);
        }


        public void ResetPosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }


        #region collisions

        
        private void OnCollisionEnter(Collision collision)
        {
            PGVRTouchable touchable = collision.collider.GetComponent<PGVRTouchable>();
            if (touchable == null && collision.collider.attachedRigidbody)
                touchable = collision.collider.attachedRigidbody.GetComponent<PGVRTouchable>(); ;

            if (touchable == null)
                return;

            if(touchDict.ContainsKey(touchable))
            {
                TouchData currentTouchData = touchDict[touchable];
                currentTouchData.AddContact();

                /*
                if (Time.time < currentTrouchData.touchStartTime + minRepeatTimeSec)
                {
                    return;
                }
                */

                touchDict[touchable] = currentTouchData;
            }
            else
            {
                //register touch
                TouchData touchData = new TouchData(touchable, Time.time);
                touchData.touchRoutine = StartCoroutine(TouchRoutine(touchData));
                touchDict[touchable] = touchData;

                //inform touchable
                touchable.TouchStarted(interactableController, this.rigidbody.velocity, this.rigidbody.angularVelocity);
            }




        }



        /*
        private void OnCollisionStay(Collision collision)
        {
            Debug.Log("OnCollisionStay");
        }

    */

        private void OnCollisionExit(Collision collision)
        {
            PGVRTouchable touchable = collision.collider.GetComponent<PGVRTouchable>();
            if (touchable == null && collision.collider.attachedRigidbody)
                touchable = collision.collider.attachedRigidbody.GetComponent<PGVRTouchable>(); ;

            if (touchable == null)
                return;

            if (touchDict.ContainsKey(touchable))
            {
                TouchData currentTouchData = touchDict[touchable];
                currentTouchData.RemoveContact();
                touchDict[touchable] = currentTouchData;

                if (currentTouchData.contacts <= 0)
                {
                    //no more colliders are touching, remove this
                    //Debug.Log("REMOVE touchdata");
                    StopCoroutine(touchDict[touchable].touchRoutine);
                    touchDict.Remove(touchable);

                    //inform touchable
                    touchable.TouchEnded(interactableController, this.rigidbody.velocity, this.rigidbody.angularVelocity);
                }
            }

        }

        
        private IEnumerator TouchRoutine(TouchData touchData)
        {
            while(true)
            {
                //Debug.Log("TouchRoutine loop");
                touchData.touchable.TouchUpdated(interactableController, this.rigidbody.velocity, this.rigidbody.angularVelocity);

                yield return null;
            }
        }
       

        #endregion

    }
}
