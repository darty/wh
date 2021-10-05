using PGLibrary.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace PGLibrary.PGVR
{
    /// <summary>
    /// Teleporting, based on Valve.VR.InteractionSystem.Teleport
    /// </summary>
    public class PGVRTeleport : MonoBehaviour
    {
        [Header("General")]
        public PGVRPlayer pgvrPlayer;
        public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");
        public PGVRInteractableController[] interactableControllers;
        public bool IsTeleporting { get; protected set; }
        private PGVRInteractableController currentTeleportController;
        private PGVRTeleportPointBase[] teleportPoints;
        private PGVRTeleportPointBase currentPointedTeleportPoint;      //TeleportPoint we are currently aiming at
        private Vector3 currentPointedPosition;                         //position we are currently aiming at;
        public PGVRTeleportPointBase CurrentTeleportPoint { get; protected set; }            //TeleportPoint we are on
        private bool isPointerVisible = false;
        private float pointerShowStartTime;
        public LayerMask traceLayerMask;

        //fade
        public float teleportFadeTime = 0.1f;

        [Header("Graphics")]
        public Transform destinationReticleTransform;
        public Transform invalidReticleTransform;
        public GameObject teleportPointerObject;
        private PGVRTeleportArc teleportArc;
        private const float arcDistance = 10.0f;
        public Color pointerValidColor;
        public Color pointerInvalidColor;
        public Color pointerLockedColor;

        [Header("Chaperone")]
        public bool showPlayAreaMarker;
        public GameObject playAreaPreviewCorner;
        public GameObject playAreaPreviewSide;
        private Transform playAreaPreviewTransform;
        private Transform[] playAreaPreviewCorners;
        private Transform[] playAreaPreviewSides;
        SteamVR_Events.Action chaperoneInfoInitializedAction;

        //const
        private const ushort validHapticDuration = 800;
        private const float teleportPointMeshFadeTime = 0.2f;

        #region lifecycle

        private void Awake()
        {
            teleportArc = GetComponent<PGVRTeleportArc>();
            if (teleportArc == null)
                Debug.LogError("teleportarc is null ", this.gameObject);
            teleportArc.traceLayerMask = traceLayerMask;

            chaperoneInfoInitializedAction = ChaperoneInfo.InitializedAction(OnChaperoneInfoInitialized);

            //get all teleportpoints in this scene (dirty way, optimize later)
            teleportPoints = GameObject.FindObjectsOfType<PGVRTeleportPointBase>();
        }

        private void Start()
        {
            //get all teleportpoints in this scene (dirty way, optimize later), initialize them
            //teleportPoints = GameObject.FindObjectsOfType<PGVRTeleportPointBase>();
            //initi teleportpoints
            foreach (PGVRTeleportPointBase teleportPointBase in teleportPoints)
            {
                teleportPointBase.Initialize();
            }

            HideTeleportPoints();
        }


        //-------------------------------------------------
        private void OnEnable()
        {
            if (chaperoneInfoInitializedAction != null)
                chaperoneInfoInitializedAction.enabled = true;
            OnChaperoneInfoInitialized(); // In case it's already initialized
        }


        //-------------------------------------------------
        private void OnDisable()
        {
            if(chaperoneInfoInitializedAction != null)
                chaperoneInfoInitializedAction.enabled = false;
            //HidePointer();
            HideTeleportPoints();
        }


        private void Update()
        {
            foreach(PGVRInteractableController controller in interactableControllers)
            {
                //button down?
                bool eligibileTeleportButtonDown = false;
                if (IsTeleportButtonDown(controller))
                {
                    if (IsEligibleForTeleport(controller))
                    {
                        eligibileTeleportButtonDown = true;
                        //start showing teleportpoint and marker
                        ShowTeleportPoints();
                        ShowTeleportPointer(controller);

                        UpdateTeleportPointColors();
                    }
                }


                //button up?
                if (!eligibileTeleportButtonDown)
                {
                    //was this the pressed controller?
                    if (controller != currentTeleportController)
                        continue;

                    if (IsEligibleForTeleport(controller))
                    {
                        //released while pointing at a teleportpoint?
                        if (currentPointedTeleportPoint != null)
                        {
                            TryTeleportPlayer(currentPointedTeleportPoint);
                        }
                    }

                    //reset current teleportpoint
                    ResetCurrentPointedTeleportPoint();

                    //hide pointer
                    HideTeleportPointer(controller);
                    //hide teleportpoints
                    HideTeleportPoints();
                }
            }

            if(isPointerVisible)
            {
                UpdateTeleportPointer();
            }

        }

        #endregion


        #region input

        //-------------------------------------------------
        private bool IsTeleportButtonDown(PGVRInteractableController interactableController)
        {
            //just return button state, be sure to check for teleport eligibility when using this
            return interactableController.controllerInput.IsTeleporting();

            /*
            if (IsEligibleForTeleport(interactableController))
            {
                return interactableController.controllerInput.IsTeleporting();
            }

            return false;
            */
        }


        public bool IsEligibleForTeleport(PGVRInteractableController interactableController)
        {
            if (interactableController == null)
            {
                return false;
            }

            if (!interactableController.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (interactableController.IsHoveringInteractable())
            {
                return false;
            }

            if (interactableController.HasGrabbedInteractableObject())
            {
                return false;
            }

            return true;
        }

        #endregion

        #region pointer graphics



        private void ShowTeleportPointer(PGVRInteractableController interactableController)
        {
            if (!isPointerVisible)
            {
                //Debug.Log("ShowTeleportPointer");
                isPointerVisible = true;
                currentTeleportController = interactableController;
                pointerShowStartTime = Time.time;

                //show teleport pointer and arc
                teleportPointerObject.SetActive(false);
                teleportArc.Show();
            }


        }


        private void HideTeleportPointer(PGVRInteractableController interactableController)
        {
            if(isPointerVisible)
            {
                //Debug.Log("HideTeleportPointer");
                isPointerVisible = false;
                currentTeleportController = null;

                //hide teleport pointers and arc
                teleportPointerObject.SetActive(false);
                destinationReticleTransform.gameObject.SetActive(false);
                invalidReticleTransform.gameObject.SetActive(false);
                teleportArc.Hide();

                if (playAreaPreviewTransform)
                {
                    playAreaPreviewTransform.gameObject.SetActive(false);
                }
            }
        }


        private void UpdateTeleportPointer()
        {
            //Set arc cariables
            Vector3 pointerStart = currentTeleportController.transform.position;
            Vector3 pointerEnd;
            Vector3 pointerDir = currentTeleportController.transform.forward;
            Vector3 arcVelocity = pointerDir * arcDistance;

            //Check pointer angle
            float dotUp = Vector3.Dot(pointerDir, Vector3.up);
            float dotForward = Vector3.Dot(pointerDir, pgvrPlayer.HmdTransform.forward);
            bool pointerAtBadAngle = false;
            if ((dotForward > 0 && dotUp > 0.75f) || (dotForward < 0.0f && dotUp > 0.5f))
            {
                pointerAtBadAngle = true;
            }

            //Trace to see if the pointer hit anything
            PGVRTeleportPointBase hitTeleportPoint = null;
            RaycastHit hitInfo;

            teleportArc.SetArcData(pointerStart, arcVelocity, true, pointerAtBadAngle);
            if (teleportArc.DrawArc(out hitInfo))
            {
                //hit something
                pointerEnd = hitInfo.point;
                //hitSomething = true;
                if (!pointerAtBadAngle)
                {
                    hitTeleportPoint = hitInfo.collider.GetComponentInParent<PGVRTeleportPointBase>();
                    currentPointedPosition = hitInfo.point;
                }
            }
            else
            {
                //hit nothing
                pointerEnd = teleportArc.GetArcPositionAtTime(teleportArc.arcDuration);
            }

            //position markers
            PositionMarkers(pointerEnd);

            //did we exit previous teleportpoint?
            if (currentPointedTeleportPoint != null && currentPointedTeleportPoint != hitTeleportPoint)
            {
                //exit pointer
                currentPointedTeleportPoint.StopHighlight();
            }

            //show markers
            if (hitTeleportPoint != null)
                PointerHitMarker(hitTeleportPoint, currentPointedPosition);
            else
                PointerHitNoMarker();

            //keep current teleportpoint
            currentPointedTeleportPoint = hitTeleportPoint;
        }


        private void PointerHitMarker(PGVRTeleportPointBase hitTeleportPoint, Vector3 hitPosition)
        {
            //set destination markers
            destinationReticleTransform.gameObject.SetActive(false);
            invalidReticleTransform.gameObject.SetActive(false);

            if(currentPointedTeleportPoint != hitTeleportPoint)
            {
                if (hitTeleportPoint.locked)
                {
                    //Set arc color
                    teleportArc.SetColor(pointerLockedColor);
                    //start highlight
                    hitTeleportPoint.StartLockedHighlight();
                }
                else
                {
                    //Set arc color
                    teleportArc.SetColor(pointerValidColor);
                    //start highlight
                    hitTeleportPoint.StartHighlight();
                }

                //hit new teleportpoint, start haptic feedback
                currentTeleportController.controllerInput.TriggerHapticPulse(validHapticDuration);
            }

            //show position reticle?
            destinationReticleTransform.gameObject.SetActive(hitTeleportPoint.ShowPositionReticle());


            //show play area?
            if (showPlayAreaMarker)
                ShowPlayArea(hitTeleportPoint, hitPosition);
        }


        private void ShowPlayArea(PGVRTeleportPointBase hitTeleportPoint, Vector3 hitPosition)
        {
            //is this a TeleportArea?
            PGVRTeleportArea teleportArea = hitTeleportPoint as PGVRTeleportArea;
            if (!teleportArea || teleportArea.locked)
                return;

            Vector3 playerFeetOffset = pgvrPlayer.trackingOriginTransform.position - pgvrPlayer.FeetPositionGuess;

            //position and show preview
            if (playAreaPreviewTransform)
            {
                playAreaPreviewTransform.position = hitPosition + playerFeetOffset;
                playAreaPreviewTransform.gameObject.SetActive(true);
            }
        }


        private void HidePlayArea()
        {
            playAreaPreviewTransform?.gameObject?.SetActive(false);
        }


        private void HighlightSelectedTeleportPoint(PGVRTeleportPointBase hitTeleportPoint)
        {

        }


         private void PointerHitNoMarker()
        {
            //Set arc color
            teleportArc.SetColor(pointerInvalidColor);

            //set destination markers
            destinationReticleTransform.gameObject.SetActive(false);
            invalidReticleTransform.gameObject.SetActive(true);

            HidePlayArea();
        }

        private void PositionMarkers(Vector3 position)
        {
            destinationReticleTransform.position = position;
            invalidReticleTransform.position = position;
        }

        #endregion


        #region teleportpoints

        private void UpdateTeleportPointColors()
        {
            float deltaTime = Time.time - pointerShowStartTime;
            float meshAlpha = 0f;
            if (deltaTime > teleportPointMeshFadeTime)
            {
                meshAlpha = 1.0f;
                //meshFading = false;
            }
            else
            {
                float lerp = MathHelper.Normalize(Time.time, pointerShowStartTime, pointerShowStartTime + teleportPointMeshFadeTime);
                meshAlpha = Mathf.Lerp(0.0f, 1.0f, lerp);
            }

            //Tint color for the teleport points
            foreach (PGVRTeleportPointBase teleportPoint in teleportPoints)
            {
                teleportPoint.SetAlpha(meshAlpha);
            }
        }


        private void ShowTeleportPoints()
        {
            SetTeleportPoints(true);
        }


        private void HideTeleportPoints()
        {
            SetTeleportPoints(false);
        }


        private void SetTeleportPoints(bool active)
        {
            foreach (PGVRTeleportPointBase teleportPointBase in teleportPoints)
            {
                //is this the current teleportpoint?
                if (CurrentTeleportPoint != null && teleportPointBase == CurrentTeleportPoint)
                {
                    if (teleportPointBase != null && teleportPointBase.gameObject != null)
                    {
                        bool showCurrentPoint = active;
                        if (active)
                            showCurrentPoint = teleportPointBase.ShowIfCurrentTeleportPoint();
                        teleportPointBase.gameObject.SetActive(showCurrentPoint);
                    }
                }
                else
                {
                    if (teleportPointBase != null && teleportPointBase.gameObject != null)
                        teleportPointBase.gameObject.SetActive(active);
                }
            }
        }


        private void ResetCurrentPointedTeleportPoint()
        {
            if (currentPointedTeleportPoint != null)
            {
                currentPointedTeleportPoint.StopHighlight();
                currentPointedTeleportPoint = null;
            }
        }


        #endregion

        #region teleport



        public virtual void TryTeleportPlayer(PGVRTeleportPointBase teleportPoint)
        {
            if (!IsTeleporting)
            {
                if (teleportPoint != null && !teleportPoint.locked)
                {
                    StartCoroutine(TeleportPlayer(teleportPoint));
                }
            }
        }



        protected virtual IEnumerator TeleportPlayer(PGVRTeleportPointBase teleportPoint)
        {
            //start teleport
            IsTeleporting = true;

            //fade in overlay
            SteamVR_Fade.Start(Color.clear, 0);
            SteamVR_Fade.Start(Color.black, teleportFadeTime);

            //wait
            yield return new WaitForSeconds(teleportFadeTime);

            //find teleportPosition
            Vector3 teleportPosition = teleportPoint.transform.position;

            PGVRTeleportArea teleportArea = teleportPoint as PGVRTeleportArea;
            if (teleportArea != null)
            {
                teleportPosition = currentPointedPosition;
                Vector3 playerFeetOffset = pgvrPlayer.trackingOriginTransform.position - pgvrPlayer.FeetPositionGuess;
                teleportPosition += playerFeetOffset;
            }

            //teleport
            pgvrPlayer.trackingOriginTransform.position = teleportPosition;

            //notify previous and new teleportPoint
            CurrentTeleportPoint?.PlayerLeft();
            SetCurrentTeleportPoint(teleportPoint);
            CurrentTeleportPoint?.PlayerEntered();

            //fade out overlay and wait
            SteamVR_Fade.Start(Color.clear, teleportFadeTime);

            //wait
            yield return new WaitForSeconds(teleportFadeTime);

            //teleport done
            IsTeleporting = false;
        }





        public void LeaveCurrentTeleportPoint()
        {
            CurrentTeleportPoint?.PlayerLeft();
        }


        public void SetCurrentTeleportPoint(PGVRTeleportPointBase teleportPoint)
        {
            CurrentTeleportPoint = teleportPoint;
        }

        #endregion

        #region SteamVR Chaperone

        //-------------------------------------------------
        private void OnChaperoneInfoInitialized()
        {
            ChaperoneInfo chaperone = ChaperoneInfo.instance;

            if (chaperone.initialized && chaperone.roomscale)
            {
                //Set up the render model for the play area bounds

                if (playAreaPreviewTransform == null)
                {
                    playAreaPreviewTransform = new GameObject("PlayAreaPreviewTransform").transform;
                    playAreaPreviewTransform.parent = transform;
                    Util.ResetTransform(playAreaPreviewTransform);

                    playAreaPreviewCorner.SetActive(true);
                    playAreaPreviewCorners = new Transform[4];
                    playAreaPreviewCorners[0] = playAreaPreviewCorner.transform;
                    playAreaPreviewCorners[1] = Instantiate(playAreaPreviewCorners[0]);
                    playAreaPreviewCorners[2] = Instantiate(playAreaPreviewCorners[0]);
                    playAreaPreviewCorners[3] = Instantiate(playAreaPreviewCorners[0]);

                    playAreaPreviewCorners[0].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewCorners[1].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewCorners[2].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewCorners[3].transform.parent = playAreaPreviewTransform;

                    playAreaPreviewSide.SetActive(true);
                    playAreaPreviewSides = new Transform[4];
                    playAreaPreviewSides[0] = playAreaPreviewSide.transform;
                    playAreaPreviewSides[1] = Instantiate(playAreaPreviewSides[0]);
                    playAreaPreviewSides[2] = Instantiate(playAreaPreviewSides[0]);
                    playAreaPreviewSides[3] = Instantiate(playAreaPreviewSides[0]);

                    playAreaPreviewSides[0].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewSides[1].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewSides[2].transform.parent = playAreaPreviewTransform;
                    playAreaPreviewSides[3].transform.parent = playAreaPreviewTransform;
                }

                float x = chaperone.playAreaSizeX;
                float z = chaperone.playAreaSizeZ;

                playAreaPreviewSides[0].localPosition = new Vector3(0.0f, 0.0f, 0.5f * z - 0.25f);
                playAreaPreviewSides[1].localPosition = new Vector3(0.0f, 0.0f, -0.5f * z + 0.25f);
                playAreaPreviewSides[2].localPosition = new Vector3(0.5f * x - 0.25f, 0.0f, 0.0f);
                playAreaPreviewSides[3].localPosition = new Vector3(-0.5f * x + 0.25f, 0.0f, 0.0f);

                playAreaPreviewSides[0].localScale = new Vector3(x - 0.5f, 1.0f, 1.0f);
                playAreaPreviewSides[1].localScale = new Vector3(x - 0.5f, 1.0f, 1.0f);
                playAreaPreviewSides[2].localScale = new Vector3(z - 0.5f, 1.0f, 1.0f);
                playAreaPreviewSides[3].localScale = new Vector3(z - 0.5f, 1.0f, 1.0f);

                playAreaPreviewSides[0].localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                playAreaPreviewSides[1].localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                playAreaPreviewSides[2].localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                playAreaPreviewSides[3].localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);

                playAreaPreviewCorners[0].localPosition = new Vector3(0.5f * x - 0.25f, 0.0f, 0.5f * z - 0.25f);
                playAreaPreviewCorners[1].localPosition = new Vector3(0.5f * x - 0.25f, 0.0f, -0.5f * z + 0.25f);
                playAreaPreviewCorners[2].localPosition = new Vector3(-0.5f * x + 0.25f, 0.0f, -0.5f * z + 0.25f);
                playAreaPreviewCorners[3].localPosition = new Vector3(-0.5f * x + 0.25f, 0.0f, 0.5f * z - 0.25f);

                playAreaPreviewCorners[0].localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                playAreaPreviewCorners[1].localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
                playAreaPreviewCorners[2].localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                playAreaPreviewCorners[3].localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);

                playAreaPreviewTransform.gameObject.SetActive(false);
            }
        }


        #endregion
    }
}
