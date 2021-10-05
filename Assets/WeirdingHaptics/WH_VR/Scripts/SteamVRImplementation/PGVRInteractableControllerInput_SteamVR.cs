using PGLibrary.PGVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace PGLibrary.PGVRSteam
{
    public class PGVRInteractableControllerInput_SteamVR : PGVRInteractableControllerInput
    {
        public SteamVR_Behaviour_Pose trackedObject;
        public SteamVR_Input_Sources handType;

        public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
        public SteamVR_Action_Boolean grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
        public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");

        public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        public SteamVR_Action_Boolean recordAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("RecordHaptics");
        public SteamVR_Action_Boolean switchModeAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SwitchMode");
        public SteamVR_Action_Boolean switchActuatorAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SwitchActuator");

        #region controls

        public override bool IsLeftHand()
        {
            return handType == SteamVR_Input_Sources.LeftHand;
        }

        public override bool IsGrabbingGrip()
        {
            //Debug.Log("IsGrabbingGrip? " + grabGripAction.GetState(handType).ToString());

            return grabGripAction.GetState(handType);
        }

        public override bool IsGrabbingPinch()
        {
            //Debug.Log("IsGrabbingPinch? " + grabGripAction.GetState(handType).ToString());

            return grabPinchAction.GetState(handType);
        }

        public override bool IsGrabGripDown()
        {
            //Debug.Log("IsGrabGripDown? " + grabGripAction.GetStateDown(handType).ToString());

            return grabGripAction.GetStateDown(handType);
        }

        public override bool IsGrabPinchDown()
        {
            //Debug.Log("IsGrabGripDown? " + grabPinchAction.GetStateDown(handType).ToString());

            return grabPinchAction.GetStateDown(handType);
        }

        public override bool IsTeleporting()
        {
            /*
            Debug.Log("IsTeleporting? " + teleportAction.GetState(handType));
            Debug.Log("IsGrabbingPinch? " + grabPinchAction.GetState(handType));
            Debug.Log("IsGrabbingGrip? " + grabGripAction.GetState(handType));*/
            return teleportAction.GetState(handType);
        }


        // only triggered by left hand (BUG - right hand for now)
        public override bool IsRecording()
        {
            //Debug.Log("Recording " + recordAction.GetStateDown(handType));
            return recordAction.GetState(handType);
        }

        // only triggered by left hand (BUG - right hand for now)
        public override bool IsSwitchButtonClicked()
        {
            //Debug.Log("Switch " + switchModeAction.GetState(handType));
            return switchModeAction.GetStateUp(handType);
        }


        public override bool IsSwitchActuatorClicked()
        {
            return switchActuatorAction.GetStateUp(handType);
        }


        #endregion


        #region haptics

        public override void Vibrate(float duration, float frequency, float amplitude)
        {
            hapticAction.Execute(0, duration, frequency, amplitude, handType);
        }

        #endregion


        #region velocity

        public override void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            trackedObject.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
        }

        #endregion


    }
}
