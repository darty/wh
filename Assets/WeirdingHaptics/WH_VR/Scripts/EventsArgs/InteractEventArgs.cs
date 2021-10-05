using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class InteractEventArgs : EventArgs
    {
        public PGVRInteractableObject InteractableObject { get; private set; }
        public PGVRInteractableController InteractableController { get; private set; }
        public PGVRGrabType GrabType { get; private set; }

        public InteractEventArgs(PGVRInteractableObject interactableObject, PGVRInteractableController interactableController, PGVRGrabType grabType = PGVRGrabType.None)
        {
            this.InteractableObject = interactableObject;
            this.InteractableController = interactableController;
            this.GrabType = grabType;
        }
    }
}
