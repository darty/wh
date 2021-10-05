using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    /// <summary>
    /// based on Valve.VR.InteractionSystem.TeleportPoint
    /// </summary>
    public class PGVRTeleportPoint : PGVRTeleportPointBase
    {
        public MeshRenderer markerMesh;
        public Transform lookAtIcon;
        public string meshColorName;
        private int meshColorID;
        private Color meshColor;

        #region lifecycle

        protected virtual void Awake()
        {
            meshColorID = Shader.PropertyToID(meshColorName);
        }

        #endregion


        #region graphics

        public void Hide()
        {
            markerMesh.enabled = false;
            lookAtIcon.gameObject.SetActive(false);
        }


        public override void SetAlpha(float alpha)
        {
            meshColor = markerMesh.material.GetColor(meshColorID);
            meshColor.a = alpha;
            markerMesh.material.SetColor(meshColorID, meshColor);
        }


        public override void UpdateVisuals()
        {
            //Debug.Log("PGVRTeleportPoint UpdateVisuals: " + this.name + " locked = " + locked.ToString(), this.gameObject);

            if (isHighlighted)
            {
                if (locked)
                {
                    SetMeshMaterials(settings.PointLockedMaterial);
                }
                else
                {
                    SetMeshMaterials(settings.PointHighlightedMaterial);
                }
            }
            else
            {
                SetMeshMaterials(settings.PointVisibleMaterial);
            }
        }


        private void SetMeshMaterials(Material material)
        {
            markerMesh.material = material;
        }

        #endregion
    }
}
