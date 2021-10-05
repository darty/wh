using PGLibrary.PGVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Polygoat.Haptics
{
    [RequireComponent(typeof(UidComponent))]
    public class HapticInteractable : MonoBehaviour
    {
        private PGVRInteractableObject_Selectable selectable;
        private UidComponent uidComponent;

        public Vector3 worldMenuOffset;


        public HapticData hapticData;

        // keeps track whether the object was selected and its menus are displayed
        private bool selected;

        public enum InteractionState
        {
            Idle,
            Grabbed,
            Touched
        }
        private InteractionState interactionState;
        // need to keep track of grab and touch to avoid conflicts
        private bool grabbed; 
        private bool touched;


        public string Uid
        {
            get
            {
                return uidComponent.Uid;
            }
        }


        private void OnDrawGizmosSelected()
        {
        }


        private void Awake()
        {
            selectable = this.GetComponent<PGVRInteractableObject_Selectable>();
            uidComponent = this.GetComponent<UidComponent>();
        }

        private void Start()
        {
            LoadHapticData();

            PGVRInteractableObject_Grabbable grabbable = this.GetComponent<PGVRInteractableObject_Grabbable>();
            if (grabbable != null)
            {
                grabbable.OnGrabStarted += Grabbable_OnGrabStarted;
                grabbable.OnGrabUpdated += Grabbable_OnGrabUpdated;
                grabbable.OnGrabEnded += Grabbable_OnGrabEnded;
            }

            PGVRTouchable touchable = this.GetComponent<PGVRTouchable>();
            if (touchable != null)
            {
                touchable.OnTouchStarted += Touchable_OnTouchStarted;
                touchable.OnTouchUpdated += Touchable_OnTouchUpdated;
                touchable.OnTouchEnded += Touchable_OnTouchEnded;
            }
        }


        private void OnEnable()
        {
            selectable.OnSelectionDown += Selectable_OnSelectionDown;
            interactionState = InteractionState.Idle;
        }


        private void OnDisable()
        {
            selectable.OnSelectionDown -= Selectable_OnSelectionDown;
            interactionState = InteractionState.Idle;
        }

        public void ForceUnselection()
        {
            selected = false;
        }

        private void Selectable_OnSelectionDown(object sender, SelectEventArgs e)
        {
            if(selected) 
            {
                selected = false;
                HideMenu();
            }
            else
            {
                selected = true;
                ShowMenu();
            }   
        }


        private void HideMenu()
        {
            if (hapticData == null)
            {
                Debug.LogWarning("hapticData is null");
            }

            HapticEditor.Instance.HideMenus(Uid);
        }

        private void ShowMenu()
        {
            if(hapticData == null)
            {
                Debug.LogWarning("hapticData is null");
            }

            HapticEditor.Instance.ShowMenus(this, hapticData);
        }


        public Vector3 GetMenuPosition()
        {
            return this.transform.position + worldMenuOffset;
        }


        private void LoadHapticData()
        {
            hapticData = HapticEditor.Instance.Load(this.Uid);
            if(hapticData == null)
            {
                Debug.LogWarning("hapticData == null");
            }
        }


        public InteractionState GetInteractionState()
        {
            return interactionState;
        }

        #region Grab events


        private void Grabbable_OnGrabStarted(object sender, GrabStartEventArgs e)
        {
            grabbed = true;
            interactionState = InteractionState.Grabbed;
            
            // give information to HapticEditor for recording data
            HapticEditor.Instance.TargetInteractable(this, hapticData);
            HapticEditor.Instance.DisableSelectRayOnController();

            if (hapticData == null || hapticData.grabEventData == null)
                return;

            foreach (HapticEventData hapticEventData in hapticData.grabEventData)
            {
                if (!hapticEventData.enabled)
                    continue;

                PlayHaptics(e.InteractableController, hapticEventData);
            }
        }


        private void Grabbable_OnGrabUpdated(object sender, GrabEventArgs e)
        {
        }


        private void Grabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
        {
            grabbed = false;
            if(touched)
                interactionState = InteractionState.Touched;
            else
            {
                interactionState = InteractionState.Idle;
                HapticEditor.Instance.EnableSelectRayOnController();
            }

            if (hapticData == null || hapticData.grabEventData == null || hapticData.grabEventData.Count == 0)
                return;

            foreach (HapticEventData hapticEventData in hapticData.grabEventData)
            {
                if (!hapticEventData.enabled)
                    continue;

                // stop all events
                StopHaptics(e.InteractableController, hapticEventData);
            }
        }

        #endregion


        #region Touch events

        private void Touchable_OnTouchStarted(object sender, TouchEventArgs e)
        {
            touched = true;
            if(!grabbed)
                interactionState = InteractionState.Touched;

            // give information to HapticEditor for recording data
            HapticEditor.Instance.TargetInteractable(this, hapticData);
            HapticEditor.Instance.DisableSelectRayOnController();

            if (hapticData == null || hapticData.touchEventData == null)
                return;

            foreach (HapticEventData hapticEventData in hapticData.touchEventData)
            {
                if (!hapticEventData.enabled)
                    continue;

                PlayHaptics(e.InteractableController, hapticEventData);
            }
        }


        private void Touchable_OnTouchUpdated(object sender, TouchEventArgs e)
        {

        }


        private void Touchable_OnTouchEnded(object sender, TouchEventArgs e)
        {
            touched = false;
            // this case should never happen, but who knows...
            if(grabbed)
                interactionState = InteractionState.Grabbed;
            else
            {
                interactionState = InteractionState.Idle;
                HapticEditor.Instance.EnableSelectRayOnController();
            }

            if (hapticData == null || hapticData.touchEventData == null || hapticData.touchEventData.Count == 0)
                return;

            foreach (HapticEventData hapticEventData in hapticData.touchEventData)
            {
                if (!hapticEventData.enabled)
                    continue;

                // stop all events
                StopHaptics(e.InteractableController, hapticEventData);
            }
        }


        #endregion


        #region Haptics

        private void PlayHaptics(PGVRInteractableController interactableController, HapticEventData hapticEventData)
        {
            if (hapticEventData == null)
                return;

            HapticDevice hapticDevice = interactableController.GetComponent<HapticDevice>();
            if (hapticDevice == null)
                return;

            hapticDevice.PlayHapticEvent(hapticEventData, this.transform);
        }

        private void StopHaptics(PGVRInteractableController interactableController, HapticEventData hapticEventData)
        {
            Debug.Log("StopHaptics");
            if (hapticEventData == null)
                return;

            HapticDevice hapticDevice = interactableController.GetComponent<HapticDevice>();
            if (hapticDevice == null)
                return;

            hapticDevice.StopHapticEvent(hapticEventData);
        }

        #endregion


        #region Position interpolation

        public float GetInterpolation()
        {
            PGVRLinearConstraint linearConstraint = this.GetComponent<PGVRLinearConstraint>();
            if(linearConstraint != null)
            {
                return linearConstraint.LinearMapping;
            }

            return 0f;
        }

        public bool HasInterpolation()
        {
            return this.GetComponent<PGVRLinearConstraint>() != null;
        }

        #endregion
    }
}
