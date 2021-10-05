using PGLibrary.PGVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


namespace PGLibrary.PGVRSteam
{
    
    public class PGVRInteractableObject_GrabPose_SteamVR : PGVRInteractableObject_GrabPose
    {
        public SteamVR_Skeleton_Poser skeletonPoser;


        private void Awake()
        {
            if (skeletonPoser == null)
                skeletonPoser = this.GetComponent<SteamVR_Skeleton_Poser>();
        }


        public override Vector3 GetPosePosition(Valve.VR.SteamVR_Behaviour_Skeleton skeleton)
        {
            return skeletonPoser.GetBlendedPose(skeleton).position;
        }


        public override Quaternion GetPoseRotation(Valve.VR.SteamVR_Behaviour_Skeleton skeleton)
        {
            return skeletonPoser.GetBlendedPose(skeleton).rotation;
        }


        protected override void Grabbable_OnGrabStarted(object sender, GrabStartEventArgs e)
        {
            
        }


        protected override void Grabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
        {
            
        }


        public override SteamVR_Skeleton_Poser GetSkeletonPoser()
        {
            return skeletonPoser;
        }
    }
}
