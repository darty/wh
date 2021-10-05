using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRTouchable : MonoBehaviour
    {
        //events
        public event EventHandler<TouchEventArgs> OnTouchStarted;
        public event EventHandler<TouchEventArgs> OnTouchUpdated;
        public event EventHandler<TouchEventArgs> OnTouchEnded;


        private const float minRepeatTimeSec = 1f;



        public void TouchStarted(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            //Debug.Log("TouchStarted", this.gameObject);
            OnTouchStarted?.Invoke(this, new TouchEventArgs(interactableController, velocity, angularVelocity));
        }

        public void TouchUpdated(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            //Debug.Log("TouchUpdated", this.gameObject);
            OnTouchUpdated?.Invoke(this, new TouchEventArgs(interactableController, velocity, angularVelocity));
        }


        public void TouchEnded(PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            //Debug.Log("TouchEnded", this.gameObject);
            OnTouchEnded?.Invoke(this, new TouchEventArgs(interactableController, velocity, angularVelocity));
        }

    }
}
