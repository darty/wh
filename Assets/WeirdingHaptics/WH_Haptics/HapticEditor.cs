using PGLibrary.Helpers;
using PGLibrary.PGVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Polygoat.Haptics
{
    public class HapticEditor : SingletonGameObject<HapticEditor>
    {
        public SmoothingAudioInput smoothingAudioInput;

        public GameObject editModeManager;

        public bool inEditorMode;
        public Text editorModeLabel;

        public GameObject grabMenuPrefab;
        public GameObject touchMenuPrefab;

        // relates menus to a UID to enable showing and removing menus per object
        public InteractionMappingCanvas interactionMappingCanvasPrefab;
        private InteractionMappingCanvas grabInteractionCanvas;
        private Dictionary<string, List<GameObject>> grabMenus;
        private InteractionMappingCanvas touchInteractionCanvas;
        private Dictionary<string, List<GameObject>> touchMenus;
        private GameObject menuParent;
        private GameObject MenuParent
        {
            get
            {
                if (menuParent == null)
                {
                    menuParent = new GameObject("MenuParent");
                }
                return menuParent;
            }
        }

        private Vector3 offsetMappingCanvas = new Vector3(0.05f, 0f, 0f);
        private void PositionMenu(Transform t, int eventID, bool isGrabMenu)
        {
            eventID = Mathf.Clamp(eventID, 0, 5);

            // rotate menu to be able to see several at once
            t.rotation = MenuParent.transform.rotation;
            float rotation = 45f * eventID;
            float[] offsetX = { 0f, .475f, .7f, .475f, 0f };
            float[] offsetZ = { 0f, .2f, .7f, 1.2f, 1.8f };
            if (isGrabMenu)
                rotation *= -1;

            Vector3 offsetLayer = new Vector3(.35f + offsetX[eventID], .3f, -offsetZ[eventID]);
            if (isGrabMenu)
                offsetLayer.x *= -1;
            t.localPosition = offsetLayer;

            t.Rotate(new Vector3(0f, 1f, 0f), rotation);
        }


        public HapticDevice[] hapticDevices;

        //current focus
        private HapticInteractable hapticInteractable;
        private HapticData hapticData;
        private HapticEventData hapticEventData;
        private HapticInteractable.InteractionState currentInteractionState;

        // get information about user's movement to infer what kind of mapping they are using
        private Vector3 lastPosition;
        private float distanceTravelled;
        private float interactionTime;
        private List<float> velocities;
        private List<float> Velocities
        {
            get
            {
                if (velocities == null)
                    velocities = new List<float>();
                return velocities;
            }
        }

        [Header("Recording")]
        private bool recordingArmed;
        private bool sequenceRecorded; // tells whether we are done with the recording for this vocalization
        private bool stopRecording;
        private Coroutine recordingCoroutine;
        public GameObject recordingArmedFeedback;
        public GameObject recordingFeedback;
        public AudioSource recordingAudioSource;
        private AudioClip recordingClip;
        private bool isRecordingAudio;
        private Coroutine recordPositionRoutine;
        private HapticInteractable.InteractionState recordedType;

        private const int defaultLengthSec = 10;
        private const int defaultFrequency = 44100;

        public event EventHandler OnRecordingStarted;
        public event EventHandler OnRecordingStopped;

        public PGVRInteractableControllerInput rightController;
        public PGVRInteractableControllerInput leftController;


        // Start is called before the first frame update
        void Start()
        {
            grabMenus = new Dictionary<string, List<GameObject>>();
            touchMenus = new Dictionary<string, List<GameObject>>();

            if (!inEditorMode)
                rightController.GetComponent<PGVRInteractableControllerPointer>().DisableLinepointer();
        }

        // Update is called once per frame
        void Update()
        {

            if (leftController.IsSwitchButtonClicked())
            {
                if (inEditorMode)
                    EnableEditorMode(false);
                else
                    EnableEditorMode(true);
                //Debug.Log("need to switch mode");
            }

            if (inEditorMode)
            {
                // left controller buttons handlers
                if (leftController.IsRecording() && !recordingArmed)
                {
                    ArmRecording(true);
                }
                else if (!leftController.IsRecording() && recordingArmed)
                {
                    ArmRecording(false);
                }
            }

            if (rightController.IsSwitchActuatorClicked())
                GameObject.FindObjectOfType<AudioCalibration>()?.SwitchActuator();

            // recording haptics based on user's vocalization
            if (hapticInteractable != null)
            {
                // inferring mappings of vocalizations based on user's interactions
                HapticInteractable.InteractionState lastInteractionState = hapticInteractable.GetInteractionState();

                //Debug.Log("inferring record: object state " + lastInteractionState);

                if (!recordingArmed)
                {
                    // if the recording stopped while interacting with the object, we stop the recording and save data
                    if (!sequenceRecorded && currentInteractionState != HapticInteractable.InteractionState.Idle)
                    {
                        //Debug.Log("Inferring record: saving sequence data");
                        StopRecording(true);
                        StopCoroutine(recordingCoroutine);
                        ShowRecordingFeedback(false);
                        InferMappingBasedOnRecordData(currentInteractionState);
                        currentInteractionState = HapticInteractable.InteractionState.Idle;
                    }
                }
                else // only check interactions if recording is armed 
                {
                    // interaction stopped
                    if (lastInteractionState == HapticInteractable.InteractionState.Idle && currentInteractionState != lastInteractionState)
                    {
                        // if we already recorded the sequence, then nothing to do here
                        if (!sequenceRecorded)
                        {
                            //Debug.Log("Inferring record: saving sequence data");
                            InferMappingBasedOnRecordData(currentInteractionState);
                            stopRecording = true;
                            // recording coroutine will stop on its own
                        }
                        sequenceRecorded = false;
                    }

                    // user started interacting with the object
                    if (currentInteractionState == HapticInteractable.InteractionState.Idle && lastInteractionState != HapticInteractable.InteractionState.Idle)
                    {
                        // get first position of recording
                        lastPosition = rightController.transform.position;
                        distanceTravelled = 0f;
                        interactionTime = 0f;
                        Velocities.Clear();

                        if (lastInteractionState == HapticInteractable.InteractionState.Touched)
                        {
                            //Debug.Log("Inferring record: Add touch event");
                            //TODO create a touch event and start recording
                            hapticData.AddTouchEventData();
                            // record on the last EventData (the one we just added)
                            recordingCoroutine = StartCheckForRecord(hapticData.touchEventData[hapticData.touchEventData.Count - 1]);
                            recordedType = HapticInteractable.InteractionState.Touched;
                        }
                        else if (lastInteractionState == HapticInteractable.InteractionState.Grabbed)
                        {
                            //Debug.Log("Inferring record: Add grab event");
                            //TODO create a grab event and start recording
                            hapticData.AddGrabEventData();
                            // record on the last EventData (the one we just added)
                            recordingCoroutine = StartCheckForRecord(hapticData.grabEventData[hapticData.grabEventData.Count - 1]);
                            recordedType = HapticInteractable.InteractionState.Grabbed;
                        }
                    }

                    // user changed the type of interaction (touch => grab)
                    if (currentInteractionState == HapticInteractable.InteractionState.Touched && lastInteractionState == HapticInteractable.InteractionState.Grabbed)
                    {
                        //TODO stop recording the touch event and remove it, then record the grab event
                        //Debug.Log("Inferring record: Stopped recording touch events");
                        StopRecording(false);
                        if (recordingCoroutine != null) StopCoroutine(recordingCoroutine);
                        hapticData.DeleteTouchEventData(hapticData.touchEventData.Count - 1);
                        ShowRecordingFeedback(false);

                        //Debug.Log("Inferring record: Add grab event (after touch event)");
                        // add new grab event and start recording
                        hapticData.AddGrabEventData();
                        recordingCoroutine = StartCheckForRecord(hapticData.grabEventData[hapticData.grabEventData.Count - 1]);

                        recordedType = HapticInteractable.InteractionState.Grabbed;

                        // reset information about interaction
                        lastPosition = rightController.transform.position;
                        distanceTravelled = 0f;
                        interactionTime = 0f;
                        Velocities.Clear();
                    }

                    // user changed the type of interaction (grab => touch)
                    if (currentInteractionState == HapticInteractable.InteractionState.Grabbed && lastInteractionState == HapticInteractable.InteractionState.Touched)
                    {
                        //TODO stop recording the grab event and save it (no touch event should be recorded)
                        //Debug.Log("Inferring record: saving grab event data");
                        StopRecording(true);
                        if (recordingCoroutine != null) StopCoroutine(recordingCoroutine);
                        recordingCoroutine = null;
                        ShowRecordingFeedback(false);

                        // we will not record anything else after this
                        sequenceRecorded = true;

                        InferMappingBasedOnRecordData(HapticInteractable.InteractionState.Grabbed);
                    }

                    // record data about the interaction
                    if ((lastInteractionState == HapticInteractable.InteractionState.Touched || lastInteractionState == HapticInteractable.InteractionState.Grabbed)
                        && currentInteractionState == lastInteractionState)
                    {
                        //Debug.Log("Inferring record: recording interaction");
                        //TODO record interaction data
                        distanceTravelled += Vector3.Distance(rightController.transform.position, lastPosition);
                        lastPosition = rightController.transform.position;
                        interactionTime += Time.deltaTime;
                        float velocity = rightController.GetComponent<PGVRInteractableController>().EstimatedVelocity.magnitude;
                        Velocities.Add(velocity);
                        //Debug.Log("recording speed " + rightController.GetComponent<PGVRInteractableController>().EstimatedVelocity.magnitude);
                    }

                    // update to current interaction state
                    currentInteractionState = lastInteractionState;
                }
            }
        }

        private void InferMappingBasedOnRecordData(HapticInteractable.InteractionState actionType)
        {
            if (hapticEventData == null) { 
                Debug.LogError("No haptic event data");
                return;
            }

            float averageVelocity = 0f;
            foreach (float velocity in Velocities)
                averageVelocity += velocity;
            hapticEventData.recordingSpeed = averageVelocity / Velocities.Count;

            float distanceThreshold = 0.5f; // in meters 
            float timeThreshold = 2.5f; // in seconds

            //TODO need to include positional to the list below
            // if linearConstraint is not null => object can use positional mapping
            PGVRLinearConstraint linearConstraint = hapticInteractable.GetComponent<PGVRLinearConstraint>();

            // set to default as Instantaneous
            hapticEventData.hapticType = HapticType.Instantaneous;
            // positional
            if (linearConstraint != null && distanceTravelled > distanceThreshold)
            {
                hapticEventData.hapticType = HapticType.Positional;
            }
            // instantaneous
            else if (distanceTravelled <= distanceThreshold && interactionTime <= timeThreshold)
            {
                hapticEventData.hapticType = HapticType.Instantaneous;
            }
            // continuous passive
            else if (distanceTravelled <= distanceThreshold && interactionTime > timeThreshold)
            {
                hapticEventData.hapticType = HapticType.ContinuousPassive;
            }
            // continuous active
            else if (distanceTravelled > distanceThreshold && interactionTime > timeThreshold)
            {
                hapticEventData.hapticType = HapticType.ContinuousActive;
            }
        }

        private void EnableEditorMode(bool enable)
        {
            if (enable)
            {
                editModeManager.GetComponent<EditModeManager>().EnableEditMode();
                EnableSelectRayOnController(true);
            }
            else
            {
                editModeManager.GetComponent<EditModeManager>().DisableEditMode();
                DisableSelectRayOnController(true);
                if (hapticInteractable)
                    HideMenus(hapticInteractable.Uid);
            }

            inEditorMode = enable;
            editorModeLabel.text = enable ? "Edit Mode" : "Play Mode";
        }


        public void EnableSelectRayOnController(bool forceChange = false)
        {
            if (forceChange || inEditorMode)
                rightController.GetComponent<PGVRInteractableControllerPointer_SteamVR>().EnableLinepointer();
        }


        public void DisableSelectRayOnController(bool forceChange = false)
        {
            if (forceChange || inEditorMode)
                rightController.GetComponent<PGVRInteractableControllerPointer_SteamVR>().DisableLinepointer();
        }


        private void ArmRecording(bool arm)
        {
            recordingArmed = arm;
            recordingArmedFeedback.SetActive(arm);
            // remove recording feedback if recording button is not pressed anymore
            if (recordingFeedback.activeSelf)
                recordingFeedback.SetActive(false);
            // hide menus if user's ready to record haptics
            if (arm && hapticInteractable != null)
            {
                hapticInteractable.ForceUnselection();
                HideMenus(hapticInteractable.Uid);

            }
        }

        private void ShowRecordingFeedback(bool show)
        {
            if (show)
                recordingArmedFeedback.SetActive(false);
            else if (recordingArmed)
                recordingArmedFeedback.SetActive(true);
            recordingFeedback.SetActive(show);
        }


        // called by an interactable object to set the focus on it
        public void TargetInteractable(HapticInteractable hapticInteractable, HapticData hapticData)
        {
            // do not allow changes when recording audio
            if (isRecordingAudio) return;

            if (this.hapticInteractable != null && this.hapticInteractable.Uid != hapticInteractable.Uid)
            {
                this.hapticInteractable.ForceUnselection();
                HideMenus(this.hapticInteractable.Uid);
            }

            this.hapticInteractable = hapticInteractable;
            this.hapticData = hapticData;

            CheckHapticData();
        }



        #region UI


        public void HideMenus(string uid)
        {
            if (grabMenus == null || touchMenus == null || (!grabMenus.ContainsKey(uid) && !touchMenus.ContainsKey(uid)))
            {
                return;
            }

            if (grabMenus.ContainsKey(uid))
            {
                if (grabInteractionCanvas) Destroy(grabInteractionCanvas.gameObject);
                foreach (GameObject menu in grabMenus[uid])
                    Destroy(menu);
                grabMenus.Remove(uid);
            }

            if (touchMenus.ContainsKey(uid))
            {
                if (touchInteractionCanvas) Destroy(touchInteractionCanvas.gameObject);
                foreach (GameObject menu in touchMenus[uid])
                    Destroy(menu);
                touchMenus.Remove(uid);
            }

            SaveHapticData();
            this.hapticInteractable = null;
            this.hapticData = null;
        }

        public void ShowMenus(HapticInteractable hapticInteractable, HapticData hapticData)
        {
            // do not display menus if the recording is armed
            if (recordingArmed) return;

            if (this.hapticInteractable != null && this.hapticInteractable.Uid != hapticInteractable.Uid)
            {
                this.hapticInteractable.ForceUnselection();
                HideMenus(this.hapticInteractable.Uid);
            }

            this.hapticInteractable = hapticInteractable;
            this.hapticData = hapticData;

            // position root of all menus
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            MenuParent.transform.position = hapticInteractable.GetMenuPosition();
            MenuParent.transform.rotation = Quaternion.LookRotation(new Vector3(MenuParent.transform.position.x, 0f, MenuParent.transform.position.z) - new Vector3(camera.transform.position.x, 0f, camera.transform.position.z));

            ShowGrabMenus();
            ShowTouchMenus();
        }


        public void RepositionMenus()
        {
            RepositionGrabMenus();
            RepositionTouchMenus();
        }

        public void ShowGrabMenus()
        {
            if (grabMenus.ContainsKey(hapticInteractable.Uid))
            {
                Debug.LogError("Grab menus already displayed");
                return;
            }

            // no grab event to display
            if (hapticData == null || hapticData.grabEventData.Count == 0)
            {
                Debug.Log("No event to display");
                return;
            }

            grabInteractionCanvas = Instantiate(interactionMappingCanvasPrefab);
            grabInteractionCanvas.Initialize(InteractionMappingCanvas.InteractionType.Grab);
            grabInteractionCanvas.transform.SetParent(MenuParent.transform);
            grabInteractionCanvas.transform.localPosition = -offsetMappingCanvas;
            grabInteractionCanvas.transform.localRotation = Quaternion.identity;

            grabMenus.Add(hapticInteractable.Uid, new List<GameObject>());

            for (int eventID = 0; eventID < hapticData.grabEventData.Count; eventID++)
            {
                // clean hapticEvent if no audioclip loaded
                if (hapticData.grabEventData[eventID].AudioClip == null)
                {
                    Debug.LogWarning("Removing grab event data (audioclip null)");
                    hapticData.grabEventData.RemoveAt(eventID);
                    continue;
                }

                GameObject menu = Instantiate(grabMenuPrefab);
                menu.GetComponent<GrabMenu>().Initialize(hapticInteractable, hapticData, eventID);
                menu.transform.SetParent(MenuParent.transform);
                PositionMenu(menu.transform, eventID, true);

                grabMenus[hapticInteractable.Uid].Add(menu);
            }
        }

        public void RemoveGrabLayer(int index)
        {
            if (hapticInteractable == null || !grabMenus.ContainsKey(hapticInteractable.Uid))
            {
                Debug.LogError("no grab menu to remove (index = " + index + ")");
            }

            if (index >= 0 && grabMenus[hapticInteractable.Uid].Count > index)
                grabMenus[hapticInteractable.Uid].RemoveAt(index);

            // update index of menus after the one removed from the list
            for (int i = index; i < grabMenus[hapticInteractable.Uid].Count; i++)
                grabMenus[hapticInteractable.Uid][i].GetComponent<GrabMenu>().UpdateIndex(i);
        }


        public void RepositionGrabMenus()
        {
            if (hapticInteractable == null || !grabMenus.ContainsKey(hapticInteractable.Uid))
            {
                Debug.Log("no menu to reposition");
            }
            Debug.Log("repositioning grab menus");

            List<GameObject> menus = grabMenus[hapticInteractable.Uid];

            // if no more events, we delete the grab interaction canvas
            if (menus.Count == 0)
                Destroy(grabInteractionCanvas.gameObject);

            for (int eventID = 0; eventID < hapticData.grabEventData.Count; eventID++)
            {
                //menus[eventID].transform.localPosition = new Vector3(-(.35f + (.4f * eventID)), .3f, 0f);
                PositionMenu(menus[eventID].transform, eventID, true);
            }
        }

        public void ShowTouchMenus()
        {
            if (touchMenus.ContainsKey(hapticInteractable.Uid))
            {
                Debug.LogError("Touch menus already displayed");
                return;
            }

            // no event to display
            if (hapticData == null || hapticData.touchEventData.Count == 0)
            {
                return;
            }

            touchInteractionCanvas = Instantiate(interactionMappingCanvasPrefab);
            touchInteractionCanvas.Initialize(InteractionMappingCanvas.InteractionType.Touch);
            touchInteractionCanvas.transform.SetParent(MenuParent.transform);
            touchInteractionCanvas.transform.localPosition = offsetMappingCanvas;
            touchInteractionCanvas.transform.localRotation = Quaternion.identity;

            touchMenus.Add(hapticInteractable.Uid, new List<GameObject>());

            for (int eventID = 0; eventID < hapticData.touchEventData.Count; eventID++)
            {
                for (int i = 0; i < hapticData.touchEventData.Count; i++)
                    if (hapticData.touchEventData[eventID].AudioClip == null)
                    {
                        Debug.LogWarning("Removing touch event data (audioclip null)");
                        hapticData.touchEventData.RemoveAt(eventID);
                        continue;
                    }

                //TODO offset each menu to avoid occlusion
                GameObject menu = Instantiate(touchMenuPrefab);
                menu.transform.SetParent(MenuParent.transform);
                menu.GetComponent<TouchMenu>().Initialize(hapticInteractable, hapticData, eventID);
                PositionMenu(menu.transform, eventID, false);
                //menu.transform.localPosition = new Vector3(.35f + (.4f * eventID), .3f, 0f);
                //menu.transform.localRotation = Quaternion.identity;
                touchMenus[hapticInteractable.Uid].Add(menu);
            }

        }

        public void RemoveTouchLayer(int index)
        {
            if (hapticInteractable == null || !touchMenus.ContainsKey(hapticInteractable.Uid))
            {
                Debug.LogError("no touch menu to remove (index = " + index + ")");
            }

            if (index >= 0 && touchMenus[hapticInteractable.Uid].Count > index)
                touchMenus[hapticInteractable.Uid].RemoveAt(index);

            // update index of menus after the one removed from the list
            for (int i = index; i < touchMenus[hapticInteractable.Uid].Count; i++)
                touchMenus[hapticInteractable.Uid][i].GetComponent<TouchMenu>().UpdateIndex(i);
        }


        public void RepositionTouchMenus()
        {
            if (hapticInteractable == null || !touchMenus.ContainsKey(hapticInteractable.Uid))
            {
                Debug.Log("no menu to reposition");
            }

            Debug.Log("repositioning touch menus");

            List<GameObject> menus = touchMenus[hapticInteractable.Uid];

            // if no more events, we delete the grab interaction canvas
            if (menus.Count == 0)
                Destroy(touchInteractionCanvas.gameObject);

            for (int eventID = 0; eventID < hapticData.touchEventData.Count; eventID++)
            {
                PositionMenu(menus[eventID].transform, eventID, false);
            }
        }


        #endregion

        #region Data


        private void CheckHapticData()
        {
            if (hapticData == null)
                hapticData = new HapticData(hapticInteractable.Uid);
            hapticData.instanceUid = hapticInteractable.Uid;

            SaveHapticData();
        }


        public void SaveHapticData()
        {
            SaveToJson(hapticData);
        }

        #endregion


        #region Recording

        public bool IsRecording()
        {
            string micDevice = GetMicrophoneDevice();
            if (string.IsNullOrEmpty(micDevice))
                return false;

            return Microphone.IsRecording(micDevice);
        }


        public void PlayRecording(HapticEventData hapticEventData)
        {
            if (hapticEventData == null)
            {
                Debug.LogError("hapticEventData is null");
                return;
            }

            if (hapticEventData == null || hapticEventData.AudioClip == null)
            {
                Debug.LogError("no audioclip to play");
                return;
            }

            recordingAudioSource.Stop();
            recordingAudioSource.clip = hapticEventData.AudioClip;
            recordingAudioSource.Play();
        }


        public Coroutine StartCheckForRecord(HapticEventData hapticEventData)
        {
            return StartCoroutine(CheckRecordingRoutine(hapticEventData));
        }


        private IEnumerator CheckRecordingRoutine(HapticEventData hapticEventData)
        {
            while (true)
            {
                if (!isRecordingAudio && recordingArmed)
                {
                    ShowRecordingFeedback(true);
                    //wait a while
                    yield return new WaitForSeconds(0.5f);
                    StartRecording(hapticEventData);
                }

                //if trigger on left controller is up or the user is not interacting anymore => stop recording
                if (isRecordingAudio && (!recordingArmed || stopRecording))
                {
                    stopRecording = false;
                    ShowRecordingFeedback(false);
                    StopRecording(true);
                    yield break;
                }

                yield return null;
            }
        }


        public void StartRecording(HapticEventData hapticEventData)
        {
            string micDevice = GetMicrophoneDevice();
            if (string.IsNullOrEmpty(micDevice))
                return;

            Debug.Log("StartRecording micDevice = " + micDevice);
            this.hapticEventData = hapticEventData;
            this.hapticEventData.PrepareForRecord();
            recordingClip = Microphone.Start(micDevice, false, defaultLengthSec, defaultFrequency);
            isRecordingAudio = true;
            recordPositionRoutine = StartCoroutine(RecordPositionRoutine());

            OnRecordingStarted?.Invoke(this, EventArgs.Empty);
        }


        private IEnumerator RecordPositionRoutine()
        {
            float time = 0f;

            while (true)
            {
                //record
                if (this.hapticInteractable != null && this.hapticInteractable.HasInterpolation())
                {
                    float lerp = this.hapticInteractable.GetInterpolation();
                    Debug.Log("record positional data, lerp = " + lerp);
                    this.hapticEventData.AddPositionalData(time, lerp);
                }

                //quit?
                if (!isRecordingAudio)
                    yield break;

                time += Time.deltaTime;

                yield return null;
            }
        }


        private string GetMicrophoneDevice()
        {
            string[] micDevices = Microphone.devices;

            if (micDevices.Length < 1)
            {
                Debug.LogError("No microphone vailable");
                return null;
            }

            return micDevices[0];
        }



        /// <summary>
        /// stop the microphone from recording and save data
        /// </summary>
        /// <param name="saveData">indicates we do not want to save the data recorded (likely because the user changed interaction style)</param>
        public void StopRecording(bool saveData)
        {
            string micDevice = GetMicrophoneDevice();
            if (string.IsNullOrEmpty(micDevice))
                return;

            Debug.Log("StopRecording micDevice = " + micDevice);
            int currentPosition = Microphone.GetPosition(micDevice);
            // currentPosition goes back to 0 when Microphone recorded for 10s
            if(Microphone.IsRecording(micDevice))
                Microphone.End(micDevice);
            isRecordingAudio = false;

            if (recordPositionRoutine != null)
                StopCoroutine(recordPositionRoutine);

            if (saveData)
            {
                if (recordingClip.samples == 0)
                {
                    Debug.LogError("Did not record any samples");
                    //TODO need to delete event data here
                    return;
                }

                // if data recorded but position at zero => recordingClip is full of data
                if (currentPosition == 0f)
                {
                    if(interactionTime >= 0.4f)
                        currentPosition = recordingClip.samples;
                    else // no data recorded because movement too fast
                    {
                        Debug.LogWarning("No data recorded, recording event too fast: " + interactionTime + "s");
                        if (recordedType == HapticInteractable.InteractionState.Grabbed)
                            hapticData?.grabEventData.RemoveAt(hapticData.grabEventData.Count - 1);
                        else
                            hapticData?.touchEventData.RemoveAt(hapticData.touchEventData.Count - 1);
                        return;
                    }
                }

                float[] soundData = new float[recordingClip.samples * recordingClip.channels];
                recordingClip.GetData(soundData, 0);

                //trim data to currentPosition
                float[] trimmedSoundData = new float[currentPosition * recordingClip.channels];

                //soundData.CopyTo(trimmedSoundData, 0);
                Array.Copy(soundData, trimmedSoundData, currentPosition);

                recordingClip = AudioClip.Create("recording", currentPosition, recordingClip.channels, recordingClip.frequency, false);
                recordingClip.SetData(trimmedSoundData, 0);

                //generate paths
                string recordingSubPath;
                string recordingFullPath;
                GenerateRecordingPath(hapticData.instanceUid, out recordingSubPath, out recordingFullPath);

                //save audio file
                SavWav.Save(recordingFullPath, recordingClip);

                //save data file
                this.hapticEventData.audioClipSubPath = recordingSubPath;


                float[] trimmedfreqs = smoothingAudioInput.GetFrequenciesFromSignal(trimmedSoundData);
                hapticEventData.SetAudioClip(recordingClip, trimmedfreqs);

                SaveHapticData();


                PGVRLinearConstraint linearConstraint = hapticInteractable.GetComponent<PGVRLinearConstraint>();

                // positional
                if (linearConstraint != null)
                {
                    Debug.Log("TIMESCALING:: Do your timescaling here");
                    Debug.Log("TIMESCALING:: Rubberband library removed to avoid conflicts with licensing");

                    // TimeScaling timeScaler = this.GetComponent<TimeScaling>();
                    // Debug.Log("TIMESCALING:: timeScaler " + timeScaler);
                    // if (timeScaler)
                    // {
                    //     try
                    //     {
                    //         timeScaler.timeScaleAudioRecording(this.hapticEventData.AudioClipFullPath,
                    //                                 recordingClip.samples,
                    //                                 this.hapticEventData.recordingTimes,
                    //                                 this.hapticEventData.recordingLerps);

                    //         this.hapticEventData.audioClipScaledSubPath = Path.Combine("hapticData", this.hapticData.instanceUid, timeScaler._audioFileScaled);
                    //         this.hapticEventData.scaledLerps = new List<float>(timeScaler._lerpsScaled);
                    //         this.hapticEventData.scaledTimes = new List<float>(timeScaler._timesScaled);

                    //         SaveHapticData();

                    //         StartCoroutine(LoadScaledAudioClip(this.hapticEventData));
                    //     }
                    //     catch (Exception e)
                    //     {
                            // Debug.LogError("Something went wrong with the timescaling:: " + e.Message);

                            this.hapticEventData.audioClipScaledSubPath = this.hapticEventData.audioClipSubPath;
                            this.hapticEventData.scaledLerps = new List<float>(this.hapticEventData.recordingLerps);
                            this.hapticEventData.scaledTimes = new List<float>(this.hapticEventData.recordingTimes);

                            SaveHapticData();

                            StartCoroutine(LoadScaledAudioClip(this.hapticEventData));
                    //     }
                    // }
                }
            }
            // not recording any data
            else
            {
                this.hapticEventData = null;
            }

            OnRecordingStopped?.Invoke(this, EventArgs.Empty);
        }


        private void GenerateRecordingPath(string instanceId, out string subpath, out string fullpath)
        {
            string directory = Path.Combine("hapticData", instanceId);
            string randomFileName = Path.GetRandomFileName();
            subpath = Path.Combine(directory, randomFileName) + ".wav";
            fullpath = Path.Combine(Application.streamingAssetsPath, subpath);
        }


        private IEnumerator LoadAudioClip(HapticEventData hapticEventData)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(hapticEventData.AudioClipFullPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    try
                    {
                        AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                        hapticEventData.AudioClip = myClip;
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogError("Could not load clip");
                    }
                }
            }
        }

        private IEnumerator LoadScaledAudioClip(HapticEventData hapticEventData)
        {
            if (hapticEventData.audioClipScaledSubPath != null && hapticEventData.audioClipScaledSubPath.Length > 0)
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(hapticEventData.AudioClipScaledFullPath, AudioType.WAV))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                        hapticEventData.AudioClipScaled = myClip;
                        Debug.Log("LoadScaledAudioClip done");
                    }
                }
            }
        }

        #endregion


        #region Haptic Devices

        public void RestartDistanceBasedHaptics()
        {
            foreach (HapticDevice hapticDevice in hapticDevices)
            {
                hapticDevice.RestartDistanceBasedHaptics();
            }
        }


        #endregion

        #region Serialization

        public void SaveToJson(HapticData hapticData)
        {
            string path = GetPath(hapticData.instanceUid);
            string json = JsonUtility.ToJson(hapticData);
            File.WriteAllText(path, json);
        }


        public HapticData Load(string instanceId)
        {
            string path = GetPath(instanceId);
            if (!System.IO.File.Exists(path))
            {
                InitFile(instanceId);
            }
            if (System.IO.File.Exists(path))
            {
                string json = File.ReadAllText(path);
                HapticData data = JsonUtility.FromJson<HapticData>(json);

                //load audioclips
                if (data.grabEventData != null)
                {
                    foreach (HapticEventData hapticEventData in data.grabEventData)
                    {
                        StartCoroutine(LoadAudioClip(hapticEventData));
                        StartCoroutine(LoadScaledAudioClip(hapticEventData));
                    }
                }


                if (data.touchEventData != null)
                {
                    foreach (HapticEventData hapticEventData in data.touchEventData)
                    {
                        StartCoroutine(LoadAudioClip(hapticEventData));
                        StartCoroutine(LoadScaledAudioClip(hapticEventData));
                    }
                }

                return data;
            }
            return null;
        }

        public void InitFile(string instanceId)
        {
            HapticData data = new HapticData(instanceId);
            SaveToJson(data);
        }


        private string GetPath(string instanceId)
        {
            string directory = Path.Combine(Application.streamingAssetsPath, "hapticData");
            System.IO.Directory.CreateDirectory(directory);
            return Path.Combine(directory, instanceId + ".json");
        }


        #endregion

    }
}
