using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class PGVRInteractablePackageSpawner : MonoBehaviour
    {

        public PGVRInteractablePackage itemPackagePrefab;
        public bool useGrabToTake = false;
        public bool usePoolManager = false;


        private void OnEnable()
        {
            AddEventHandlers();
        }


        private void OnDisable()
        {
            RemoveEventHandlers();
        }


        private void AddEventHandlers()
        {
            PGVRInteractableObject interactableObject = this.GetComponent<PGVRInteractableObject>();
            if (interactableObject == null)
                Debug.LogWarning("PGVRInteractablePackageSpawner: cannot find PGVRInteractableObject");
            else
            {
                interactableObject.OnInteracted += InteractableObject_OnInteracted;
            }
        }

            
        private void RemoveEventHandlers()
        {
            PGVRInteractableObject interactableObject = this.GetComponent<PGVRInteractableObject>();
            if (interactableObject == null)
                Debug.LogWarning("PGVRInteractablePackageSpawner: cannot find PGVRInteractableObject");
            else
            {
                interactableObject.OnInteracted -= InteractableObject_OnInteracted;
            }
        }


        private void InteractableObject_OnInteracted(object sender, InteractEventArgs e)
        {
            Debug.Log("InteractableObject_OnInteracted");
            SpawnAndGrabPackage(e.InteractableController, e.GrabType);
        }


        private void SpawnAndGrabPackage(PGVRInteractableController interactableController, PGVRGrabType grabType)
        {
            //release current grab if needed
            if (itemPackagePrefab.packageType == PGVRInteractablePackage.ItemPackageType.OneHanded)
            {
                //remove one and two-handed items from this hand and two - handed items from both hands
                interactableController.ReleaseCurrentGrab();
                interactableController.otherController?.ReleaseInteractables(PGVRInteractableType.TwoHanded);
            }
            else  if (itemPackagePrefab.packageType == PGVRInteractablePackage.ItemPackageType.TwoHanded)
            {
                interactableController.ReleaseCurrentGrab();
                interactableController.otherController?.ReleaseCurrentGrab();
            }

            //instantiate interactable and grab with grabbing controller
            PGVRInteractableObject interactableObject;
            /*if (usePoolManager)
                interactableObject = PGPoolManager.Instance.InstantiatePooledGO(itemPackagePrefab.interactableObjectPrefab.gameObject, this.transform.position).GetComponent<PGVRInteractableObject>();
            else*/
                interactableObject = Instantiate(itemPackagePrefab.interactableObjectPrefab);
            interactableController.GrabInteractableObject(interactableObject, grabType);


            //instantiate interactable and grab with other controller
            if (itemPackagePrefab.otherHandInteractableObjectPrefab != null)
            {
                PGVRInteractableObject otherInteractableObject = Instantiate(itemPackagePrefab.otherHandInteractableObjectPrefab);
                interactableController.otherController.GrabInteractableObject(otherInteractableObject, grabType);
            }
        }


    }
}
