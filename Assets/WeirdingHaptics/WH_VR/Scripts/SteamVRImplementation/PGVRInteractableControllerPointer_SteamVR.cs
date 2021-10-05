using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace PGLibrary.PGVR
{
    public class PGVRInteractableControllerPointer_SteamVR : PGVRInteractableControllerPointer
    {
        [Header("SteamVR")]
        public SteamVR_Behaviour_Pose controller;
        public SteamVR_Action_Boolean interactUIAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "InteractUI");
        public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "Teleport");



        public override bool GetPressedReleased(out bool pressed, out bool released)
        {
            int controllerIndex = controller.GetDeviceIndex();
            if (controllerIndex == -1)
            {
                pressed = false;
                released = false;
                return false;
            }

            var action = interactUIAction[controller.inputSource];

            pressed = action.stateDown;
            released = action.stateUp;

            return true;
        }



        public override bool IsTeleportPressed()
        {
            int controllerIndex = controller.GetDeviceIndex();
            return teleportAction[controller.inputSource].state;
        }
    }
}
