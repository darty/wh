﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Polygoat.Haptics
{
    public class HapticDevice_SteamVR : HapticDevice
    {
        [Header("SteamVR")]
        public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");
        public SteamVR_Input_Sources inputSource;

        public override void TriggerPulse(float durationSeconds, float frequency, float amplitude)
        {
            if(calibration.device == AudioCalibration.Device.NativeActuator)
                hapticAction.Execute(0, durationSeconds, frequency, amplitude, inputSource);
        }

    }

}
