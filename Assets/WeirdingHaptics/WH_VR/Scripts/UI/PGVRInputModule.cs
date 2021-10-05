
namespace PGLibrary.PGVR
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    // cursor uses controllers array, GetDistance and UpdateDistance
    public class PGVRInputModule : StandaloneInputModule
    {
        public PGVRInteractableControllerPointer[] pointers;

        private readonly Dictionary<int, VRPointerEventData> pointerDataDictionary = new Dictionary<int, VRPointerEventData>();



        protected override void Start()
        {
            base.Start();

        }


        public override void Process()
        {
            SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
                SendMoveEventToSelectedObject();

            ProcessControllerEvents();
        }


        private void ProcessControllerEvents()
        {
            for (int i = 0; i < pointers.Length; i++)
            {
                bool released;
                bool pressed;
                VRPointerEventData pointerEventData = GetVRPointerEventData(i, out pressed, out released);

                ProcessTouchPress(pointerEventData, pressed, released);

                if (!released)
                {
                    ProcessMove(pointerEventData);
                    ProcessDrag(pointerEventData);
                }
            }
        }


        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            var press = pointerEvent.pointerPress;
            var pressRaw = pointerEvent.rawPointerPress;
            var eligibleForClick = pointerEvent.eligibleForClick;

            base.ProcessDrag(pointerEvent);

            pointerEvent.pointerPress = press;
            pointerEvent.rawPointerPress = pressRaw;
            pointerEvent.eligibleForClick = eligibleForClick;
        }


        private VRPointerEventData GetVRPointerEventData(int controllerIndex, out bool pressed, out bool released)
        {
            PGVRInteractableControllerPointer pointer = pointers[controllerIndex];

            VRPointerEventData pointerData;
            GetVRPointerData(controllerIndex, out pointerData, true);

            pointerData.Reset();
            pointerData.ray = new Ray(pointer.transform.position, pointer.transform.forward);

            if (!pointer.GetPressedReleased(out pressed, out released))
            {
                return pointerData;
            }

            pointerData.button = PointerEventData.InputButton.Left;

            Vector2 prevPos = pointerData.position;

            // Event system orders results by depth and sorting layers
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

            // Find first raycast where gameobject if not null
            RaycastResult raycastResult = FindFirstRaycast(m_RaycastResultCache);

            //pass information to pointer
            pointer.SetUIHit(raycastResult.isValid, raycastResult.worldPosition, raycastResult.worldNormal, raycastResult.distance);
            pointer.UpdateEventSystem();

            pointerData.position = raycastResult.screenPosition;
            PointerDataPositionToDraggedObject(pointerData);

            pointerData.pointerCurrentRaycast = raycastResult;
            pointerData.delta = pointerData.position - prevPos;

            m_RaycastResultCache.Clear();
            return pointerData;
        }



        private static void PointerDataPositionToDraggedObject(VRPointerEventData pointerData)
        {
            if (pointerData.pointerDrag != null)
            {
                VRPointerEventData vrData = pointerData as VRPointerEventData;
                Plane plane = new Plane(vrData.pointerDrag.transform.forward, vrData.pointerDrag.transform.position);
                float distance;

                if (plane.Raycast(vrData.ray, out distance))
                {
                    var worldPos = vrData.ray.GetPoint(distance);
                    pointerData.position = vrData.pressEventCamera.WorldToScreenPoint(worldPos);
                }
            }
        }

        private bool GetVRPointerData(int controllerId, out VRPointerEventData data, bool create)
        {
            if (!pointerDataDictionary.TryGetValue(controllerId, out data) && create)
            {
                data = new VRPointerEventData(eventSystem)
                {
                    pointerId = controllerId
                };
                pointerDataDictionary.Add(controllerId, data);
                return true;
            }
            return false;
        }
    }
}

