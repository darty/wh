using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class AudioCalibration : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float minAmplitude;

    [Range(0.0f, 1.0f)]
    public float maxAmplitude;

    [Range(0.0f, 1000.0f)]
    public float minFrequency;
    [Range(0.0f, 1000.0f)]
    public float maxFrequency;

    public enum Device { NativeActuator, EnhancedActuator };
    [Tooltip("NativeActuator: vibrations come from the Oculus Controller, EnhancedActuator: vibrations come from the Nintendo Switch Actuator")]
    public Device device;

    // UI interactions

    // amplitude
    public Button calibrateAmplitude;
    public GameObject amplitudePanel;
    public Button validateAmplitude;
    public Button calibrateMinimalAmplitude;
    public Text minAmpLabel;
    public Button calibrateMaximalAmplitude;
    public Text maxAmpLabel;

    // frequency
    public Button calibrateFrequency;
    public GameObject frequencyPanel;
    public Button validateFrequency;
    public Button calibrateFrequencyRange;
    public Text minFreqLabel;
    public Text maxFreqLabel;

    // recording timers
    public Text amplitudeTimer;
    public Text frequencyTimer;
    private Text currentTimer;
    private float currentTime;
    private bool recording;

    // magnitudes to control haptics in the editor
    public float frequencyMagnitude;
    public float expressivenessMagnitude;
    public float randomnessMagnitude;
    public float intensityMagnitude;
    public float modulationMagnitude;

    // flags for calibration
    enum CalibrationType
    {
        MinimalAmplitude,
        MaximalAmplitude,
        FrequencyRange
    }
    private CalibrationType calibrationType;

    // microphone recording
    private AudioClip audioRecording;
    // waveform images for debugging
    public Image ampWaveform;
    public Image freqWaveform;
    private Image currentWaveform;

    private SmoothingAudioInput smoothing;

    private void Awake()
    {
        calibrateAmplitude.onClick.AddListener(() => SetActivateAmplitudePanel(true));
        calibrateFrequency.onClick.AddListener(() => SetActivateFrequencyPanel(true));
        validateAmplitude.onClick.AddListener(() => SetActivateAmplitudePanel(false));
        validateFrequency.onClick.AddListener(() => SetActivateFrequencyPanel(false));

        // calibration features
        calibrateMinimalAmplitude.onClick.AddListener(() => SetCalibrateType(CalibrationType.MinimalAmplitude));
        calibrateMaximalAmplitude.onClick.AddListener(() => SetCalibrateType(CalibrationType.MaximalAmplitude));
        calibrateFrequencyRange.onClick.AddListener(() => SetCalibrateType(CalibrationType.FrequencyRange));

        LoadCalibrationData();

        minAmpLabel.text = ""+minAmplitude.ToString("0.00");
        maxAmpLabel.text = ""+maxAmplitude.ToString("0.00");

        minFreqLabel.text = "" + minFrequency.ToString("0.0");
        maxFreqLabel.text = "" + maxFrequency.ToString("0.0");

        smoothing = UnityEngine.Object.FindObjectOfType<SmoothingAudioInput>();
        if (!smoothing) Debug.LogError("No SmoothingAudioInput object in the scene");

        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        GetComponentInChildren<Canvas>().worldCamera = camera.GetComponent<Camera>();
    }

    public void SwitchActuator()
    {
        device = device == Device.NativeActuator ? Device.EnhancedActuator : Device.NativeActuator;
        Debug.Log("SwitchActuator(): " + device);
    }

    private void SetActivateAmplitudePanel(bool activate)
    {
        amplitudePanel.SetActive(activate);
        if (activate) currentWaveform = ampWaveform;
    }

    private void SetActivateFrequencyPanel(bool activate)
    {
        frequencyPanel.SetActive(activate);
        if (activate) currentWaveform = freqWaveform;
    }

    private void SetCalibrateType(CalibrationType type)
    {
        calibrationType = type;
        RecordAudio(calibrationType == CalibrationType.FrequencyRange ? 6 : 3);
    }

    private void SetTimer(float time)
    {
        if (calibrationType == CalibrationType.MinimalAmplitude || calibrationType == CalibrationType.MaximalAmplitude)
            currentTimer = amplitudeTimer;
        else
            currentTimer = frequencyTimer;
        currentTime = time;
        currentTimer.text = "0"+time+":00";
    }

    private string FormatTime(float time)
    {
        float s = Mathf.Floor(time);
        float ms = (time - s) * 100f;
        return string.Format("0{0}:{1}", Mathf.Floor(time), Mathf.Floor(ms));
    }


    private void AnalyzeAudio()
    {
        ShowAudioRecording();
        //TODO analyze audio based on the calibration type
        if (calibrationType == CalibrationType.MinimalAmplitude || calibrationType == CalibrationType.MaximalAmplitude)
        {
            // we calibrate the min and max amplitude using the min and max of the data recorded

            float[] samples = new float[audioRecording.samples];
            audioRecording.GetData(samples, 0);
            float[] smooth = new float[audioRecording.samples];
            smoothing.SmoothenClip(samples, smooth);

            Array.Sort(smooth, 0, smooth.Length);
            if(calibrationType == CalibrationType.MinimalAmplitude) {
                int i = 0;
                while(i < smooth.Length && smooth[i] < 0.005f) i++;
                minAmplitude = smooth[i];
                minAmpLabel.text = "" + minAmplitude.ToString("0.00");
            }
            else { 
                maxAmplitude = smooth[smooth.Length - 1];
                maxAmpLabel.text = "" + maxAmplitude.ToString("0.00");
            }
        }
        else
        {
            // we calibrate the min and max pitch using respectively the fifth and ninetyfifth percentile values

            float[] samples = new float[audioRecording.samples];
            audioRecording.GetData(samples, 0);
            float[] pitches = smoothing.GetFrequenciesFromSignal(samples);
            
            Array.Sort(pitches, 0, pitches.Length);
            
            int zeros = 0;
            while(zeros < pitches.Length && pitches[zeros] <= 0) { zeros++; }
            if (zeros == pitches.Length)
                Debug.LogError("No pitches recognized during calibration");
            else
            {
                int recognizedLength = (pitches.Length - zeros);
                //float percentile = recognizedLength / 100f;
                minFrequency = pitches[zeros + Mathf.FloorToInt(recognizedLength * .05f)];
                maxFrequency = pitches[zeros + Mathf.FloorToInt(recognizedLength * .95f)];

                minFreqLabel.text = "" + minFrequency.ToString("0.0");
                maxFreqLabel.text = "" + maxFrequency.ToString("0.0");
            }
        }

        SaveCalibrationData();
    }


    void RecordAudio(int seconds)
    {
        string[] micDevices = Microphone.devices;
        if (micDevices.Length < 1)
        {
            Debug.LogError("No microphone vailable");
        }
        else
        {
            audioRecording = Microphone.Start(micDevices[0], false, seconds, 44100);
            recording = true;
            SetTimer(calibrationType == CalibrationType.FrequencyRange ? 6f : 3f);
        }
    }


    void ShowAudioRecording()
    {
        int width = (int)currentWaveform.rectTransform.sizeDelta.x, height = (int)currentWaveform.rectTransform.sizeDelta.y;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float[] samples = new float[audioRecording.samples];
        float[] waveform = new float[width];

        SmoothingAudioInput smoothing = UnityEngine.Object.FindObjectOfType<SmoothingAudioInput>();
        audioRecording.GetData(samples, 0);

        int packSize = (audioRecording.samples / width) + 1;
        int s = 0;
        for (int i = 0; i < audioRecording.samples; i += packSize)
        {
            waveform[s] = Mathf.Abs(samples[i]);
            s++;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, Color.black);
            }
        }

        AudioCalibration calibration = UnityEngine.Object.FindObjectOfType<AudioCalibration>();
        for (int x = 0; x < waveform.Length; x++)
        {
            for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++)
            {
                tex.SetPixel(x, (height / 2) + y, Color.green);
                tex.SetPixel(x, (height / 2) - y, Color.green);
            }
        }
        tex.Apply();

        currentWaveform.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if(recording)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0) { 
                recording = false;
                currentTime = 0;
                AnalyzeAudio();
            }
            currentTimer.text = FormatTime(currentTime);
        }

        // save audio calibration data on keyboard key A press
        if (Input.GetKeyDown(KeyCode.A))
        {
            SaveCalibrationData();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SwitchActuator();
        }
    }


    // utilities
    public float MapAmplitudeToCalibration(float amplitude)
    {
        //Debug.Log("MapAmplitudeToCalibration: " + amplitude + ", min:" + minAmplitude + ", max:" + maxAmplitude);
        amplitude = Mathf.Clamp(amplitude, minAmplitude, maxAmplitude);
        //Debug.Log("MapAmplitudeToCalibration2: " + amplitude + ", min:" + minAmplitude + ", max:" + maxAmplitude + " R: " + (amplitude - minAmplitude) / (maxAmplitude - minAmplitude));
        return (amplitude - minAmplitude) / (maxAmplitude - minAmplitude);
    }

    public float MapFrequencyToCalibration(float frequency)
    {
        frequency = Mathf.Clamp(frequency, minFrequency, maxFrequency);
        return (frequency - minFrequency) / (maxFrequency - minFrequency);
    }


    #region serialization
    
    [Serializable]
    private class CalibrationData
    {
        public float minAmplitude;
        public float maxAmplitude;
        public float minFrequency;
        public float maxFrequency;

        public float frequencyMagnitude;
        public float expressivenessMagnitude;
        public float randomnessMagnitude;
        public float intensityMagnitude;
        public float modulationMagnitude;

        public CalibrationData(AudioCalibration calibration)
        {
            this.minAmplitude = calibration.minAmplitude;
            this.maxAmplitude = calibration.maxAmplitude;
            this.minFrequency = calibration.minFrequency;
            this.maxFrequency = calibration.maxFrequency;

            this.frequencyMagnitude = calibration.frequencyMagnitude;
            this.expressivenessMagnitude = calibration.expressivenessMagnitude;
            this.randomnessMagnitude = calibration.randomnessMagnitude;
            this.intensityMagnitude = calibration.intensityMagnitude;
            this.modulationMagnitude = calibration.modulationMagnitude;
        }
    }


    private string GetPath()
    {
        string directory = Path.Combine(Application.streamingAssetsPath, "audioCalibration");
        System.IO.Directory.CreateDirectory(directory);
        return Path.Combine(directory, "calibration.json");
    }


    private void LoadCalibrationData()
    {
        string path = GetPath();
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        CalibrationData data = JsonUtility.FromJson<CalibrationData>(json);

        if(data != null) { 
            this.minAmplitude = data.minAmplitude;
            this.maxAmplitude = data.maxAmplitude;
            this.minFrequency = data.minFrequency;
            this.maxFrequency = data.maxFrequency;

            this.frequencyMagnitude = data.frequencyMagnitude;
            this.expressivenessMagnitude = data.expressivenessMagnitude;
            this.randomnessMagnitude = data.randomnessMagnitude;
            this.intensityMagnitude = data.intensityMagnitude;
            this.modulationMagnitude = data.modulationMagnitude;
        }
    }


    private void SaveCalibrationData()
    {
        string path = GetPath();
        CalibrationData data = new CalibrationData(this);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    #endregion
}
