using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace Polygoat.Haptics
{
    public class AudioAnalyzer : MonoBehaviour
    {
        private float refValue = 0.1f; // RMS value for 0 dB
       // private int amountSamples = 1024;  // array size
        private float threshold = 0.02f;      // minimum amplitude to extract pitch
        private float _fSample;
        public float minDb = -160f;


        // Start is called before the first frame update
        void Start()
        {
            _fSample = AudioSettings.outputSampleRate;
       
        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// Based on https://answers.unity.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html
        /// </summary>
        /// <param name="samples"></param>
        public void AnalyzeSound(AudioSource audioSource, int amountSamples, out float rms, out float db, out float pitch)
        {
            //get samples
            float[] samples = new float[amountSamples];
            audioSource.GetOutputData(samples, 0);

            float sum = 0f;
            for (int i = 0; i < amountSamples; i++)
            {
                sum += samples[i] * samples[i]; // sum squared samples
            }

            // rms = square root of average
            rms = Mathf.Sqrt(sum / amountSamples);

            // calculate dB
            db = 20 * Mathf.Log10(rms / refValue);
            // clamp it to -160dB min
            db = Mathf.Max(db, minDb);

            // get sound spectrum
            float[] spectrum = new float[amountSamples];
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            float maxV = 0f;
            int maxN = 0;
            for (int i = 0; i < amountSamples; i++)
            { 
                // find max 
                if (spectrum[i] > maxV && spectrum[i] > threshold)
                {
                    maxV = spectrum[i];
                    maxN = i; // maxN is the index of max
                }
            }

            Debug.Log("maxV: " + maxV);
            Debug.Log("maxN: " + maxN);

            float freqN = maxN; // pass the index to a float variable
            if (maxN > 0 && maxN < amountSamples - 1)
            { 
                // interpolate index using neighbours
                float dL = spectrum[maxN - 1] / spectrum[maxN];
                var dR = spectrum[maxN + 1] / spectrum[maxN];
                freqN += 0.5f * (dR * dR - dL * dL);
            }
            Debug.Log("freqN: " + freqN);

            pitch = freqN * (_fSample / 2) / amountSamples; // convert index to frequency
        }


    }
}