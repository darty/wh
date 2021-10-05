using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Polygoat.Haptics
{
    public class GrabMenu : LightHapticEventMenu
    {

        public override List<HapticEventData> GetCurrentHapticEventDataList()
        {
            if (HapticData == null)
                return null;
            else
                return HapticData.grabEventData;
        }


        //protected override void AddEventData()
        //{
        //    HapticData.AddGrabEventData();
        //}



        protected override void RemoveEventData(int currentIndex)
        {
            HapticData.DeleteGrabEventData(currentIndex);
            HapticEditor.Instance.RemoveGrabLayer(currentIndex);
            HapticEditor.Instance.RepositionGrabMenus();
        }


    }
}