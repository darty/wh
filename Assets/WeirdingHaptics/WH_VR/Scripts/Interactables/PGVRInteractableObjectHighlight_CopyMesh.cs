using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    /// <summary>
    /// Creates a copy of all (skinned)meshrenderers in the interactiveobject
    /// Same behaviour as default SteamVR
    /// </summary>
    public class PGVRInteractableObjectHighlight_CopyMesh : PGVRInteractableObjectHighlight
    {
    
        [Header("General settings")]
        public Material highlightMat;
        [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] ignoreHighlight;

        [Header("Hover")]
        [SerializeField]
        private bool highlightOnHover;
        [SerializeField]
        private string colorPropName = "_Color";
        private int colorPropId;
        public Color hoverColor;

        [Header("InRange")]
        [SerializeField]
        private bool highlightOnInRange;
        public Color inRangeColor;

        protected MeshRenderer[] highlightRenderers;
        protected MeshRenderer[] existingRenderers;
        protected SkinnedMeshRenderer[] highlightSkinnedRenderers;
        protected SkinnedMeshRenderer[] existingSkinnedRenderers;
        protected GameObject highlightHolder;

        private bool CreatedHighlightRenderers
        {
            get { return highlightRenderers != null; }
        }


        private void Awake()
        {
            colorPropId = Shader.PropertyToID(colorPropName);
        }


        #region hover

        public override void StartHighlight()
        {
            //Debug.Log("StartHighlight: " + name);

            if (!highlightOnHover)
                return;

            CreateHighlightRenderers();
            //also execute first update immediatly to force correct pos/rot/sca
            UpdateHighlightRenderers(false);
            SetColor(hoverColor);
        }


        public override void StopHighlight(bool isInGrabRange)
        {
            //Debug.Log("StopHighlight: " + name + ",  isInGrabRange = " + isInGrabRange.ToString());

            if(isInGrabRange)
                SetColor(inRangeColor);

            //only destroy if not in range anymore
            if (!highlightOnInRange || !isInGrabRange)
                DestroyHighlightRenderers();
        }


        public override void UpdateHighlight(bool isGrabbed)
        {
            UpdateHighlightRenderers(isGrabbed);
        }

        #endregion


        #region in range


        public override void StartInRangeHighlight()
        {
            //Debug.Log("StartInRangeHighlight: " + name);

            if (!highlightOnInRange)
                return;

            CreateHighlightRenderers();
            //also execute first update immediatly to force correct pos/rot/sca
            UpdateHighlightRenderers(false);
            SetColor(inRangeColor);
        }


        public override void StopInRangeHighlight()
        {
            //Debug.Log("StopInRangeHighlight: " + name);

            DestroyHighlightRenderers();
        }


        public override void UpdateInRangeHighlight(bool isGrabbed)
        {
            UpdateHighlightRenderers(isGrabbed);
        }


        #endregion


        private void CreateHighlightRenderers()
        {
            //if already created, skip
            if (CreatedHighlightRenderers)
                return;

            //create copy of skinnedmeshrenderers
            existingSkinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            highlightHolder = new GameObject("Highlighter");
            highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

            for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];

                if (ShouldIgnoreHighlight(existingSkinned.gameObject))
                    continue;

                GameObject newSkinnedHolder = new GameObject("SkinnedHolder");
                newSkinnedHolder.transform.parent = highlightHolder.transform;
                SkinnedMeshRenderer newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
                Material[] materials = new Material[existingSkinned.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = highlightMat;
                }

                newSkinned.sharedMaterials = materials;
                newSkinned.sharedMesh = existingSkinned.sharedMesh;
                newSkinned.rootBone = existingSkinned.rootBone;
                newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
                newSkinned.bones = existingSkinned.bones;

                highlightSkinnedRenderers[skinnedIndex] = newSkinned;
            }

            //create copy of meshrenderers
            MeshFilter[] existingFilters = this.GetComponentsInChildren<MeshFilter>(true);
            existingRenderers = new MeshRenderer[existingFilters.Length];
            highlightRenderers = new MeshRenderer[existingFilters.Length];

            for (int filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
            {
                MeshFilter existingFilter = existingFilters[filterIndex];
                MeshRenderer existingRenderer = existingFilter.GetComponent<MeshRenderer>();

                if (existingFilter == null || existingRenderer == null || ShouldIgnoreHighlight(existingFilter.gameObject))
                    continue;

                GameObject newFilterHolder = new GameObject("FilterHolder");
                newFilterHolder.transform.parent = highlightHolder.transform;
                MeshFilter newFilter = newFilterHolder.AddComponent<MeshFilter>();
                newFilter.sharedMesh = existingFilter.sharedMesh;
                MeshRenderer newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

                Material[] materials = new Material[existingRenderer.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = highlightMat;
                }
                newRenderer.sharedMaterials = materials;

                highlightRenderers[filterIndex] = newRenderer;
                existingRenderers[filterIndex] = existingRenderer;
            }
        }


        private void DestroyHighlightRenderers()
        {
            if(highlightHolder != null)         
                Destroy(highlightHolder);

            existingRenderers = null;
            highlightRenderers = null;
            existingSkinnedRenderers = null;
            highlightSkinnedRenderers = null;
        }

        
        private void UpdateHighlightRenderers(bool isGrabbed)
        {
            if (highlightHolder == null || existingSkinnedRenderers == null)
                return;

            //copy pos/rot/sca from existing to highlight skinnedmeshrenderer
            for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];
                SkinnedMeshRenderer highlightSkinned = highlightSkinnedRenderers[skinnedIndex];

                if (existingSkinned != null && highlightSkinned != null && isGrabbed == false)
                {
                    highlightSkinned.transform.position = existingSkinned.transform.position;
                    highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
                    highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
                    highlightSkinned.localBounds = existingSkinned.localBounds;
                    highlightSkinned.enabled = existingSkinned.enabled && existingSkinned.gameObject.activeInHierarchy;

                    int blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
                    for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                    {
                        highlightSkinned.SetBlendShapeWeight(blendShapeIndex, existingSkinned.GetBlendShapeWeight(blendShapeIndex));
                    }
                }
                else if (highlightSkinned != null)
                    highlightSkinned.enabled = false;

            }

            //copy pos/rot/sca from existing to highlight meshrenderer
            for (int rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
            {
                MeshRenderer existingRenderer = existingRenderers[rendererIndex];
                MeshRenderer highlightRenderer = highlightRenderers[rendererIndex];

                if (existingRenderer != null && highlightRenderer != null && isGrabbed == false)
                {
                    highlightRenderer.transform.position = existingRenderer.transform.position;
                    highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
                    highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
                    highlightRenderer.enabled = existingRenderer.enabled && existingRenderer.gameObject.activeInHierarchy;
                }
                else if (highlightRenderer != null)
                    highlightRenderer.enabled = false;
            }
        }


        private void SetColor(Color color)
        {
            for (int skinnedIndex = 0; skinnedIndex < highlightSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = highlightSkinnedRenderers[skinnedIndex];
                skinnedMeshRenderer.material.SetColor(colorPropId, color);
            }

            for (int rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
            {
                MeshRenderer meshRenderer = highlightRenderers[rendererIndex];
                meshRenderer.material.SetColor(colorPropId, color);
            }
        }


        private bool ShouldIgnoreHighlight(GameObject check)
        {
            for (int ignoreIndex = 0; ignoreIndex < ignoreHighlight.Length; ignoreIndex++)
            {
                if (check == ignoreHighlight[ignoreIndex])
                    return true;
            }

            return false;
        }

    }
}
