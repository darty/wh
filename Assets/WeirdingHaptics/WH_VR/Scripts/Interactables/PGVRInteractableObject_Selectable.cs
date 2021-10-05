using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRInteractableObject_Selectable : MonoBehaviour
    {
        public bool highlightOnSelected = true;
        private PGVRInteractableObject interactable;
        private PGVRInteractableObjectHighlight interactableObjectHighlight;

        //events
        public event EventHandler<SelectEventArgs> OnSelectionDown;
        public event EventHandler<SelectEventArgs> OnSelectionUp;


        private void Awake()
        {
            if (interactableObjectHighlight == null)
                interactableObjectHighlight = this.GetComponent<PGVRInteractableObjectHighlight>();
            if (interactable == null)
                interactable = this.GetComponent<PGVRInteractableObject>();
        }

        /// <summary>
        /// Pointer enters this InteractableObject
        /// </summary>
        /// <param name="interactableControllerPointer"></param>
        public void SelectionStart(PGVRInteractableControllerPointer interactableControllerPointer)
        {
            //Debug.Log("SelectionStart");

            if(highlightOnSelected)
            {
                interactableObjectHighlight?.StartHighlight();
            }
        }


        /// <summary>
        /// Pointer is hovering this InteractableObject
        /// </summary>
        /// <param name="interactableControllerPointer"></param>
        public void SelectionUpdate(PGVRInteractableControllerPointer interactableControllerPointer)
        {
            //Debug.Log("SelectionUpdate");
        }


        /// <summary>
        /// Pointer exists this InteractableObject
        /// </summary>
        /// <param name="interactableControllerPointer"></param>
        public void SelectionExit(PGVRInteractableControllerPointer interactableControllerPointer)
        {
            //Debug.Log("SelectionExit");

            if (highlightOnSelected)
            {
                interactableObjectHighlight?.StopHighlight(false);
            }
        }


        public void SelectionInteractPress(PGVRInteractableControllerPointer interactableControllerPointer)
        {
            //Debug.Log("SelectionInteractPress");
            OnSelectionDown?.Invoke(this, new SelectEventArgs(interactableControllerPointer, interactable));
        }


        public void SelectionInteractRelease(PGVRInteractableControllerPointer interactableControllerPointer)
        {
            OnSelectionUp?.Invoke(this, new SelectEventArgs(interactableControllerPointer, interactable));
        }

    }
}
