using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Polygoat.Haptics
{
    public class DistanceMenu : HapticEventMenu
    {
        [Header("Distance")]
        public Slider sliderPitchDistance;
        public Slider sliderLoudnessDistance;


        public override List<HapticEventData> GetCurrentHapticEventDataList()
        {
            return HapticData.distanceEventData;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            sliderPitchDistance.onValueChanged.AddListener(HandlePitchDistanceValueChanged);
            sliderLoudnessDistance.onValueChanged.AddListener(HandleLoudnessDistanceValueChanged);
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            sliderPitchDistance.onValueChanged.RemoveAllListeners();
            sliderLoudnessDistance.onValueChanged.RemoveAllListeners();
        }



        protected override void UpdateForm()
        {
            base.UpdateForm();
            if (CurrentHapticEventData != null)
            {
                sliderPitchDistance.value = CurrentHapticEventData.pitchDistance;
                sliderLoudnessDistance.value = CurrentHapticEventData.amplitudeDistance;
            }
        }


        protected override void AddEventData()
        {
            HapticData.AddDistanceEventData();
        }


        private void HandlePitchDistanceValueChanged(float value)
        {
            CurrentHapticEventData.pitchDistance = value;
        }


        private void HandleLoudnessDistanceValueChanged(float value)
        {
            CurrentHapticEventData.amplitudeDistance = value;
        }


    }
}