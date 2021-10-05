using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public abstract class PGVRInteractableControllerPointer : MonoBehaviour
    {
        [Header("General")]
        [Tooltip("Runs on the eventsystem update loop (provides sync with UI interaction)")]
        public bool runOnEventSystem;
        public bool disableOnTeleport = true;

        [Header("Raycast")]
        public float distance = 20f;
        public LayerMask layerMask;
        public QueryTriggerInteraction triggerInteraction;

        //hit information
        private bool hitSelectable;
        private Vector3 hitWorldPosition;
        private Vector3 hitWorldNormal;
        private float hitDistance;
        private RaycastHit hitInfo;
        private PGVRInteractableObject_Selectable previousSelectable;

        [Header("Graphics")]
        public float thickness = 0.002f;
        public bool drawSelectionSphere;
        public float selectionSphereRadius = 0.2f;
        public Color defaultColor;
        public Color hoverColor = Color.blue;
        public Color clickColor = Color.green;

        private MeshRenderer lineRenderer;
        private MeshRenderer selectionSphereRenderer;
        private bool lineRendererEnabled = true;

        //ui information
        private bool uiHitValid;
        private Vector3 uiHitWorldPosition;
        private Vector3 uiHitWorldNormal;
        private float uitHitDistance;


        private void Start()
        {
            CreatePointerRenderer();
        }


        private void Update()
        {
            if (runOnEventSystem)
                return;

            ExecuteUpdate();
        }


        public void UpdateEventSystem()
        {
            if (!runOnEventSystem)
                return;

            ExecuteUpdate();
        }

        private void ExecuteUpdate()
        {
            if (!lineRendererEnabled)
            {
                lineRenderer.enabled = selectionSphereRenderer.enabled = false;
                return;
            }
            if (disableOnTeleport && IsTeleportPressed())
            {
                lineRenderer.enabled = selectionSphereRenderer.enabled = false;
            }
            else
            {
                lineRenderer.enabled = selectionSphereRenderer.enabled = true;
                CastPointer();
            }

        }

        public void EnableLinepointer()
        {
            lineRendererEnabled = true;
        }

        public void DisableLinepointer()
        {
            lineRendererEnabled = false;
        }


        private void OnDisable()
        {
            if(previousSelectable)
            {
                previousSelectable.SelectionExit(this);
                previousSelectable = null;
            }
        }


        private void CreatePointerRenderer()
        {
            GameObject pointer = new GameObject();
            pointer.transform.parent = this.transform;
            pointer.transform.localPosition = Vector3.zero;
            pointer.transform.localRotation = Quaternion.identity;

            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.parent = pointer.transform;
            line.transform.localScale = new Vector3(thickness, thickness, distance);
            line.transform.localPosition = new Vector3(0f, 0f, distance/2);
            line.transform.localRotation = Quaternion.identity;
            line.RemoveComponent<BoxCollider>();

            //set materials
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", defaultColor);

            lineRenderer = line.GetComponent<MeshRenderer>();
            lineRenderer.material = newMaterial;

            if (drawSelectionSphere)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = pointer.transform;
                sphere.transform.localScale = new Vector3(selectionSphereRadius * 2f, selectionSphereRadius * 2f, selectionSphereRadius * 2f);
                sphere.RemoveComponent<Collider>();

                selectionSphereRenderer = sphere.GetComponent<MeshRenderer>();
                selectionSphereRenderer.material = newMaterial;
            }
        }


        private void SetColor(Color color)
        {
            lineRenderer.material.SetColor("_Color", color);
            selectionSphereRenderer.material.SetColor("_Color", color);
        }


        private void CastPointer()
        {
            float castdistance = distance;
            castdistance = Mathf.Min(castdistance, uitHitDistance);

            hitSelectable = Physics.Raycast(this.transform.position, this.transform.forward, out hitInfo, castdistance, layerMask, triggerInteraction);

            PGVRInteractableObject_Selectable currentSelectable = null;

            if (hitSelectable)
            {
                hitWorldPosition = hitInfo.point;
                hitWorldNormal = hitInfo.normal;
                hitDistance = hitInfo.distance;
                currentSelectable = hitInfo.collider.GetComponent<PGVRInteractableObject_Selectable>();

                if(currentSelectable == null && hitInfo.collider.attachedRigidbody != null)
                    currentSelectable = hitInfo.collider.attachedRigidbody.GetComponent<PGVRInteractableObject_Selectable>();
            }

            //update selection
            if(previousSelectable == currentSelectable)
            {
                //update
                previousSelectable?.SelectionUpdate(this);
            }
            else 
            {
                //selection ended
                if (previousSelectable != null)
                {
                    previousSelectable.SelectionExit(this);
                }

                //selection started
                if(currentSelectable != null)
                {
                    currentSelectable.SelectionStart(this);
                }
            }

            //check interaction
            if (currentSelectable)
            {
                //pressed/released button?
                bool released;
                bool pressed;
                bool readButton = GetPressedReleased(out pressed, out released);
                if (readButton)
                {
                    if(pressed)
                    {
                        currentSelectable.SelectionInteractPress(this);
                    }

                    if (released)
                    {
                        currentSelectable.SelectionInteractRelease(this);
                    }
                }
            }

            previousSelectable = currentSelectable;

            //color
            if(previousSelectable == null)
            {
                SetColor(defaultColor);
            }
            else
            {
                SetColor(hoverColor);
            }

            UpdateSelectionCursor();
        }


        private void UpdateSelectionCursor()
        {
            if (selectionSphereRenderer == null)
                return;

            if(hitSelectable || uiHitValid)
            {
                selectionSphereRenderer.gameObject.SetActive(true);
                selectionSphereRenderer.transform.position = hitWorldPosition;
                lineRenderer.transform.localPosition = new Vector3(0f, 0f, hitDistance / 2);
                lineRenderer.transform.localScale = new Vector3(thickness, thickness, hitDistance);
            }
            else
            {
                selectionSphereRenderer.gameObject.SetActive(false);
                lineRenderer.transform.localPosition = new Vector3(0f, 0f, distance / 2);
                lineRenderer.transform.localScale = new Vector3(thickness, thickness, distance);
            }
        }


        public void SetUIHit(bool validHit, Vector3 worldPosition, Vector3 worldNormal, float distance)
        {
            if(validHit)
            {
                uiHitValid = true;
                hitWorldPosition = uiHitWorldPosition = worldPosition;
                hitWorldNormal = uiHitWorldNormal = worldNormal;
                uitHitDistance = distance;
                hitDistance = uitHitDistance;
            }
            else
            {
                uiHitValid = false;
                uitHitDistance = float.MaxValue;
            }
        }


        public abstract bool GetPressedReleased(out bool pressed, out bool released);
        public abstract bool IsTeleportPressed();


    }
}
