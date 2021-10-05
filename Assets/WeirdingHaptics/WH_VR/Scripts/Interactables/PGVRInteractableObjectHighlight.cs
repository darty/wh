using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public abstract class PGVRInteractableObjectHighlight : MonoBehaviour
    {
        //highlight for regular hovering
        public abstract void StartHighlight();
        public abstract void StopHighlight(bool isInGrabRange);
        public abstract void UpdateHighlight(bool isGrabbed);

        //highlight for in range
        public abstract void StartInRangeHighlight();
        public abstract void StopInRangeHighlight();
        public abstract void UpdateInRangeHighlight(bool isGrabbed);

        /*
        public abstract void StartInRangeHighlight();
        public abstract void StopInRangeHighlight();
        public abstract void UpdateInRangeHighlight(bool isGrabbed);

        public abstract void StartTargetedHighlight();
        public abstract void StopTargetedHighlight();
        public abstract void UpdateTargetedHighlight(bool isGrabbed);*/
    }
}
