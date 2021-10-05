using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Polygoat.Haptics
{
    public class TouchMenu : LightHapticEventMenu
    {


        public override List<HapticEventData> GetCurrentHapticEventDataList()
        {
            if (HapticData == null)
                return null;
            else
                return HapticData.touchEventData;
        }


        //protected override void AddEventData()
        //{
        //    HapticData.AddTouchEventData();
        //}


        protected override void RemoveEventData(int currentIndex)
        {
            HapticData.DeleteTouchEventData(currentIndex);
            HapticEditor.Instance.RemoveTouchLayer(currentIndex);
            HapticEditor.Instance.RepositionTouchMenus();
        }


    }
}