using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using PitchDetector;
using Pitch;
using Polygoat.Haptics;

public class SmoothingAudioInput : MonoBehaviour
{
    public enum SmoothingMethod
    {
        RMS, ANALOG
    }

    public enum PitchMethod
    {
        PITCH_TRACKER,
        YIN
    }

    public PitchMethod _pitchMethod = PitchMethod.YIN;

    // audio parameters to smoothen input
    // sample rate of the audio input
    public int sampleRate;
    // attack in ms
    public float attack;
    // release in ms
    public float release;

    // sampling rate
    public int yinPitchesPerSecond;


    // pitch tracker used to compute frequencies of samples
    private PitchTracker pitchTracker;

    private Yin yinPitchTracker;

    private float[] calculatedFreqs;

    private void Awake()
    {
        pitchTracker = new PitchTracker();
        pitchTracker.SampleRate = sampleRate;
        pitchTracker.RecordPitchRecords = true;

        yinPitchTracker = new Yin(sampleRate, sampleRate / yinPitchesPerSecond);
    }

   /* public IEnumerator CalculateFrequencies(HapticClip clip)
    {
        float[] samples = clip.GetAllSamples();
        // float[] frequencies = this.GetFrequenciesFromSignal(samples);
        this.calculateFrequenciesFromSignal(samples);
        clip.setFrequencies(calculatedFreqs);
        yield return null;
    }*/

    /// <summary>
    /// Calculate coefficients for exponential decay (inspired from https://github.com/jatinchowdhury18/audio_dspy)
    /// </summary>
    /// <param name="coeffs">array to update</param>
    /// <param name="tau_ms">time constant [ms]</param>
    /// <param name="fs">sample rate [hz]</param>
    private void calculateCoefficients(float[] coeffs, float tau_ms, float fs)
    {
        coeffs[1] = Mathf.Exp(-1f / (fs * tau_ms / 1000f));
        coeffs[0] = 1f - coeffs[1];
    }

    /// <summary>
    /// Smoothen audio data using RMS with attack and release SmoothingMethod + low-pass filter
    /// Inspired from https://github.com/jatinchowdhury18/audio_dspy
    /// </summary>
    /// <param name="audioSamples"></param>
    /// <param name="smooth">samples smoothed</param>
    /// <param name="SmoothingMethod"></param>
    public void SmoothenClip(float[] audioSamples, float[] smooth, SmoothingMethod SmoothingMethod = SmoothingMethod.ANALOG)
    {
        if (smooth.Length != audioSamples.Length)
            Debug.LogError("Sample and Smooth arrays should have the same length (" + audioSamples.Length + " != " + smooth.Length + ")");

        // correct strange behavior of Microphone creating a burst in the first 300ms of the signal
        // compute the variance of the first 300ms => if very high, we correct for the burst
        //float mean = 0f;
        //int sampleSize = Mathf.FloorToInt(sampleRate * 0.3f);
        //for (int i = 0; i < sampleSize; i++)
        //{
        //    mean = audioSamples[i] * audioSamples[i];
        //}
        //mean /= sampleSize;
        //float sum = 0f;
        //for (int i = 0; i < sampleSize; i++)
        //{
        //    sum += Mathf.Pow(audioSamples[i] * audioSamples[i] - mean, 2);
        //}
        //float variance = sum / sampleSize;
        //Debug.Log("Variance of beginning of signal: " + variance);
        //if (variance < 0.01f)
        //{
        //    for (int i = 0; i < sampleSize; i++)
        //    {
        //        audioSamples[i] = 0f;
        //    }
        //}


        float[] attackCoeffs = new float[2], releaseCoeffs = new float[2];
        calculateCoefficients(attackCoeffs, attack, (float)sampleRate);
        calculateCoefficients(releaseCoeffs, release, (float)sampleRate);

        float level_est = 0f;

        // constants for ANALOG SmoothingMethod
        float alpha = 0.7722f, beta = 0.85872f;

        for (int i = 0; i < audioSamples.Length; i += 1)
        {
            if (SmoothingMethod == SmoothingMethod.RMS)
            {
                float sample = audioSamples[i] * audioSamples[i];
                if (sample > level_est) // attack mode
                    level_est = attackCoeffs[1] * level_est + attackCoeffs[0] * sample;
                else // release mode
                    level_est = releaseCoeffs[1] * level_est + releaseCoeffs[0] * sample;
            }
            else if (SmoothingMethod == SmoothingMethod.ANALOG)
            {
                // ANALOG SmoothingMethod (check https://github.com/jatinchowdhury18/audio_dspy for details)
                float rect = (float)Math.Max(beta * (Math.Exp(alpha * audioSamples[i]) - 1f), 0f);
                if (rect > level_est) // attack mode
                    level_est += attackCoeffs[0] * (rect - level_est);
                else // release mode
                    level_est += releaseCoeffs[0] * (rect - level_est);
            }

            smooth[i] = level_est;
        }

        ApplyLowPassFilter(smooth);
    }

