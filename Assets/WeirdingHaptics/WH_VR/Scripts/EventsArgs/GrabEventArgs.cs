using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class GrabEventArgs : EventArgs
    {
        public PGVRInteractableObject_Grabbable GrabbedObject { get; private set; }
        public PGVRInteractableController InteractableController { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocity { get; private set; }

        public GrabEventArgs(PGVRInteractableObject_Grabbable grabbedObject, PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            this.GrabbedObject = grabbedObject;
            this.InteractableController = interactableController;
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
        }
    }
}
