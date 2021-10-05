using PGLibrary.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    /// <summary>
    /// This class encapsulates all data regarding the player (like positions etc)
    /// </summary>
    public class PGVRPlayer : SingletonGameObject<PGVRPlayer>
    {
        [Tooltip("Virtual transform corresponding to the tracking origin. Devices are tracked relative to this.")]
        public Transform trackingOriginTransform;

        [Tooltip("List of possible transforms for the head/HMD, including the no-SteamVR fallback camera.")]
        public Transform[] hmdTransforms;

        [Tooltip("List of InteractableControllers.")]
        public PGVRInteractableController[] interactableControllers;




        /// <summary>
        /// Get the HMD transform. This might return the fallback camera transform if SteamVR is unavailable or disabled.
        /// </summary>
        public Transform HmdTransform
        {
            get
            {
                if (hmdTransforms != null)
                {
                    for (int i = 0; i < hmdTransforms.Length; i++)
                    {
                        if (hmdTransforms[i].gameObject.activeInHierarchy)
                            return hmdTransforms[i];
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Guess for the world-space direction of the player's hips/torso. This is effectively just the gaze direction projected onto the floor plane.
        /// </summary>
        public Vector3 BodyDirectionGuess
        {
            get
            {
                Transform hmd = HmdTransform;
                if (hmd)
                {
                    Vector3 direction = Vector3.ProjectOnPlane(hmd.forward, trackingOriginTransform.up);
                    if (Vector3.Dot(hmd.up, trackingOriginTransform.up) < 0.0f)
                    {
                        // The HMD is upside-down. Either
                        // -The player is bending over backwards
                        // -The player is bent over looking through their legs
                        direction = -direction;
                    }
                    return direction;
                }
                return trackingOriginTransform.forward;
            }
        }



        /// <summary>
        /// Guess for the world-space position of the player's feet, directly beneath the HMD.
        /// </summary>
        public Vector3 FeetPositionGuess
        {
            get
            {
                Transform hmd = HmdTransform;
                if (hmd)
                {
                    return trackingOriginTransform.position + Vector3.ProjectOnPlane(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
                }
                return trackingOriginTransform.position;
            }
        }


        public void SetInteractableControllers(bool enabled)
        {
            foreach(PGVRInteractableController controller in interactableControllers)
            {
                controller.CanInteract = enabled;
            }
        }


        protected override void Awake()
        {
            base.Awake();

            //#if OPENVR_XR_API && UNITY_LEGACY_INPUT_HELPERS
            if (hmdTransforms != null)
            {
                foreach (var hmd in hmdTransforms)
                {
                    if (hmd.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>() == null)
                        hmd.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                }
            }
            //#endif
        }


    }
}
