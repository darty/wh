using Polygoat.Haptics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class TouchableHaptics : MonoBehaviour
    {
        public bool playOnStart;
        public AudioClip audioClipOnTouchStart;
        public bool playOnUpdate;
        public bool velocityControlsVolume;
        public AudioClip audioClipOnTouchUpdate;
        public bool playOnEnd;
        public AudioClip audioClipOnTouchEnd;

        private HapticClip hapticClipOnTouchUpdate;


        private void Start()
        {
            PGVRTouchable touchable = this.GetComponent<PGVRTouchable>();
            if (touchable != null)
            {
                touchable.OnTouchStarted += Touchable_OnTouchStarted;
                touchable.OnTouchUpdated += Touchable_OnTouchUpdated;
                touchable.OnTouchEnded += Touchable_OnTouchEnded;

                if(audioClipOnTouchUpdate != null)
                    hapticClipOnTouchUpdate = new HapticClip(audioClipOnTouchUpdate);
            }
        }

        private void Touchable_OnTouchStarted(object sender, TouchEventArgs e)
        {
            if (playOnStart)
                PlayHaptics(e.InteractableController, audioClipOnTouchStart);
            if (playOnUpdate)
                PlayHaptics(e.InteractableController, hapticClipOnTouchUpdate, true);
        }


        private void Touchable_OnTouchUpdated(object sender, TouchEventArgs e)
        {
            //send parameters to haptics?
            if (playOnUpdate && velocityControlsVolume)
            {
                float velocity = e.Velocity.magnitude;
                hapticClipOnTouchUpdate.SetVolumeFromVelocity(velocity);
            }
        }


        private void Touchable_OnTouchEnded(object sender, TouchEventArgs e)
        {
            if (playOnUpdate)
                StopHaptics(e.InteractableController, hapticClipOnTouchUpdate);
            if (playOnEnd)
                PlayHaptics(e.InteractableController, audioClipOnTouchEnd);
        }

   


        private void PlayHaptics(PGVRInteractableController interactableController, AudioClip audioClip, bool loop = false)
        {
            if (audioClip == null)
                return;

            HapticDevice hapticDevice = interactableController.GetComponent<HapticDevice>();
            if (hapticDevice == null)
                return;

            if (loop)
                hapticDevice.StartAudioClipLoop(audioClip);
            else
                hapticDevice.PlayAudioClip(audioClip);
        }


        private void PlayHaptics(PGVRInteractableController interactableController, HapticClip hapticClip, bool loop = false)
        {
            if (hapticClip == null)
                return;

            HapticDevice hapticDevice = interactableController.GetComponent<HapticDevice>();
            if (hapticDevice == null)
                return;

            if (loop)
                hapticDevice.StartHapticClipLoop(hapticClip);
            else
                hapticDevice.PlayHapticClip(hapticClip);
        }



        private void StopHaptics(PGVRInteractableController interactableController, HapticClip hapticClip)
        {
            if (hapticClip == null)
                return;

            HapticDevice hapticDevice = interactableController.GetComponent<HapticDevice>();
            if (hapticDevice == null)
                return;

            hapticDevice.StopHapticClipLoop(hapticClip);
        }

    }
}
