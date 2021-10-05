using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class TouchEventArgs : EventArgs
    {
        public PGVRInteractableController InteractableController { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocity { get; private set; }

        public TouchEventArgs( PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity)
        {
            this.InteractableController = interactableController;
            this.Velocity = velocity;
            this.AngularVelocity = angularVelocity;
        }
    }
}