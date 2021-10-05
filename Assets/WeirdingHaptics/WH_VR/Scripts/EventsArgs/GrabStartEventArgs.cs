using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class GrabStartEventArgs : GrabEventArgs
    {
        public PGVRGrabType GrabType { get; private set; }



        public GrabStartEventArgs(PGVRInteractableObject_Grabbable grabbedObject, PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity, PGVRGrabType grabType) : base(grabbedObject, interactableController, velocity, angularVelocity)
        {
            this.GrabType = grabType;
        }
    }
}
