using PGLibrary.PGVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace PGLibrary.PGVRSteam
{
    public class PGVRHandRenderer_SteamVR : PGVRHandRenderer
    {

        public GameObject renderModelPrefab;
        public RenderModel MainRenderModel { get; private set; }

        [Header("Advanced settings")]
        public bool reparentOnModelLoaded = false;
        public Transform newParent;



        #region PGVRHandRenderer implementation

        public override void InitHandRenderer()
        {
            base.InitHandRenderer();

            GameObject renderModelInstance = GameObject.Instantiate(renderModelPrefab);
            renderModelInstance.layer = gameObject.layer;
            renderModelInstance.tag = gameObject.tag;
            renderModelInstance.transform.parent = this.transform;
            renderModelInstance.transform.localPosition = Vector3.zero;
            renderModelInstance.transform.localRotation = Quaternion.identity;
            renderModelInstance.transform.localScale = renderModelPrefab.transform.localScale;

            MainRenderModel = renderModelInstance.GetComponent<RenderModel>();
            MainRenderModel.onControllerLoaded += MainRenderModel_onControllerLoaded;

            Debug.Log("PGVRHandRenderer_SteamVR InitHandRenderer END", this.gameObject);
        }



        public override void ShowController(bool permanent = false)
        {
            base.ShowController(permanent);
            MainRenderModel?.SetControllerVisibility(true, permanent);
        }


        public override void HideController(bool permanent = false)
        {
            base.HideController(permanent);
            MainRenderModel?.SetControllerVisibility(false, permanent);
        }

        #endregion


        #region eventhandlers

        private void MainRenderModel_onControllerLoaded()
        {
            Debug.Log("MainRenderModel_onControllerLoaded");
            RaiseModelLoaded(MainRenderModel.gameObject);

            if (reparentOnModelLoaded)
                this.transform.SetParent(newParent);
        }

        #endregion

    }
}
