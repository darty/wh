using PGLibrary.Math;
using PGLibrary.PGVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace PGLibrary.PGVRSteam
{
    public class PGVRHandSkeleton_SteamVR : PGVRHandSkeleton
    {
        public float blendToDefaultPoseTime = 0.1f;
        public SteamVR_Behaviour_Pose pose;
        private SteamVR_Behaviour_Skeleton skeleton;        //references the hand skeleton. This is filled in once the RenderModel is loaded
        private SteamVR_Skeleton_Poser currentSkeletonPoser;    //references the SkeletonPoser on the current grabbed object
        private Vector3 skeletonPositionOffset;        //offset of skeleton to grabbedobject (in grabbedobject local space)
        private Quaternion skeletonRotationOffset;


        [Header("Advanced settings")]
        public bool reparentOnModelLoaded = false;
        public Transform newParent;


        protected override void OnEnable()
        {
            base.OnEnable();

            pose.onTransformUpdatedEvent += OnTransformUpdated;
        }


        protected override void OnDisable()
        {
            base.OnDisable();

            pose.onTransformUpdatedEvent -= OnTransformUpdated;
        }



        protected override void HandRenderer_OnModelLoaded(object sender, EventArgs e)
        {
            Debug.Log("PGVRHandSkeleton_SteamVR HandRenderer_OnModelLoaded HandRenderer_OnModelLoaded HandRenderer_OnModelLoaded");

            base.HandRenderer_OnModelLoaded(sender, e);

            skeleton = ((PGVRHandRenderer)sender).HandInstance.GetComponent<RenderModel>().GetSkeleton();

            if (reparentOnModelLoaded)
                this.transform.SetParent(newParent);
        }


        public override void GrabObject(PGVRInteractableController.GrabbedInteractableObject grabbedInteractableObject)
        {
            base.GrabObject(grabbedInteractableObject);

            //SnapOnAttach? Actually grab the object
            if (grabbedInteractableObject.grabbingFlags.HasFlag(PGVRGrabbingFlags.SnapOnAttach))
            {
                //with pose?
                if (grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.HasGrabPose)
                {

                    /*
                    * snap position?
                    * implement this method in SteamVR_Skeleton_Poser:
                    * public void ForceNextUpdate()
                    * {
                    * poseUpdatedThisFrame = false;
                    * }
                    * 
                    * because of bug in SteamVR_Skeleton_Poser cannot get a PoseSnapshot for this hand if we don't set its poseUpdatedThisFrame to false
                    * see bug report:
                    * https://github.com/ValveSoftware/steamvr_unity_plugin/issues/503
                    * if this bug is fixed we can safely remove this code
                    */
                    PGVRInteractableObject_GrabPose pose = grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.GrabPose;
                    currentSkeletonPoser = grabbedInteractableObject.interactableObject.GetComponent<SteamVR_Skeleton_Poser>();
                    //currentSkeletonPoser.ForceNextUpdate();


                    //snap the object to the center of the attach point
                    grabbedInteractableObject.attachedGO.transform.position = this.transform.TransformPoint(pose.GetPosePosition(skeleton));
                    grabbedInteractableObject.attachedGO.transform.rotation = this.transform.rotation * pose.GetPoseRotation(skeleton);

                    //get skeleton offset in final position
                    skeletonPositionOffset = grabbedInteractableObject.attachedGO.transform.InverseTransformPoint(skeleton.transform.position);
                    skeletonRotationOffset = Quaternion.Inverse(grabbedInteractableObject.attachedGO.transform.rotation) * skeleton.transform.rotation;

                    //start blending to pose
                    //skeleton.SetTemporaryRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
                    //skeleton.SetRangeOfMotion(EVRSkeletalMotionRange.WithoutController, 1f);
                    //// skeleton.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
                    skeleton.BlendToPoser(pose.GetSkeletonPoser());
                    //skeleton.SetTemporaryRangeOfMotion(EVRSkeletalMotionRange.WithoutController);
                }
                else
                {
                    //snap the object to the center of the attach point
                    grabbedInteractableObject.attachedGO.transform.position = grabbedInteractableObject.attachmentPointTransform.position;
                    grabbedInteractableObject.attachedGO.transform.rotation = grabbedInteractableObject.attachmentPointTransform.rotation;
                }
            }
            else
            {
                //no snapOnAttach, don't grab it, but do blend the skeleton to the grabbing pose
                //with pose?
                if (grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.HasGrabPose)
                {
                    PGVRInteractableObject_GrabPose pose = grabbedInteractableObject.interactableObject.InteractableObjectGrabbable.GrabPose;
                    currentSkeletonPoser = grabbedInteractableObject.interactableObject.GetComponent<SteamVR_Skeleton_Poser>();
                    skeleton.BlendToPoser(pose.GetSkeletonPoser());
                }
            }

            initialPositionOffset = attachmentPointTransform.InverseTransformPoint(currentGrabbedGO.transform.position);
            initialRotiationOffset = Quaternion.Inverse(attachmentPointTransform.rotation) * currentGrabbedGO.transform.rotation;
        }





        public override void ReleaseObject()
        {
            base.ReleaseObject();
            RestorePose();

            if(handFollowTransform)
            {
                handRenderer.transform.localPosition = Vector3.zero;
                handRenderer.transform.localRotation = Quaternion.identity;
            }
        }


        private void RestorePose()
        {
            //restore pose
            if (skeleton && currentSkeletonPoser)
            {
                // skeleton.SetRangeOfMotion(EVRSkeletalMotionRange.WithController, 1f);
                /////// skeleton.rangeOfMotion = EVRSkeletalMotionRange.WithController;
                skeleton.BlendToSkeleton(blendToDefaultPoseTime);
                //skeleton.ResetTemporaryRangeOfMotion();

                currentSkeletonPoser = null;
            }
        }


        public override Vector3 GetFinalObjectPosition(PGVRInteractableController.GrabbedInteractableObject grabbedInteractableObject)
        {
            Vector3 finalPosePosWorld;

            //getting final position from GetBlendedPose doesn't work yet, but not needed?
            /*
            if (skeleton && currentSkeletonPoser)
            {
                Vector3 finalPosePos = currentSkeletonPoser.GetBlendedPose(skeleton).position;
                finalPosePosWorld = skeleton.transform.TransformPoint(finalPosePos);
            }
            else
            {
                finalPosePosWorld = grabbedInteractableObject.attachmentPointTransform.TransformPoint(grabbedInteractableObject.endPositionOffset);
            }
            */

            finalPosePosWorld = grabbedInteractableObject.attachmentPointTransform.TransformPoint(grabbedInteractableObject.endPositionOffset);

            return finalPosePosWorld;
        }


        public override Quaternion GetFinalObjectRotation(PGVRInteractableController.GrabbedInteractableObject grabbedInteractableObject)
        {

            //return Quaternion.identity;

            Quaternion finalPoseRotWorld;

            //getting final rotation from GetBlendedPose doesn't work yet, but not needed?
            /*
            if (skeleton && currentSkeletonPoser)
            {
                Quaternion finalPoseRot = currentSkeletonPoser.GetBlendedPose(skeleton).rotation;
                finalPoseRotWorld = skeleton.transform.rotation * finalPoseRot;
            }
            else
            {
                finalPoseRotWorld = grabbedInteractableObject.attachmentPointTransform.rotation* grabbedInteractableObject.endRotationOffset;
            }
            */

            finalPoseRotWorld = grabbedInteractableObject.attachmentPointTransform.rotation * grabbedInteractableObject.endRotationOffset;

            return finalPoseRotWorld;
        }



        public override void UpdateGrab()
        {
            base.UpdateGrab();
            //UpdateHandFollow();
        }


        protected void UpdateHandFollow()
        {
            /*
            Debug.Log("UpdateHandFollow");
            if (currentGrabbedGO == null)
                Debug.Log("currentGrabbedGO NULL");
            if (!handFollowTransform)
                Debug.Log("!handFollowTransform");
            if (skeleton == null)
                Debug.Log("skeleton NULL");*/
                

            /*
            if (currentGrabbedGO == null || !handFollowTransform || skeleton == null )
                return;
                */
            if (currentGrabbedGO == null || !handFollowTransform)
                return;

            SteamVR_Skeleton_PoseSnapshot pose = null;
            Vector3 targetHandPosition;
            Quaternion targetHandRotation;

            if (currentSkeletonPoser != null && skeleton != null)
                pose = currentSkeletonPoser.GetBlendedPose(skeleton);
            
            if(pose == null)
            {
                //target rotation of grabbedGO is the attachmentPointTransform rotation
                Quaternion rotationOffsetLocal = Quaternion.Inverse(this.transform.rotation) * attachmentPointTransform.rotation;
                targetHandRotation = currentGrabbedGO.transform.rotation * Quaternion.Inverse(rotationOffsetLocal);
 
                Vector3 positionOffsetWorld = this.transform.position - attachmentPointTransform.position;
                Quaternion rotationDiff = handRenderer.transform.rotation * Quaternion.Inverse(this.transform.rotation);
                Vector3 positionOffsetLocal = rotationDiff * positionOffsetWorld;
                targetHandPosition = currentGrabbedGO.transform.position + positionOffsetLocal;
            }
            else
            {

                //keep this code as backup, has correct skeleton (hand) placement, but no support for blended poses
                //place the hand relative to the grabbedGO (so grabbedGO takes the lead in positioning)
                /*
                targetHandPosition = currentGrabbedGO.transform.TransformPoint(skeletonPositionOffset);
                targetHandRotation = currentGrabbedGO.transform.rotation * skeletonRotationOffset;
                */

                //a grabbedObject can move when blending between different poses

                //update grabbedGo based on the current Pose position
                //SteamVR does this by setting the transform, getting local position, and resetting the transform
                /*
                //keep previous pos/rot
                Vector3 originalPosition = currentGrabbedGO.transform.position;
                Quaternion originalRotation = currentGrabbedGO.transform.rotation;
                
                //set grabbedGO to target position
                currentGrabbedGO.transform.position = GetTargetItemPosition();
                currentGrabbedGO.transform.rotation = GetTargetItemRotation();
                
                //get local pos + rot
                Vector3 localSkeletonPos = currentGrabbedGO.transform.InverseTransformPoint(transform.position);
                Quaternion localSkeletonRot = Quaternion.Inverse(currentGrabbedGO.transform.rotation) * transform.rotation;

                //reset grabbedGo (it's not this code responsibility to move it) do it before setting targetHandPosition
                currentGrabbedGO.transform.position = originalPosition;
                currentGrabbedGO.transform.rotation = originalRotation;
                */

                //get local pos + rot withou setting the transform
                Vector3 targetItemPos = GetTargetItemPosition();
                Quaternion targetItemRot = GetTargetItemRotation();
                Vector3 localSkeletonPos = MathHelper.InverseTransformPoint(transform.position, targetItemPos, targetItemRot, currentGrabbedGO.transform.lossyScale);
                Quaternion localSkeletonRot = Quaternion.Inverse(targetItemRot) * transform.rotation;

                //get hand world pos + rot based on new local skeleton pos/rot
                targetHandPosition = currentGrabbedGO.transform.TransformPoint(localSkeletonPos);
                targetHandRotation = currentGrabbedGO.transform.rotation * localSkeletonRot;
            }

            //Set handRenderer pos and rot
            if (handRenderer != null)
            {
                handRenderer.transform.position = targetHandPosition;
                handRenderer.transform.rotation = targetHandRotation;
            }
        }

  

        public override Vector3 GetTargetItemPosition()
        {
            if (currentGrabbedGO != null && currentSkeletonPoser != null && skeleton != null)
            {
                SteamVR_Skeleton_PoseSnapshot pose = currentSkeletonPoser.GetBlendedPose(skeleton);
                Vector3 posePosition = pose.position;
                Vector3 targetItemPosition = this.transform.TransformPoint(posePosition);
                return targetItemPosition;
            }
            else
            {
                return attachmentPointTransform.TransformPoint(initialPositionOffset);
            }
        }

        public override Quaternion GetTargetItemRotation()
        {
            if (currentGrabbedGO != null && currentSkeletonPoser != null && skeleton != null)
            {
                return attachmentPointTransform.rotation * initialRotiationOffset;
            }
            else
            {
                return attachmentPointTransform.rotation * initialRotiationOffset;
            }
        }


        #region eventhandling

        protected virtual void OnTransformUpdated(SteamVR_Behaviour_Pose updatedPose, SteamVR_Input_Sources updatedSource)
        {
            //Debug.Log("PGVRHandSkeleton OnTransformUpdated: " + Time.time);
            UpdateHandFollow();
        }

        #endregion

    }
}
