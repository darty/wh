using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Polygoat.Haptics
{
    public enum HapticType
    {
        Instantaneous,
        ContinuousPassive,
        ContinuousActive,
        Positional,
        Distance, // for legacy purpose
        Continuous // for legacy purpose
    }


    [Serializable]
    public class HapticEventData
    {
        public string audioClipSubPath;
        public string AudioClipFullPath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, audioClipSubPath);
            }
        }

        public string audioClipScaledSubPath;
        public string AudioClipScaledFullPath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, audioClipScaledSubPath);
            }
        }

        public bool enabled = true;
        public HapticType hapticType;

        //settings
        public int instantTimes = 1;

        //pitch
        public float pitchGlobal;
        public float pitchSpeedVelocity;
        public float pitchSpeedAngularVelocity;
        public float pitchSize;
        public float pitchRoughness;
        public float pitchDistance;

        //amplitude
        public float amplitudeGlobal;
        public float amplitudeSpeedVelocity;
        public float amplitudeSpeedAngularVelocity;
        public float amplitudeSize;
        public float amplitudeRoughness;
        public float amplitudeDistance;

        // user input
        [Range(0f, 1f)]
        public float expressiveness = 0f;
        [Range(-1f, 1f)]
        public float intensity = 0f;
        [Range(-1f, 1f)]
        public float modulation = 0f;
        [Range(0f, 1f)]
        public float randomness = 0f;

        // record speed of movement when recording to compute ratio with playing speed
        public float recordingSpeed;


        public AudioClip AudioClip { get; set; }

        private HapticClip hapticClip;
        public HapticClip HapticClip
        {
            get
            {
                if (hapticClip == null)
                {
                    hapticClip = new HapticClip(AudioClip);
                    hapticClip.SetFrequencies(savedPitches.ToArray());
                }
                return hapticClip;
            }
        }

        //positional data
        public List<float> recordingTimes = new List<float>();
        public List<float> recordingLerps = new List<float>();

        

        public AudioClip AudioClipScaled { get; set; }
        private HapticClip hapticClipScaled;
        public HapticClip HapticClipScaled
        {
            get
            {
                if (hapticClipScaled == null)
                {
                    hapticClipScaled = new HapticClip(AudioClipScaled);
                    hapticClipScaled.SetFrequencies(savedPitches.ToArray());
                }
                return hapticClipScaled;
            }
        }

        public List<float> scaledTimes = new List<float>();
        public List<float> scaledLerps = new List<float>();


        public List<float> savedPitches = new List<float>();


        public void AddPositionalData(float time, float lerp)
        {
            Debug.Log("AddingPositionalData: " + time + ", " + lerp);
            recordingTimes.Add(time);
            recordingLerps.Add(lerp);
        }


        public void PrepareForRecord()
        {
            recordingTimes.Clear();
            recordingLerps.Clear();
        }


        public void SetAudioClip(AudioClip clip, float[] freqs)
        {
            AudioClip = clip;
            hapticClip = new HapticClip(AudioClip);
            savedPitches = new List<float>(freqs);
            hapticClip.SetFrequencies(freqs);
        }

        public void SetAudioClipScaled(AudioClip clip, float[] freqs)
        {
            AudioClipScaled = clip;
            hapticClipScaled = new HapticClip(AudioClipScaled);
            savedPitches = new List<float>(freqs);
            hapticClipScaled.SetFrequencies(freqs);
        }


        public float GetLerpDuration(float startLerp, float endLerp)
        {
            //search recordingTimes for corresponding lerps
            float startTime = 0f;
            float endTime = 0f;
            bool foundStart, foundEnd;
            foundStart = foundEnd = false;

            for (int i = 0; i < recordingLerps.Count; i++)
            {
                if (!foundStart)
                {
                    if (recordingLerps[i] < startLerp)
                        startTime = recordingTimes[i];
                    else
                        foundStart = true;
                }

                if (!foundEnd)
                {
                    if (recordingLerps[i] < endLerp)
                        endTime = recordingTimes[i];
                    else
                        foundEnd = true;
                }
            }

            return Mathf.Abs(endTime - startTime);
        }

    }



    public class HapticEventRuntimeData
    {
        public HapticEventData hapticEventData;
        public float startTime;
        public float endTime;
        public bool loop;
        public int repeatTimes;
        public int timesPlayed;
        public float length;

        public float Samples { get { return hapticEventData.HapticClip.Samples; } }
        public float Frequency { get { return hapticEventData.HapticClip.Frequency; } }

        public bool TESTvelocity = true;

        public Transform sourceTransform;



        public HapticEventRuntimeData(HapticEventData hapticEventData)
        {
            this.hapticEventData = hapticEventData;
            startTime = Time.unscaledTime;

            length = hapticEventData.HapticClip.Length;
            endTime = startTime + length;

            if (hapticEventData.hapticType == HapticType.Continuous || hapticEventData.hapticType == HapticType.ContinuousActive || hapticEventData.hapticType == HapticType.ContinuousPassive || hapticEventData.hapticType == HapticType.Distance || hapticEventData.hapticType == HapticType.Positional)
                loop = true;
            else
                loop = false;

            if (hapticEventData.hapticType == HapticType.Instantaneous && hapticEventData.instantTimes > 1)
            {
                // loop = true;
                repeatTimes = hapticEventData.instantTimes;

                endTime = startTime + length * repeatTimes;
            }
        }

        public bool IsEnded(float currentTime)
        {
            if (loop)
                return false;

            bool isEnded = endTime <= currentTime;
            return isEnded;
        }

        public float[] GetSamples(int index, int sampleSize)
        {
            return hapticEventData.HapticClip.GetSamples(index, sampleSize);
        }

        public float[] GetSmoothedSamples(int index, int sampleSize, bool positionalSamples = false)
        {
            if (positionalSamples)
            {
                HapticClip clip = hapticEventData.HapticClipScaled;
                if (clip != null && clip.valid)
                {
                    return clip.GetSmoothedSamples(index, sampleSize);
                }
            }
            return hapticEventData.HapticClip.GetSmoothedSamples(index, sampleSize);
        }

        public void GetSmoothedSamplesRange(out float min, out float max, bool positionalSamples = false)
        {
            if (positionalSamples)
            {
                HapticClip clip = hapticEventData.HapticClipScaled;
                if (clip != null && clip.valid)
                {
                    clip.GetSmoothedSamplesRange(out min, out max);
                    return;
                }
            }
            hapticEventData.HapticClip.GetSmoothedSamplesRange(out min, out max);
        }

        public float GetFrequencyFromSamples(float lerp, bool positionalSamples = false)
        {
            if (positionalSamples)
            {
                HapticClip clip = hapticEventData.HapticClipScaled;
                if (clip != null && clip.valid)
                {
                    return clip.GetFrequencyFromSamples(lerp);
                }
            }
            return hapticEventData.HapticClip.GetFrequencyFromSamples(lerp);
        }

        public void GetFrequenciesRange(out float min, out float max, bool positionalSamples = false)
        {
            if (positionalSamples)
            {
                HapticClip clip = hapticEventData.HapticClipScaled;
                if (clip != null && clip.valid)
                {
                    clip.GetFrequenciesRange(out min, out max);
                    return;
                }
            }
            hapticEventData.HapticClip.GetFrequenciesRange(out min, out max);
        }
    }


    [Serializable]
    public class HapticData
    {
        public string instanceUid;
        public List<HapticEventData> grabEventData = new List<HapticEventData>();
        public List<HapticEventData> touchEventData = new List<HapticEventData>();
        public List<HapticEventData> distanceEventData = new List<HapticEventData>();

        public HapticData(string instanceUid)
        {
            this.instanceUid = instanceUid;
        }


        public void DeleteGrabEventData(int index)
        {
            Debug.Log("DeleteGrabEventData(int index) " + index);
            if (index < 0 || index >= grabEventData.Count)
            {
                Debug.LogWarning("Grab index out of range (" + index + ")");
                return;
            }
            grabEventData.RemoveAt(index);
        }


        public void AddGrabEventData()
        {
            if (grabEventData == null)
                grabEventData = new List<HapticEventData>();

            grabEventData.Add(new HapticEventData());
        }


        public void DeleteTouchEventData(int index)
        {
            Debug.Log("DeleteTouchEventData(int index) " + index);
            if (index < 0 || index >= touchEventData.Count)
            {
                Debug.LogWarning("Touch index out of range (" + index + ")");
                return;
            }
            touchEventData.RemoveAt(index);
        }


        public void AddTouchEventData()
        {
            if (touchEventData == null)
                touchEventData = new List<HapticEventData>();

            touchEventData.Add(new HapticEventData());
        }


        public void DeleteDistanceEventData(int index)
        {
            distanceEventData.RemoveAt(index);
        }


        public void AddDistanceEventData()
        {
            if (distanceEventData == null)
                distanceEventData = new List<HapticEventData>();

            HapticEventData hapticEventData = new HapticEventData();
            hapticEventData.hapticType = HapticType.Distance;
            distanceEventData.Add(hapticEventData);
        }

    }
}
