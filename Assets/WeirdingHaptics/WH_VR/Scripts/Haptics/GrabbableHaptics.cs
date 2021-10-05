using Polygoat.Haptics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class GrabbableHaptics : MonoBehaviour
    {
        public bool playOnStart;
        public AudioClip audioClipOnGrabStart;
        public bool playOnUpdate;
        public bool velocityControlsVolume;
        public AudioClip audioClipOnGrabUpdate;
        public bool playOnEnd;
        public AudioClip audioClipOnGrabEnd;

        private HapticClip hapticClipOnGrabUpdate;


        private void Start()
        {
            PGVRInteractableObject_Grabbable grabbable = this.GetComponent<PGVRInteractableObject_Grabbable>();
            if(grabbable != null)
            {
                grabbable.OnGrabStarted += Grabbable_OnGrabStarted;
                grabbable.OnGrabUpdated += Grabbable_OnGrabUpdated;
                grabbable.OnGrabEnded += Grabbable_OnGrabEnded;

                if (audioClipOnGrabUpdate != null)
                    hapticClipOnGrabUpdate = new HapticClip(audioClipOnGrabUpdate);
            }
        }


        private void Grabbable_OnGrabStarted(object sender, GrabStartEventArgs e)
        {
            if(playOnStart)
                PlayHaptics(e.InteractableController, audioClipOnGrabStart);
            if(playOnUpdate)
                PlayHaptics(e.InteractableController, hapticClipOnGrabUpdate, true);
        }


        private void Grabbable_OnGrabUpdated(object sender, GrabEventArgs e)
        {
            //send parameters to haptics?
            if(playOnUpdate && velocityControlsVolume)
            {
                float velocity = e.Velocity.magnitude;
                hapticClipOnGrabUpdate.SetVolumeFromVelocity(velocity);
            }
        }


        private void Grabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
        {
            if (playOnUpdate)
                StopHaptics(e.InteractableController, hapticClipOnGrabUpdate);
            if(playOnEnd)
                PlayHaptics(e.InteractableController, audioClipOnGrabEnd);
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