    private void ApplyLowPassFilter(float[] smooth)
    {
        float k = .3f;
        smooth[0] = k * smooth[0];
        for (int i = 1; i < smooth.Length; i++)
        {
            smooth[i] = smooth[i - 1] + k * (smooth[i] - smooth[i - 1]);
        }
    }


    private void ApplyMedianFilter(float[] smooth, int sampleLength = 5)
    {
        float[] median = new float[sampleLength];
        for (int i = 0; i < smooth.Length - sampleLength; i++)
        {
            Array.Copy(smooth, i, median, 0, sampleLength);
            Array.Sort(median);
            smooth[i] = median[median.Length / 2];
        }
    }


    // pitch per seconds should match the frequency of the haptic events
    // WARNING - pitch detection method does not work well for humans apparently
    public float[] GetFrequenciesFromSignal(float[] wav, int pitchPerSeconds = 10)
    {
        Debug.Log("Detecting Pitch...");

        if (_pitchMethod == PitchMethod.PITCH_TRACKER)
        {
            // setting sample rate sets up the tracker
            pitchTracker.PitchRecordsPerSecond = pitchPerSeconds;

            if (pitchTracker.PitchRecords.Count > 0) pitchTracker.Reset();

            pitchTracker.ProcessBuffer(wav);

            string pitches = "Pitches: ";
            Debug.Log("number of pitches recognized: " + pitchTracker.PitchRecords.Count);

            float[] frequencies = new float[pitchTracker.PitchRecords.Count];
            int i = 0;
            foreach (PitchTracker.PitchRecord pr in pitchTracker.PitchRecords)
            {
                pitches += pr.Pitch + " ";
                frequencies[i++] = pr.Pitch;
            }
            Debug.Log(pitches);

            ApplyMedianFilter(frequencies);
            pitches = "Pitches (after filter): ";
            foreach (float freq in frequencies)
            {
                pitches += freq + " ";
            }
            Debug.Log(pitches);

            return frequencies;
        }
        // YIN pitch tracker
        else
        {
            List<float> frequencies = new List<float>();
            var sampleCount = sampleRate / yinPitchesPerSecond;
            var samplesProcessed = 0;
            var srcLength = wav.Length;

            //Debug.Log("srcLength: " + srcLength);

            while (samplesProcessed < srcLength)
            {
                //Debug.Log("samplesProcessed: " + samplesProcessed);
                // int frameCount = Math.Min(srcLength - samplesProcessed, m_pitchBufSize + m_detectOverlapSamples);
                int framecount = srcLength - samplesProcessed;

                List<float> data = new List<float>();

                for (int i = 0; i < sampleCount; i++)
                {
                    data.Add(0.0f);
                }

                // float[] data = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    if (samplesProcessed + i < wav.Length)
                    {
                        data[i] = wav[samplesProcessed + i];
                    }
                }
                // Array.Copy(wav, samplesProcessed, data, 0, sampleCount);

                PitchDetectionResult result = yinPitchTracker.getPitch(data.ToArray());
                //Debug.Log("Yin Pitch: " + result.getPitch());
                float pitch = Mathf.Max(result.getPitch(), 0f);

                frequencies.Add(pitch);

                samplesProcessed += sampleCount;
            }

            float[] outFreq = frequencies.ToArray();

            ApplyMedianFilter(outFreq);

            return outFreq;
        }
    }
}
