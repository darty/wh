using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Polygoat.Haptics
{
    public class HapticClip
    {

        /// <summary>
        /// AudioClip has to be loaded with "DecompressOnLoad"
        /// </summary>
        public AudioClip audioClip;

        public bool valid;
        private float[] audioData;
        private float[] smoothedData;
        private float[] smoothDataBounds; // min and max of smoothed data
        private float[] frequencies; // the frequency array is smaller than the sample arrays because we compute frequencies over bunches of samples
        private float[] frequenciesBounds; // min and max of frequencies
        private SmoothingAudioInput smoothing;

        public float Length { get { return audioClip.length; } }
        public int Samples { get { return audioClip.samples; } }
        public int Frequency { get { return audioClip.frequency; } }

        private float volume;

        //add filters here


        private const float velocityMagicNumber = 5f;
        private const float minimumVelocity = 0.1f;


        /// <summary>
        /// Create HapticClip based on AudioClip
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="channel">0 for left, 1 for right</param>
        public HapticClip(AudioClip audioClip, int channel = 0)
        {
            this.audioClip = audioClip;
            this.valid = false;
            if (audioClip == null)
            {
                Debug.LogError("Audioclip is null");
                return;
            }
            this.valid = true;

            audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);
            
            smoothing = UnityEngine.Object.FindObjectOfType<SmoothingAudioInput>();
            if (!smoothing) Debug.LogError("No SmoothingAudioInput object in the scene");

            // need to keep smoothen data in cache for better smoothing
            smoothedData = new float[audioData.Length];
            smoothing.SmoothenClip(audioData, smoothedData);
            // set smoothDataBounds to the min and max values of the smoothed data
            UpdateSmoothedSamplesRange();

            Initialize();
        }

        public void SetFrequencies(float[] freqs)
        {
            frequencies = freqs;
            UpdateFrequencyRange();
        }


        private void Initialize()
        {
            volume = 1f;
        }


        public void Play()
        {

        }

        public float GetDuration()
        {
            // should not hardcode sample rate
            return audioClip.samples / 44100f;
        }

        public float[] GetAllSamples()
        {
            return audioData;
        }

        public float[] GetSamples(int index, int sampleSize)
        {
            int startIndex = index * audioClip.channels;
            int endIndex = startIndex + sampleSize * audioClip.channels;
            endIndex = Mathf.Min(endIndex, audioData.Length);
            int length = endIndex - startIndex;

            float[] samples = new float[length];
            Array.Copy(audioData, startIndex, samples, 0, length);
            return samples;
        }


        public float[] GetSmoothedSamples(int index, int sampleSize)
        {
            int startIndex = index * audioClip.channels;
            int endIndex = startIndex + sampleSize * audioClip.channels;
            endIndex = Mathf.Min(endIndex, smoothedData.Length);
            int length = endIndex - startIndex;

            float[] samples = new float[length];
            if (startIndex + length > smoothedData.Length)
            {
                length = smoothedData.Length - startIndex;
            }
            if (length == 0)
            {
                return null;
            }
            Array.Copy(smoothedData, startIndex, samples, 0, length);
            return samples;
        }


        public float[] GetSmoothedSamplesRange()
        {
            if (smoothDataBounds == null) { Debug.LogError("Range of smoothed data not calculated yet"); return null; }
            float[] copy = { smoothDataBounds[0], smoothDataBounds[1] };
            return copy;
        }

        public void GetSmoothedSamplesRange(out float min, out float max)
        {
            min = max = 0f;

            if (smoothDataBounds == null) { Debug.LogError("Range of smoothed data not calculated yet"); return; }

            min = smoothDataBounds[0];
            max = smoothDataBounds[1];
        }

        private void UpdateSmoothedSamplesRange()
        {
            if (smoothedData == null) { Debug.LogError("No smoothed data for this clip"); return; }
            
            smoothDataBounds = new float[2];
            smoothDataBounds[0] = float.MaxValue;
            smoothDataBounds[1] = float.MinValue;

            foreach (float sample in smoothedData)
            {
                smoothDataBounds[0] = Mathf.Min(smoothDataBounds[0], sample);
                smoothDataBounds[1] = Mathf.Max(smoothDataBounds[1], sample);
            }
        }


        // get the frequency associated to samples using the Lerp from the HapticDevice loop
        public float GetFrequencyFromSamples(float lerp)
        {
            if (frequencies == null || frequencies.Length == 0)
            {
                Debug.LogError("Frequencies not loaded yet");
                return 0.0f;
            }
            if (0f > lerp || lerp > 1f)
            {
                Debug.LogError("Lerp should be betwen 0f and 1f");
                lerp = Mathf.Clamp01(lerp);
            }
            return frequencies[Mathf.FloorToInt(frequencies.Length * lerp)];
        }


        public float[] GetFrequenciesRange()
        {
            if (frequenciesBounds == null) { Debug.LogError("Range of frequencies not calculated yet"); return null; }
            float[] copy = { frequenciesBounds[0], frequenciesBounds[1] };
            return copy;
        }

        public void GetFrequenciesRange(out float min, out float max)
        {
            min = max = 0f;

            if (frequenciesBounds == null) { Debug.LogError("Range of frequencies not calculated yet"); return; }

            min = frequenciesBounds[0];
            max = frequenciesBounds[1];
        }



        private void UpdateFrequencyRange()
        {
            if (frequencies == null) { Debug.LogError("No frequency data for this clip"); return; }

            frequenciesBounds = new float[2];
            frequenciesBounds[0] = float.MaxValue;
            frequenciesBounds[1] = float.MinValue;

            foreach (float freq in frequencies)
            {
                frequenciesBounds[0] = Mathf.Min(frequenciesBounds[0], freq);
                frequenciesBounds[1] = Mathf.Max(frequenciesBounds[1], freq);
            }
        }


        public void GetData(int index, int sampleSize, out float rms, out float db)
        {
            //average samples
            int startIndex = index * audioClip.channels;
            int endIndex = startIndex + sampleSize * audioClip.channels;
            int channelOffset = audioClip.channels;

            endIndex = Mathf.Min(endIndex, audioData.Length);
            float sum = 0f;

            for (int i = index; i < endIndex; i += channelOffset)
            {
                float sample = Mathf.Abs(audioData[i]);
                sum += sample * sample;
            }
            int amountSamples = (endIndex - startIndex) / channelOffset;

            // rms = square root of average
            rms = Mathf.Sqrt(sum / amountSamples);

            // RMS value for 0 dB
            float refValue = 0.1f;
            // calculate dB
            db = 20 * Mathf.Log10(rms / refValue);
        }


        /// <summary>
        /// Returns volume in 0-1 range
        /// </summary>
        /// <returns></returns>
        public float GetVolume()
        {
            return volume;
        }


        public void SetVolumeFromVelocity(float velocity)
        {
            volume = Mathf.Clamp01( (velocity - minimumVelocity) / velocityMagicNumber);
        }
    }
}