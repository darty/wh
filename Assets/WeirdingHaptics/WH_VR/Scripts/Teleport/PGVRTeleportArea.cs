using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRTeleportArea : PGVRTeleportPointBase
    {
        public MeshRenderer areaMesh;
        public string meshColorName;
        private int meshColorID;
        private Color meshColor;


        #region lifecycle

        protected virtual void Awake()
        {
            meshColorID = Shader.PropertyToID(meshColorName);
        }

        #endregion


        public override bool ShowPositionReticle()
        {
            return true;
        }

        public override bool ShowIfCurrentTeleportPoint()
        {
            return true;
        }


        public override void SetAlpha(float alpha)
        {
            meshColor = areaMesh.material.GetColor(meshColorID);
            meshColor.a = alpha;
            areaMesh.material.SetColor(meshColorID, meshColor);
        }


        public override void UpdateVisuals()
        {
            if (isHighlighted)
            {
                if (locked)
                {
                    SetMeshMaterials(settings.AreaLockedMaterial);
                }
                else
                {
                    SetMeshMaterials(settings.AreaHighlightedMaterial);
                }
            }
            else
            {
                SetMeshMaterials(settings.AreaVisibleMaterial);
            }
        }


        private void SetMeshMaterials(Material material)
        {
            areaMesh.material = material;
        }

    }
}
