using PGLibrary.PGVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polygoat.Haptics
{
    public class HapticDevice : MonoBehaviour
    {
        private PGVRInteractableController interactableController;
        public AudioAnalyzer audioAnalyzer;
        public AudioSource audioSource;
        public float hapticsFrequency = 10f;
        public LayerMask interactablesLayer;

        // user's calibration parameters
        protected AudioCalibration calibration;
        // smoothing audio functions
        protected SmoothingAudioInput smoothing;


        private Coroutine listenerRoutine;
        private Coroutine loopingClipRoutine;


        private Dictionary<AudioClip, Coroutine> loopingAudioClips = new Dictionary<AudioClip, Coroutine>();
        private Dictionary<HapticClip, Coroutine> loopingHapticClips = new Dictionary<HapticClip, Coroutine>();

        private List<float[]> samplesList = new List<float[]>();
        private float globalPulseTimer = 0f;
        private float globalPulseDuration;

        //distance based
        private bool distanceBasedHapticsEnabled = true;
        private List<HapticInteractable> hapticInteractablesInRange = new List<HapticInteractable>();

        //hapticeventdata
        private List<HapticEventData> hapticEventDataList = new List<HapticEventData>();
        private Dictionary<HapticEventData, HapticEventRuntimeData> hapticEventDataDict = new Dictionary<HapticEventData, HapticEventRuntimeData>();


        public Vector3 CurrentVelocity
        {
            get { return interactableController.EstimatedVelocity; }
        }

        public Vector3 CurrentAngularVelocity
        {
            get { return interactableController.EstimatedAngularVelocity; }
        }

        private const float velocityMagicNumber = 5f;
        private const float minimumVelocity = 0.1f;

        //// parameters used for USER INPUT
        //float randomnessMagnitude = .15f; // maximum change applied by randomness
        //float frequencyMagnitude = .15f; // maximum change applied by frequency on amplitude (only applies if we use AudioCalibration.NativeActuator)

        private const float maxInteractablesDistance = 10f;

        [Header("Debug")]
        public float testFrequency = 100f;
        public bool printDebug;


        private void Awake()
        {
            interactableController = this.GetComponent<PGVRInteractableController>();
            globalPulseDuration = 1f / hapticsFrequency;
            globalPulseTimer = globalPulseDuration;

            calibration = FindObjectOfType<AudioCalibration>();
            if (!calibration) Debug.LogError("No AudioCalibration object in the scene");

            smoothing = FindObjectOfType<SmoothingAudioInput>();
            if (!smoothing) Debug.LogError("No SmoothingAudioInput object in the scene");
        }


        private void Update()
        {
            //CheckInteractablesInRange();
        }


        /// <summary>
        /// Implement this method for each haptics device
        /// </summary>
        /// <param name="durationSeconds"></param>
        /// <param name="frequency"></param>
        /// <param name="amplitude"></param>
        public virtual void TriggerPulse(float durationSeconds, float frequency, float amplitude)
        {
            //hapticAction.Execute();
        }


        #region distance based

        public void RestartDistanceBasedHaptics()
        {
            StopDistanceBasedHaptics();
            StartDistanceBasedHaptics();
        }


        public void StartDistanceBasedHaptics()
        {
            distanceBasedHapticsEnabled = true;
        }


        public void StopDistanceBasedHaptics()
        {
            distanceBasedHapticsEnabled = false;

            foreach (HapticInteractable hapticInteractable in hapticInteractablesInRange)
            {
                StopDistanceBasedHaptics(hapticInteractable.hapticData, hapticInteractable.transform);
            }

            hapticInteractablesInRange.Clear();
        }


        private void CheckInteractablesInRange()
        {
            if (!distanceBasedHapticsEnabled)
                return;

            //Search for HapticInteractables
            List<HapticInteractable> newHapticInteractablesInRange = new List<HapticInteractable>();
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, maxInteractablesDistance, interactablesLayer);
            foreach (Collider collider in colliders)
            {
                HapticInteractable hapticInteractable = collider.GetComponent<HapticInteractable>();
                if (hapticInteractable == null)
                    continue;

                newHapticInteractablesInRange.Add(hapticInteractable);
            }

            //interactables enter range?
            foreach (HapticInteractable hapticInteractable in newHapticInteractablesInRange)
            {
                /*
                if (!hapticInteractablesInRange.Contains(hapticInteractable))
                {

                    //entered
                    PlayDistanceBasedHaptics(hapticInteractable.hapticData, hapticInteractable.transform);
                }*/

                if (hapticInteractable.hapticData != null)
                    PlayDistanceBasedHaptics(hapticInteractable.hapticData, hapticInteractable.transform);
            }

            //interactables exit range?
            foreach (HapticInteractable hapticInteractable in hapticInteractablesInRange)
            {
                if (!newHapticInteractablesInRange.Contains(hapticInteractable))
                {
                    //exited
                    if (hapticInteractable.hapticData != null)
                        StopDistanceBasedHaptics(hapticInteractable.hapticData, hapticInteractable.transform);
                }
            }

            hapticInteractablesInRange = newHapticInteractablesInRange;
        }


        private void PlayDistanceBasedHaptics(HapticData hapticData, Transform sourceTransform)
        {
            foreach (HapticEventData hapticEventData in hapticData.distanceEventData)
            {
                if (hapticEventData == null || hapticEventData.AudioClip == null || !hapticEventData.enabled)
                    continue;
                if (hapticEventDataDict.ContainsKey(hapticEventData))
                    continue;

                Debug.LogWarning("ADD DISTANCE CLIP");

                Debug.LogWarning("ADD DISTANCE CLIP type = " + hapticEventData.hapticType.ToString());
                HapticEventRuntimeData runtimeData = new HapticEventRuntimeData(hapticEventData);
                runtimeData.sourceTransform = sourceTransform;
                hapticEventDataDict.Add(hapticEventData, runtimeData);
            }
        }


        private void StopDistanceBasedHaptics(HapticData hapticData, Transform sourceTransform)
        {
            Debug.LogWarning("StopDistanceBasedHaptics", sourceTransform.gameObject);
            foreach (HapticEventData hapticEventData in hapticData.distanceEventData)
            {
                if (hapticEventDataDict.ContainsKey(hapticEventData))
                {
                    hapticEventDataDict.Remove(hapticEventData);
                }
            }
        }


        #endregion



        #region read audio data from AudioListener (must play an audioclip)

        public void StartListener()
        {
            if (listenerRoutine == null)
                listenerRoutine = StartCoroutine(HapticsAudioRoutine());
        }

        public void StopListener()
        {
            if (listenerRoutine != null)
            {
                StopCoroutine(listenerRoutine);
                listenerRoutine = null;
            }
        }


        private IEnumerator HapticsAudioRoutine()
        {
            float frequency = audioSource.clip.frequency;

            //10hz frequency (haptics can change 10 times per second)
            float hapticPulseDuration = 1f / hapticsFrequency;
            float pulseTimer = 0f;

            while (true)
            {
                //trigger new pulse?
                if (pulseTimer <= 0f)
                {
                    float rms;
                    float db;
                    float pitch;

                    float sampleDuration = (int)(hapticPulseDuration * frequency);
                    int sampleSize = 1024;

                    audioAnalyzer.AnalyzeSound(audioSource, sampleSize, out rms, out db, out pitch);

                    Debug.Log("rms: " + rms);
                    Debug.Log("db: " + db);
                    Debug.Log("pitch: " + pitch);

                    //trigger pulse
                    audioAnalyzer.minDb = -30;
                    float maxDb = -audioAnalyzer.minDb;
                    float hapticsAmplitude = (db - audioAnalyzer.minDb) / maxDb;
                    Debug.Log("hapticsAmplitude: " + hapticsAmplitude);

                    TriggerPulse(hapticPulseDuration, testFrequency, Mathf.Abs(hapticsAmplitude));

                    pulseTimer += hapticPulseDuration;
                }

                pulseTimer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        #endregion


        #region read audio from audioclip 

        public void PlayAudioClip(AudioClip audioClip)
        {
            Debug.Log("HapticDevice PlayAudioClip");
            HapticClip hapticClip = new HapticClip(audioClip);
            PlayHapticClip(hapticClip);
        }


        public void StartAudioClipLoop(AudioClip audioClip)
        {
            Debug.Log("HapticDevice StartAudioClipLoop");
            if (loopingAudioClips.ContainsKey(audioClip))
            {
                Debug.LogError("Cannot play same looping audioClip twice: " + audioClip.name);
                return;
            }

            HapticClip hapticClip = new HapticClip(audioClip);
            Coroutine loopRoutine = StartCoroutine(HapticsRoutine(hapticClip, true));
            loopingAudioClips.Add(audioClip, loopRoutine);
        }


        public void StopAudioClipLoop(AudioClip audioClip)
        {
            Debug.Log("HapticDevice StopAudioClipLoop");
            if (!loopingAudioClips.ContainsKey(audioClip))
            {
                Debug.LogError("Cannot stop looping audioClip: " + audioClip.name);
                return;
            }

            StopCoroutine(loopingAudioClips[audioClip]);
            loopingAudioClips.Remove(audioClip);
        }


        public void PlayHapticClip(HapticClip hapticClip)
        {
            Debug.Log("HapticDevice PlayHapticClip");
            StartCoroutine(HapticsRoutine(hapticClip));
        }


        public void StartHapticClipLoop(HapticClip hapticClip)
        {
            Debug.Log("HapticDevice StartHapticClipLoop");
            if (loopingHapticClips.ContainsKey(hapticClip))
            {
                Debug.LogError("Cannot play same looping audioClip twice: " + hapticClip.audioClip.name);
                return;
            }

            Coroutine loopRoutine = StartCoroutine(HapticsRoutine(hapticClip, true));
            loopingHapticClips.Add(hapticClip, loopRoutine);
        }


        public void StopHapticClipLoop(HapticClip hapticClip)
        {
            Debug.Log("HapticDevice StopHapticClipLoop");
            if (!loopingHapticClips.ContainsKey(hapticClip))
            {
                Debug.LogError("Cannot stop looping audioClip: " + hapticClip.audioClip.name);
                return;
            }

            StopCoroutine(loopingHapticClips[hapticClip]);
            loopingHapticClips.Remove(hapticClip);
        }


        private IEnumerator HapticsRoutine(HapticClip hapticClip, bool loop = false)
        {
            float startTime = Time.unscaledTime;
            float length = hapticClip.Length;
            float endTime = startTime + length;
            float samples = hapticClip.Samples;
            float frequency = hapticClip.audioClip.frequency;

            Log("Start HapticsRoutine");
            Log("length = " + length);
            Log("samples = " + samples);
            Log("frequency = " + hapticClip.audioClip.frequency);

            //10hz frequency (haptics can change 10 times per second)
            float hapticPulseDuration = 1f / hapticsFrequency;
            float pulseTimer = 0f;

            //Debug.Log("hapticPulseDuration = " + hapticPulseDuration);

            while (loop || Time.unscaledTime < endTime)
            {
                //trigger new pulse?
                if (pulseTimer <= 0f)
                {
                    //get sample data
                    float lerp = (Time.unscaledTime - startTime) / length;
                    float wrappedLerp = lerp % 1;

                    int sampleIndex = (int)(samples * wrappedLerp);
                    float sampleSize = hapticPulseDuration * frequency;

                    /*
                    float db, rms = 0f;
                    
                    hapticClip.GetData(sampleIndex, (int)sampleSize, out rms, out db);

                    Log("wrappedLerp = " + wrappedLerp);
                    Log("sampleIndex = " + sampleIndex);
                    Log("sampleSize = " + sampleSize);
                    Log("rms = " + rms);
                    Log("db = " + db);

                    //trigger pulse
                    TriggerPulse(hapticPulseDuration, testFrequency, db * hapticClip.GetVolume());
                    */

                    pulseTimer += hapticPulseDuration;
                }

                pulseTimer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }


        private void Log(string text)
        {
            if (!printDebug)
                return;

            Debug.Log(text);
        }

        #endregion


        #region play from hapticData


        public void PlayHapticEvent(HapticEventData hapticEventData, Transform sourceTransform)
        {
            //Debug.Log("PlayHapticEvent " + hapticEventData.AudioClipFullPath + " source " + sourceTransform.GetComponent<UidComponent>().Uid + " Type:" + hapticEventData.hapticType);

            if (hapticEventData == null || hapticEventData.AudioClip == null)
            {
                Debug.Log("PlayHapticEvent:: hapticEventData == null || hapticEventData.AudioClip == null");
                return;
            }

            HapticEventRuntimeData runtimeData = new HapticEventRuntimeData(hapticEventData);
            runtimeData.sourceTransform = sourceTransform;
            hapticEventDataDict.Add(hapticEventData, runtimeData);
        }


        public void StopHapticEvent(HapticEventData hapticEventData)
        {
            //Debug.Log("HapticDevice StopHapticEvent");
            if (!hapticEventDataDict.ContainsKey(hapticEventData))
            {
                //Debug.LogError("Cannot stop looping hapticEventData on " + this.gameObject);
                return;
            }

            hapticEventDataDict.Remove(hapticEventData);
        }




        private void LateUpdate()
        {
            globalPulseTimer -= Time.unscaledDeltaTime;

            if (globalPulseTimer <= 0f)
            {
                CalculateNewPulse();
                //CalculateSwipePulse();

                globalPulseTimer += globalPulseDuration;
            }
        }


        private void CalculateNewPulse()
        {
            // Debug.Log("CalculateNewPulse()");
            List<HapticEventData> toRemove = new List<HapticEventData>();
            //samplesList.Clear();

            //float dbSum = 0f;
            List<float> amplitudes = new List<float>();
            List<float> frequencies = new List<float>();

            if (hapticEventDataDict == null)
                return;

            if (hapticEventDataDict == null || hapticEventDataDict.Count == 0)
            {
                /*Debug.Log("CalculateNewPulse():: (hapticEventDataDict == null || hapticEventDataDict.Count == 0)");
                if (hapticEventDataDict == null)
                {
                    Debug.Log("hapticEventDataDict == null");
                }
                else
                {
                    Debug.Log("hapticEventDataDict.Count == 0");
                }*/

                return;
            }



            foreach (KeyValuePair<HapticEventData, HapticEventRuntimeData> kvp in hapticEventDataDict)
            {
                //Debug.Log("CalculateNewPulse(): " + kvp.Key.hapticType);
                //get sample data (regular flow)
                float lerp = (Time.unscaledTime - kvp.Value.startTime) / kvp.Value.length;
                float wrappedLerp = lerp % 1;

                int sampleIndex = (int)(kvp.Value.Samples * wrappedLerp);
                float sampleSize = globalPulseDuration * kvp.Value.Frequency;

                bool positionalSamples = false;

                //get sample data (positional type)
                if (kvp.Key.hapticType == HapticType.Positional)
                {
                    positionalSamples = true;

                    PGVRLinearConstraint linearConstraint = kvp.Value.sourceTransform.GetComponent<PGVRLinearConstraint>();

                    float prevLineairLerp = linearConstraint.PreviousLinearMapping;
                    float currentLineairLerp = linearConstraint.LinearMapping;
                    float linearRange = Mathf.Abs(currentLineairLerp - prevLineairLerp);
                    float minLineairLerp = Mathf.Min(prevLineairLerp, currentLineairLerp);

                    //do timewarping stuff here
                    sampleIndex = (int)(kvp.Value.Samples * minLineairLerp);
                    sampleSize = globalPulseDuration * kvp.Value.Frequency;
                    float lerpDuration = kvp.Key.GetLerpDuration(prevLineairLerp, currentLineairLerp);

                    //Debug.Log("lerpDuration " + lerpDuration);
                    //Debug.Log("minLineairLerp " + minLineairLerp);
                    //Debug.Log("linearRange " + linearRange);
                }

                //update time
                if (kvp.Value.IsEnded(Time.unscaledTime))
                {
                    toRemove.Add(kvp.Key);
                }


                float[] samples = kvp.Value.GetSmoothedSamples(sampleIndex, (int)sampleSize, positionalSamples);
                if (samples == null || samples.Length == 0)
                {
                    continue;
                }
                /*string deb2 = "samples: " + sampleIndex + ",s: " + sampleSize + "::";
                foreach (float s in samples)
                {
                    deb2 += "," + s;
                }
                Debug.Log(deb2);*/
                //samplesList.Add(samples);

                //TEST getting parameters
                //float rms, db = 0f;
                //GetPlaceholderAudioParameters(samples, out rms, out db);

                // get mean amplitude from the current samples
                float amplitude = 0f;
                foreach (float sample in samples) { amplitude += sample; }
                amplitude = amplitude / samples.Length;
                //amplitude = Mathf.Clamp(amplitude, calibration.minAmplitude, calibration.maxAmplitude);
                // need to use normalized amplitude in the following
                float amplitudeNormalized = calibration.MapAmplitudeToCalibration(amplitude);

                float frequency = kvp.Value.GetFrequencyFromSamples(wrappedLerp, positionalSamples);
                //frequency = Mathf.Clamp(frequency, calibration.minFrequency, calibration.maxFrequency);
                // need to use normalized frequency in the following
                float frequencyNormalized = calibration.MapFrequencyToCalibration(frequency);


                // get range of amplitudes for the current HapticClip
                float minAmpNorm, maxAmpNorm;
                kvp.Value.GetSmoothedSamplesRange(out minAmpNorm, out maxAmpNorm, positionalSamples);
                // map min and max based on calibration data
                minAmpNorm = calibration.MapAmplitudeToCalibration(minAmpNorm);
                maxAmpNorm = calibration.MapAmplitudeToCalibration(maxAmpNorm);
                float clipAmpRange = maxAmpNorm - minAmpNorm;
                float meanAmpNorm = minAmpNorm + clipAmpRange / 2f;

                // get range of frequencies for the current HapticClip
                float minFreqNorm, maxFreqNorm;
                kvp.Value.GetFrequenciesRange(out minFreqNorm, out maxFreqNorm, positionalSamples);
                // map min and max based on calibration data
                minFreqNorm = calibration.MapFrequencyToCalibration(minFreqNorm);
                maxFreqNorm = calibration.MapFrequencyToCalibration(maxFreqNorm);
                float clipFreqRange = maxFreqNorm - minFreqNorm;
                float meanFreqNorm = minFreqNorm + clipFreqRange / 2f;


                // USER INPUT: deal with Expressiveness here (amplitude)
                // we compute the range of the audio clip and boost up or down data based on their distance from the mean
                // alternative: we could also use the following function (?) f(x) = (a^x - 1) / (a - 1)
                float distToMeanNorm = (amplitudeNormalized - meanAmpNorm) / clipAmpRange;
                float sinusoid = (1 / (1 + Mathf.Exp(-kvp.Key.expressiveness * 10f * distToMeanNorm))); //gives a result between [0;1]
                float expressivenessAmp = amplitudeNormalized * (2 * sinusoid - 1) * calibration.expressivenessMagnitude;

                // apply expressiveness to pitch
                float expressivenessFreq = 0f;
                // possible that no pitches are detected, in which case the expressiveness would be NaN
                if (meanFreqNorm != 0f)
                {
                    distToMeanNorm = (frequencyNormalized - meanFreqNorm) / clipFreqRange;
                    sinusoid = (1 / (1 + Mathf.Exp(-kvp.Key.expressiveness * 10f * distToMeanNorm))); //gives a result between [0;1]
                    expressivenessFreq = frequencyNormalized * (2 * sinusoid - 1) * calibration.expressivenessMagnitude;
                }


                // USER INPUT: deal with Intensity here (amplitude)
                // absolute boost of amplitude
                float intensity = kvp.Key.intensity * calibration.intensityMagnitude;


                // USER INPUT: deal with Modulation here (pitch)
                // absolute boost of frequency
                float modulation = kvp.Key.modulation * calibration.modulationMagnitude;


                // USER INPUT: deal with Randomness here (both)
                float randomnessAmp = kvp.Key.randomness * Random.Range(-calibration.randomnessMagnitude, calibration.randomnessMagnitude);
                float randomnessFreq = kvp.Key.randomness * Random.Range(-calibration.randomnessMagnitude, calibration.randomnessMagnitude);

                /*
                 //db += kvp.Key.amplitudeGlobal;
                 float velocitymultiplier = Mathf.Clamp01((CurrentVelocity.magnitude - minimumVelocity) / velocityMagicNumber);
                 velocitymultiplier *= kvp.Key.amplitudeSpeedVelocity;
                 db *= velocitymultiplier;

                Debug.Log("velocitymultiplier = " + velocitymultiplier);
                Debug.Log("db = " + db);
                */


                

                float freqBias = (frequencyNormalized * 2 - 1) * calibration.frequencyMagnitude;
                if (frequencyNormalized == 0f)
                    freqBias = 0f;
                //Debug.Log("AN: " + amplitudeNormalized + "FB: " + freqBias + "EX: " + expressiveness + "RA: " + randomnessAmp + "I: " + intensity);
                float amplitudeResult = Mathf.Clamp01(amplitudeNormalized + expressivenessAmp + randomnessAmp + intensity);
                if (calibration.device == AudioCalibration.Device.NativeActuator)
                    amplitudeResult = Mathf.Clamp01(amplitudeResult + freqBias);
                float frequencyResult = Mathf.Clamp01(frequencyNormalized + expressivenessFreq + randomnessFreq + modulation);


                // USER INPUT: deal with Velocity here
                // apply velocity to amplitude AND frequency?
                // also, need to take into account the type of the event (continuous passive or continuous active)
                float velocityMultiplier = 0f;

                if (kvp.Key.hapticType == HapticType.ContinuousActive)
                {
                    //velocityMultiplier = Mathf.Clamp01((CurrentVelocity.magnitude - minimumVelocity) / velocityMagicNumber);

                    velocityMultiplier = CurrentVelocity.magnitude / kvp.Key.recordingSpeed;
                    //Debug.Log("current velocity " + CurrentVelocity.magnitude + "; recording velocity " + kvp.Key.recordingSpeed + "; ratio " + velocityMultiplier);

                    amplitudeResult = Mathf.Clamp01(amplitudeResult * velocityMultiplier);
                    frequencyResult = Mathf.Clamp01(frequencyResult * velocityMultiplier);
                }
                else if (kvp.Key.hapticType == HapticType.Positional)
                {
                    if (CurrentVelocity.magnitude < minimumVelocity)
                    {
                        amplitudeResult = 0.0f;
                        frequencyResult = 0.0f;
                    }
                    //velocityMultiplier = Mathf.Clamp01((CurrentVelocity.magnitude - minimumVelocity) / velocityMagicNumber);
                    //Debug.Log("current velocity " + CurrentVelocity.magnitude + "; recording velocity " + kvp.Key.recordingSpeed + "; ratio " + velocityMultiplier);

                    //amplitudeResult = Mathf.Clamp01(amplitudeResult * velocityMultiplier);
                    //frequencyResult = Mathf.Clamp01(frequencyResult * velocityMultiplier);


                    //Debug.Log("AR: " + amplitudeResult + ", FR:" + frequencyResult + " velocity " + CurrentVelocity.magnitude + "; recording velocity " + kvp.Key.recordingSpeed + "; ratio " + velocityMultiplier);
                }


                //dbSum += db;
                //Debug.Log("amplitude " + amplitudeResult + "; frequency " + frequencyResult + "; lerp " + wrappedLerp);
                Debug.Log("amplitude " + amplitudeResult + " (" + amplitudeNormalized + "); expressiveness " + expressivenessAmp + "; intensity " + intensity + "; randomness " + randomnessAmp + "; velocity factor " + velocityMultiplier);
                Debug.Log("frequency " + frequencyResult + " (" + frequencyNormalized + "); expressiveness " + expressivenessFreq + "; modulation " + modulation +  "; randomness " + randomnessFreq + "; velocity factor " + velocityMultiplier);
                amplitudes.Add(amplitudeResult);
                frequencies.Add(frequencyResult);
            }

            //remove ended clips
            foreach (HapticEventData hapticEventData in toRemove)
            {
                //Debug.Log("Delete");
                hapticEventDataDict.Remove(hapticEventData);
            }


            //add all samples together (or calculate average?)
            /*
            int amountClips = samplesList.Count;
            int maxLength = 0;

            foreach (float[] samples in samplesList)
            {
                maxLength = Mathf.Max(maxLength, samples.Length);
            }

            float[] sampleResult = new float[maxLength];

            foreach (float[] samples in samplesList)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    sampleResult[i] += samples[i];
                }
            }

            //trigger pulse
            float db, rms = 0f;
            GetPlaceholderAudioParameters(sampleResult, out rms, out db);

            TriggerPulse(globalPulseDuration, testFrequency, db * 1f);
            */

            // get the maximum amplitude and plays it => probably need to change that
            amplitudes.Sort();
            float amplitudeMax = amplitudes[amplitudes.Count - 1];
            frequencies.Sort();
            float frequencyMax = frequencies[frequencies.Count - 1];

            //if (calibration.device == AudioCalibration.Device.NativeActuator) // oculus controller
            //{
            //    // mix frequency with amplitude: high frequencies dampen amplitude, while low frequencies amplify it
            //    float ratioFrequency = -(frequencyMax * 2 - 1); // get a value between -1f and 1f
            //    amplitudeMax += ratioFrequency * frequencyMagnitude;
            //}
            // we do not modify more for the Nintendo Switch actuator

            //TODO remove this line and write something above
            TriggerPulse(globalPulseDuration, frequencyMax, amplitudeMax);
        }



        public void GetPlaceholderAudioParameters(float[] samples, out float rms, out float db)
        {
            float sum = 0f;

            for (int i = 0; i < samples.Length; i += 1)
            {
                float sample = Mathf.Abs(samples[i]);
                sum += sample * sample;
            }
            int amountSamples = samples.Length;

            // rms = square root of average
            rms = Mathf.Sqrt(sum / amountSamples);

            // RMS value for 0 dB
            float refValue = 0.1f;
            // calculate dB
            db = 20 * Mathf.Log10(rms / refValue);
        }

        #endregion
    }
}
