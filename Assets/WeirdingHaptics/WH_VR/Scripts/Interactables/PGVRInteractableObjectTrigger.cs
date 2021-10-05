using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRInteractableObjectTrigger : MonoBehaviour
    {
        public event EventHandler<InteractableEnterTriggerEventArgs> OnInteractableEnteredContainer;


        private void OnTriggerEnter(Collider other)
        {
            PGVRInteractableObject interactableObject = other.attachedRigidbody?.GetComponent<PGVRInteractableObject>();
            if(interactableObject != null)
            {
                InteractableEnteredContainer(interactableObject);
            }
        }


        protected virtual void InteractableEnteredContainer(PGVRInteractableObject interactableObject)
        {
            OnInteractableEnteredContainer?.Invoke(this, new InteractableEnterTriggerEventArgs(interactableObject));
        }


    }
}
