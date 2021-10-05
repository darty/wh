using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    /// <summary>
    /// A package of InteractableObjects that can interact with the hands and be returned
    /// </summary>
    [CreateAssetMenu]
    public class PGVRInteractablePackage : ScriptableObject
    {
        public enum ItemPackageType { Unrestricted, OneHanded, TwoHanded }

        public ItemPackageType packageType = ItemPackageType.Unrestricted;
        public PGVRInteractableObject interactableObjectPrefab;           // interactable to be spawned on tracked controller
        public PGVRInteractableObject otherHandInteractableObjectPrefab;  // interactable to be spawned in Other Hand
        //public GameObject previewPrefab;        // used to preview inputObject
        //public GameObject fadedPreviewPrefab;   // used to preview insubstantial inputObject
    }
}
