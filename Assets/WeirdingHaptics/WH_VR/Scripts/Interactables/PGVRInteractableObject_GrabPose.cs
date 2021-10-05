using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public abstract class PGVRInteractableObject_GrabPose : MonoBehaviour
    {
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
                Debug.LogWarning("PGVRInteractableObject_GrabPose: cannot find PGVRInteractableObject_Grabbable");
            else
            {
                grabbable.OnGrabStarted += Grabbable_OnGrabStarted;
                grabbable.OnGrabEnded += Grabbable_OnGrabEnded;
            }
        }

        
        private void RemoveEventHandlers()
        {
            PGVRInteractableObject_Grabbable grabbable = this.GetComponent<PGVRInteractableObject_Grabbable>();
            if (grabbable == null)
                Debug.LogWarning("PGVRInteractableObject_GrabPose: cannot find PGVRInteractableObject_Grabbable");
            else
            {
                grabbable.OnGrabStarted -= Grabbable_OnGrabStarted;
                grabbable.OnGrabEnded -= Grabbable_OnGrabEnded;
            }
        }

        public abstract Valve.VR.SteamVR_Skeleton_Poser GetSkeletonPoser();
        public abstract Vector3 GetPosePosition(Valve.VR.SteamVR_Behaviour_Skeleton skeleton);
        public abstract Quaternion GetPoseRotation(Valve.VR.SteamVR_Behaviour_Skeleton skeleton);


        protected virtual void Grabbable_OnGrabStarted(object sender, GrabStartEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        protected virtual void Grabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
        {
            throw new System.NotImplementedException();
        }

    }
}
