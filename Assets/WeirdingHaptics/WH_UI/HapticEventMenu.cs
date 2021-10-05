using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Polygoat.Haptics
{
    public class HapticEventMenu : MonoBehaviour
    {
        [Header("General")]
        public Button btnClose;
        public RectTransform panelContent;
        public RectTransform panelSettingsInstant;
        public RectTransform panelSettingsContinuous;
        public RectTransform panelPitch;
        public RectTransform panelLoudness;
        public Toggle tglEnabled;

        [Header("Audio")]
        public RectTransform panelRecording;
        public RectTransform panelReadyForRecording;
        public Button btnPlay;
        public Button btnPlayHaptics;
        public Button btnRecord;
        public TMPro.TextMeshProUGUI lblRecord;
        public TMPro.TextMeshProUGUI lblFilename;
        public Toggle tglContinuous;
        public Toggle tglInstanteneous;
        public Toggle tglPositional;
        public Image imgWaveform;
        public Color waveformColor;
        public Color smootheningColor;
        public Color calibrationColor;
        public Color bgColor;

        private Texture2D waveformTexture;

        [Header("Pitch")]
        public Slider sliderPitchGlobal;
        public Slider sliderPitchVelocity;
        public Slider sliderPitchAngularVelocity;
        public Slider sliderPitchSize;
        public Slider sliderPitchRoughness;

        [Header("Loudness")]
        public Slider sliderLoudnessGlobal;
        public Slider sliderLoudnessVelocity;
        public Slider sliderLoudnessAngularVelocity;
        public Slider sliderLoudnessSize;
        public Slider sliderLoudnessRoughness;

        [Header("Navigation")]
        public TMPro.TextMeshProUGUI lblNavigation;
        public Button btnPrev;
        public Button btnNext;
        private int currentEventIndex;

        [Header("Creation")]
        public Button btnAdd;
        public Button btnDelete;

        public HapticInteractable HapticInteractable { get; private set; }
        public HapticData HapticData { get; private set; }
        public HapticEventData CurrentHapticEventData
        {
            get
            {
                List<HapticEventData> eventList = GetCurrentHapticEventDataList();
                if (eventList != null && currentEventIndex <= eventList.Count - 1)
                    return eventList[currentEventIndex];
                else
                    return null;
            }
        }

        public virtual List<HapticEventData> GetCurrentHapticEventDataList()
        {
            return null;
        }



        private void Start()
        {
            HideRecordingScreen();
            this.gameObject.SetActive(false);
        }



        protected virtual void OnEnable()
        {
            btnClose.onClick.AddListener(HandleCloseButtonClick);
            btnRecord.onClick.AddListener(HandleRecordButtonClick);
            btnPlay.onClick.AddListener(HandlePlayButtonClick);
            btnPrev.onClick.AddListener(HandlePrevButtonClick);
            btnNext.onClick.AddListener(HandleNextButtonClick);
            btnAdd.onClick.AddListener(HandleAddButtonClick);
            btnDelete.onClick.AddListener(HandleDeleteButtonClick);

            tglEnabled.onValueChanged.AddListener(HandleEnabledValueChanged);
            tglContinuous.onValueChanged.AddListener(HandleContinuousValueChanged);
            tglInstanteneous.onValueChanged.AddListener(HandleInstanteneousValueChanged);
            tglPositional.onValueChanged.AddListener(HandlePositionalValueChanged);

            sliderPitchGlobal.onValueChanged.AddListener(HandlePitchGlobalValueChanged);
            sliderPitchVelocity.onValueChanged.AddListener(HandlePitchVelocityValueChanged);
            sliderPitchAngularVelocity.onValueChanged.AddListener(HandlePitchAngularVelocityValueChanged);
            sliderPitchSize.onValueChanged.AddListener(HandlePitchSizeValueChanged);
            sliderPitchRoughness.onValueChanged.AddListener(HandlePitchRoughnessValueChanged);

            sliderLoudnessGlobal.onValueChanged.AddListener(HandleLoudnessGlobalValueChanged);
            sliderLoudnessVelocity.onValueChanged.AddListener(HandleLoudnessVelocityValueChanged);
            sliderLoudnessAngularVelocity.onValueChanged.AddListener(HandleLoudnessAngularVelocityValueChanged);
            sliderLoudnessSize.onValueChanged.AddListener(HandleLoudnessSizeValueChanged);
            sliderLoudnessRoughness.onValueChanged.AddListener(HandleLoudnessRoughnessValueChanged);

            HapticEditor.Instance.OnRecordingStarted += Instance_OnRecordingStarted;
            HapticEditor.Instance.OnRecordingStopped += Instance_OnRecordingStopped;
        }

   

        protected virtual void OnDisable()
        {
            btnClose.onClick.RemoveAllListeners();
            btnRecord.onClick.RemoveAllListeners();
            btnPlay.onClick.RemoveAllListeners();
            btnPrev.onClick.RemoveAllListeners();
            btnNext.onClick.RemoveAllListeners();
            btnAdd.onClick.RemoveAllListeners();
            btnDelete.onClick.RemoveAllListeners();

            tglEnabled.onValueChanged.RemoveAllListeners();
            tglContinuous.onValueChanged.RemoveAllListeners();
            tglInstanteneous.onValueChanged.RemoveAllListeners();
            tglPositional.onValueChanged.RemoveAllListeners();

            sliderPitchGlobal.onValueChanged.RemoveAllListeners();
            sliderPitchVelocity.onValueChanged.RemoveAllListeners();
            sliderPitchAngularVelocity.onValueChanged.RemoveAllListeners();
            sliderPitchSize.onValueChanged.RemoveAllListeners();
            sliderPitchRoughness.onValueChanged.RemoveAllListeners();

            sliderLoudnessGlobal.onValueChanged.RemoveAllListeners();
            sliderLoudnessVelocity.onValueChanged.RemoveAllListeners();
            sliderLoudnessAngularVelocity.onValueChanged.RemoveAllListeners();
            sliderLoudnessSize.onValueChanged.RemoveAllListeners();
            sliderLoudnessRoughness.onValueChanged.RemoveAllListeners();

            HapticEditor.Instance.OnRecordingStarted -= Instance_OnRecordingStarted;
            HapticEditor.Instance.OnRecordingStopped -= Instance_OnRecordingStopped;
        }


        public void Initialize(HapticInteractable hapticInteractable, HapticData hapticData)
        {
            HapticInteractable = hapticInteractable;
            HapticData = hapticData;
            currentEventIndex = 0;
            UpdateForm();
        }



        #region data

        protected virtual void AddEventData()
        {

        }

        protected virtual void RemoveEventData(int currentIndex)
        {

        }


        private void StopRecording()
        {
            //stop recording
            lblRecord.text = "REC";
            CheckRecordingState(false);
            UpdateForm();
            HideRecordingScreen();
        }

        private void ShowRecordingScreen()
        {
            HideReadyForRecordingScreen();
            panelRecording.gameObject.SetActive(true);
        }


        private void HideRecordingScreen()
        {
            panelRecording.gameObject.SetActive(false);
        }


        private void ShowReadyForRecordingScreen()
        {
            panelReadyForRecording.gameObject.SetActive(true);
        }


        private void HideReadyForRecordingScreen()
        {
            panelReadyForRecording.gameObject.SetActive(false);
        }

        #endregion


        #region navigation


        private void UpdateNavigation()
        {
            bool hasPrev = currentEventIndex > 0;
            bool hasNext = false;
            if (GetCurrentHapticEventDataList() != null)
            {
                hasNext = currentEventIndex < (GetCurrentHapticEventDataList().Count - 1);
            }

            btnPrev.interactable = hasPrev;
            btnNext.interactable = hasNext;
        }


        private void CheckRecordingState(bool isRecording)
        {
            btnPrev.interactable = !isRecording;
            btnNext.interactable = !isRecording;
            btnClose.interactable = !isRecording;
            btnPlay.interactable = !isRecording;
            btnAdd.interactable = !isRecording;
            btnDelete.interactable = !isRecording;
        }


        protected virtual void UpdateForm()
        {
            Debug.LogWarning("UpdateForm");
            UpdateNavigation();

            HapticEventData currentEvent = CurrentHapticEventData;
            if (currentEvent == null)
            {
                Debug.Log("currentEvent is nulllll");

                lblNavigation.text = "0/0";

                //hide content
                panelContent.gameObject.SetActive(false);
                panelPitch.gameObject.SetActive(false);
                panelLoudness.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("currentEvent is NOT nulllll");

                //show all
                panelContent.gameObject.SetActive(true);
                panelPitch.gameObject.SetActive(true);
                panelLoudness.gameObject.SetActive(true);

                //update controls
                tglEnabled.isOn = currentEvent.enabled;
                lblFilename.text = currentEvent.audioClipSubPath;
                lblNavigation.text = (currentEventIndex + 1).ToString() + "/" + GetCurrentHapticEventDataList().Count.ToString();

                bool isContinous = CurrentHapticEventData.hapticType == HapticType.ContinuousPassive;
                bool isInstantaneous = CurrentHapticEventData.hapticType == HapticType.Instantaneous;
                bool isPositional = CurrentHapticEventData.hapticType == HapticType.Positional;

                tglContinuous.isOn = isContinous;
                tglInstanteneous.isOn = isInstantaneous;
                tglPositional.isOn = isPositional;

                //update waveform
                if (currentEvent.AudioClip != null)
                {
                    waveformTexture = GetWaveformSpectrum(currentEvent.AudioClip, 1f, (int)imgWaveform.rectTransform.sizeDelta.x, (int)imgWaveform.rectTransform.sizeDelta.y, waveformColor, smootheningColor, calibrationColor, bgColor);
                    imgWaveform.sprite = Sprite.Create(waveformTexture, new Rect(0f, 0f, waveformTexture.width, waveformTexture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    imgWaveform.sprite = null;
                }

                //update pitch sliders
                sliderPitchGlobal.value = CurrentHapticEventData.pitchGlobal;
                sliderPitchVelocity.value = CurrentHapticEventData.pitchSpeedVelocity;
                sliderPitchAngularVelocity.value = CurrentHapticEventData.pitchSpeedAngularVelocity;
                sliderPitchSize.value = CurrentHapticEventData.pitchSize;
                sliderPitchRoughness.value = CurrentHapticEventData.pitchRoughness;

                //update loudness sliders
                sliderLoudnessGlobal.value = CurrentHapticEventData.amplitudeGlobal;
                sliderLoudnessVelocity.value = CurrentHapticEventData.amplitudeSpeedVelocity;
                sliderLoudnessAngularVelocity.value = CurrentHapticEventData.amplitudeSpeedAngularVelocity;
                sliderLoudnessSize.value = CurrentHapticEventData.amplitudeSize;
                sliderLoudnessRoughness.value = CurrentHapticEventData.amplitudeRoughness;
            }
        }

        private void ClampIndex()
        {
            currentEventIndex = Mathf.Min(currentEventIndex, GetCurrentHapticEventDataList().Count - 1);
            currentEventIndex = Mathf.Max(currentEventIndex, 0);

        }

        #endregion


        #region waveform


        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            // clamp value before mapping it
            value = Mathf.Min(value, to1);
            value = Mathf.Max(value, from1);
            // map value to given range
            return ((value - from1) / (to1 - from1)) * (to2 - from2) + from2;
        }

        public Texture2D GetWaveformSpectrum(AudioClip audio, float saturation, int width, int height, Color waveCol, Color smoothCol, Color calibCol, Color bgCol)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float[] samples = new float[audio.samples];
            float[] smoothenedSamples = new float[audio.samples];
            float[] waveform = new float[width];
            float[] smoothCurve = new float[width];

            SmoothingAudioInput smoothing = Object.FindObjectOfType<SmoothingAudioInput>();
            audio.GetData(samples, 0);
            smoothing.SmoothenClip(samples, smoothenedSamples);
            // float[] frequencies = smoothing.GetFrequenciesFromSignal(samples);

            int packSize = (audio.samples / width) + 1;
            int s = 0;
            for (int i = 0; i < audio.samples; i += packSize)
            {
                waveform[s] = Mathf.Abs(samples[i]);
                smoothCurve[s] = smoothenedSamples[i];
                s++;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tex.SetPixel(x, y, bgCol);
                }
            }

            AudioCalibration calibration = Object.FindObjectOfType<AudioCalibration>();
            for (int x = 0; x < waveform.Length; x++)
            {
                for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++)
                {
                    tex.SetPixel(x, (height / 2) + y, waveCol);
                    tex.SetPixel(x, (height / 2) - y, waveCol);
                }

                
                int sy = (int)(smoothCurve[x] * ((float)height * .75f));
                for (int ty = sy - 1; ty <= sy + 1; ty++) { 
                    tex.SetPixel(x, (height / 2) + ty, smoothCol);
                    tex.SetPixel(x, (height / 2) - ty, smoothCol);
                }


                // modify amplitude based on frequency detected
                // modulate amplitude based on the frequency level
                //float freq = frequencies[Mathf.FloorToInt(((float)x / (float)waveform.Length) * frequencies.Length)];
                //if (freq != 0)
                //{
                //    float ratio = Remap(freq, calibration.minFrequency, calibration.maxFrequency, -1f, 1f);
                //    float oldValue = smoothCurve[x];
                //    smoothCurve[x] = Mathf.Clamp01(smoothCurve[x] + smoothing.freqCoeff * ratio);
                //    //Debug.Log("index:" + x + " ratio: " + ratio + " ratio*coeff:" + (smoothing.freqCoeff * ratio) + " old value: " + oldValue + " new value: " + smoothCurve[x]);
                //}
                //sy = (int)(smoothCurve[x] * ((float)height * .75f));
                //for (int ty = sy - 1; ty <= sy + 1; ty++)
                //{
                //    tex.SetPixel(x, (height / 2) + ty, calibCol);
                //    tex.SetPixel(x, (height / 2) - ty, calibCol);
                //}

                //if(calibration)
                //{
                //    // min amplitude
                //    int min = (int) (height * .75f * calibration.minAmplitude);
                //    tex.SetPixel(x, (height / 2) + min, calibCol);
                //    tex.SetPixel(x, (height / 2) - min, calibCol);
                //    // max amplitude
                //    int max = (int)(height * .75f * calibration.maxAmplitude);
                //    tex.SetPixel(x, (height / 2) + max, calibCol);
                //    tex.SetPixel(x, (height / 2) - max, calibCol);
                //}

            }
            tex.Apply();

            return tex;
        }



        #endregion


        #region eventhandlers

        private void HandleCloseButtonClick()
        {
            //HapticEditor.Instance.HideHapticMenu();
        }


        private void HandlePlayButtonClick()
        {
            HapticEditor.Instance.PlayRecording(CurrentHapticEventData);
        }


        private void HandleRecordButtonClick()
        {
            if (HapticEditor.Instance.IsRecording())
            {
                /*
                //stop recording
                HapticEditor.Instance.StopRecording();
                lblRecord.text = "REC";
                CheckRecordingState();
                UpdateForm();*/
            }
            else
            {
                //start recording
                HapticEditor.Instance.StartCheckForRecord(CurrentHapticEventData);
                lblRecord.text = "STOP";
                ShowReadyForRecordingScreen();
                CheckRecordingState(true);
                UpdateForm();
            }
        }

        private void HandlePrevButtonClick()
        {
            currentEventIndex--;
            ClampIndex();
            UpdateForm();
        }

        private void HandleNextButtonClick()
        {
            currentEventIndex++;
            ClampIndex();
            UpdateForm();
        }


        protected virtual void HandleAddButtonClick()
        {
            AddEventData();
            Debug.Log("HapticData count = " + GetCurrentHapticEventDataList().Count);
            currentEventIndex = GetCurrentHapticEventDataList().Count - 1;
            UpdateForm();
        }

        private void HandleDeleteButtonClick()
        {
            RemoveEventData(currentEventIndex);
            ClampIndex();
            UpdateForm();
            HapticEditor.Instance.SaveHapticData();
        }


        private void HandleEnabledValueChanged(bool enabled)
        {
            Debug.Log("HandleEnabledValueChanged");
            CurrentHapticEventData.enabled = enabled;
        }

        private void HandleContinuousValueChanged(bool enabled)
        {
            Debug.Log("HandleContinuousValueChanged");

            if (!enabled || CurrentHapticEventData.hapticType == HapticType.Distance)
                return;

            CurrentHapticEventData.hapticType = HapticType.Continuous;
        }

        private void HandleInstanteneousValueChanged(bool enabled)
        {
            Debug.Log("HandleInstanteneousValueChanged");
            if (!enabled || CurrentHapticEventData.hapticType == HapticType.Distance)
                return;

            CurrentHapticEventData.hapticType = HapticType.Instantaneous;
        }

        private void HandlePositionalValueChanged(bool enabled)
        {
            Debug.Log("HandlePositionalValueChanged");
            if (!enabled || CurrentHapticEventData.hapticType == HapticType.Distance)
                return;

            CurrentHapticEventData.hapticType = HapticType.Positional;
        }


        private void HandlePitchGlobalValueChanged(float value)
        {
            CurrentHapticEventData.pitchGlobal = value;
        }

        private void HandlePitchVelocityValueChanged(float value)
        {
            CurrentHapticEventData.pitchSpeedVelocity = value;
        }

        private void HandlePitchAngularVelocityValueChanged(float value)
        {
            CurrentHapticEventData.pitchSpeedAngularVelocity = value;
        }

        private void HandlePitchSizeValueChanged(float value)
        {
            CurrentHapticEventData.pitchSize = value;
        }

        private void HandlePitchRoughnessValueChanged(float value)
        {
            CurrentHapticEventData.pitchRoughness = value;
        }

        private void HandleLoudnessGlobalValueChanged(float value)
        {
            CurrentHapticEventData.amplitudeGlobal = value;
        }

        private void HandleLoudnessVelocityValueChanged(float value)
        {
            CurrentHapticEventData.amplitudeSpeedVelocity = value;
        }

        private void HandleLoudnessAngularVelocityValueChanged(float value)
        {
            CurrentHapticEventData.amplitudeSpeedAngularVelocity = value;
        }

        private void HandleLoudnessSizeValueChanged(float value)
        {
            CurrentHapticEventData.amplitudeSize = value;
        }

        private void HandleLoudnessRoughnessValueChanged(float value)
        {
            CurrentHapticEventData.amplitudeRoughness = value;
        }


        //private void HandleSettingsInstantRepeatChanged(float value)
        //{
        //    CurrentHapticEventData.instantTimes = (int)value;
        //    lblSettingsInstantTimes.text = CurrentHapticEventData.instantTimes.ToString();
        //}


        //private void HandleContinousSpeedDependantValueChanged(bool enabled)
        //{
        //    CurrentHapticEventData.speedDependant = enabled;
        //}


        

        private void Instance_OnRecordingStarted(object sender, System.EventArgs e)
        {
            ShowRecordingScreen();
        }

        private void Instance_OnRecordingStopped(object sender, System.EventArgs e)
        {
            Debug.LogWarning("Instance_OnRecordingStopped");
            StopRecording();
        }

        #endregion


    }
}