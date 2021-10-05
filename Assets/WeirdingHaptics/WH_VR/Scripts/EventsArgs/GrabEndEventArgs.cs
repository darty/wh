using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PGLibrary.PGVR
{
    public class GrabEndEventArgs : GrabEventArgs
    {
        public bool GrabbedByOtherController { get; private set; }


        public GrabEndEventArgs(PGVRInteractableObject_Grabbable grabbedObject, PGVRInteractableController interactableController, Vector3 velocity, Vector3 angularVelocity, bool grabbedByOtherController) : base (grabbedObject, interactableController, velocity, angularVelocity)
        {
            this.GrabbedByOtherController = grabbedByOtherController;
        }
    }
}
