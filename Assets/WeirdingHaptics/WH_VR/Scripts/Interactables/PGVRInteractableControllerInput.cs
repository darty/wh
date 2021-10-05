using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    /// <summary>
    /// abstraction layer for PGVR Interaction System
    /// </summary>
    public abstract class PGVRInteractableControllerInput : MonoBehaviour
    {
        #region controls

        /// <summary>
        /// returns true if this is the left hand (false if right)
        /// </summary>
        /// <returns></returns>
        public abstract bool IsLeftHand();

        /// <summary>
        /// returns true on the frame GrabPinch went from state up to down
        /// </summary>
        /// <returns></returns>
        public abstract bool IsGrabPinchDown();

        /// <summary>
        /// returns true on the frame GrabGrib went from state up to down
        /// </summary>
        /// <returns></returns>
        public abstract bool IsGrabGripDown();

        /// <summary>
        /// returns true on each frame GrabPinch is down
        /// </summary>
        /// <returns></returns>
        public abstract bool IsGrabbingPinch();

        /// <summary>
        /// returns true on each frame GrabGrib is down
        /// </summary>
        /// <returns></returns>
        public abstract bool IsGrabbingGrip();



        /// <summary>
        /// returns true on the frame Teleport went from state up to down
        /// </summary>
        /// <returns></returns>
        public abstract bool IsTeleporting();



        /// <summary>
        /// returns true on while the record button is pressed
        /// </summary>
        /// <returns></returns>
        public abstract bool IsRecording();


        /// <summary>
        /// returns true whether the switch button mode was clicked
        /// </summary>
        /// <returns></returns>
        public abstract bool IsSwitchButtonClicked();



        /// <summary>
        /// returns true whether the switch actuator button was clicked
        /// </summary>
        /// <returns></returns>
        public abstract bool IsSwitchActuatorClicked();

        #endregion


        #region haptics

        public virtual void TriggerHapticPulse(ushort microSecondsDuration)
        {
            float seconds = (float)microSecondsDuration / 1000000f;
            Vibrate(seconds, 1f / seconds, 1f);
        }

        public virtual void Vibrate(float duration, float frequency, float amplitude)
        {

        }

        #endregion


        #region velocity

        /// <summary>
        /// get estimation of controller velovity and angularvelocity
        /// </summary>
        /// <returns></returns>
        public abstract void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity);

        #endregion


    }
}
