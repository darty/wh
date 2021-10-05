using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class InteractableEnterTriggerEventArgs : EventArgs
    {
        public PGVRInteractableObject InteractableObject { get; private set; }



        public InteractableEnterTriggerEventArgs(PGVRInteractableObject interactableObject)
        {
            this.InteractableObject = interactableObject;
        }
    }
}
