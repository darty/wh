using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PGLibrary.PGVR
{
    public class SelectEventArgs : EventArgs
    {
        public PGVRInteractableControllerPointer InteractableControllerPointer { get; private set; }
        public PGVRInteractableObject InteractableObject { get; private set; }


        public SelectEventArgs(PGVRInteractableControllerPointer interactableControllerPointer, PGVRInteractableObject interactableObject)
        {
            this.InteractableControllerPointer = interactableControllerPointer;
            this.InteractableObject = interactableObject;
        }
    }
}