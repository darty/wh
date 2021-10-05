using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRHandRenderer : MonoBehaviour
    {
        public GameObject HandInstance { get; private set; }
        public bool IsVisible { get; private set; }

        //events
        public event EventHandler<EventArgs> OnModelLoaded;


        /// <summary>
        /// Load the hand model.
        /// implementation must call RaiseModelLoaded when the model is loaded
        /// </summary>
        public virtual void InitHandRenderer()
        {
           
        }


        /// <summary>
        /// PGVRHandRenderer must raise this event when the hand model is loaded
        /// </summary>
        protected void RaiseModelLoaded(GameObject loadedHandModel)
        {
            Debug.Log("PGVRHandRenderer RaiseModelLoaded");

            HandInstance = loadedHandModel;
            OnModelLoaded?.Invoke(this, EventArgs.Empty);
        }



        public virtual void HideController(bool permanent = false)
        {
            IsVisible = false;
        }


        public virtual void ShowController(bool permanent = false)
        {
            IsVisible = true;
        }


    }
}
