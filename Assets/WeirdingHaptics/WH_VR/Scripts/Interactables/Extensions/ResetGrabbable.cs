using PGLibrary.PGVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGrabbable : MonoBehaviour
{
    public float resetDelay = 3f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Coroutine resetRoutine;


    private void Awake()
    {
        initialPosition = this.transform.position;
        initialRotation = this.transform.rotation;
    }


    private void OnEnable()
    {
        this.GetComponent<PGVRInteractableObject_Grabbable>().OnGrabStarted += ResetGrabbable_OnGrabStarted;
        this.GetComponent<PGVRInteractableObject_Grabbable>().OnGrabEnded += ResetGrabbable_OnGrabEnded;
    }


    private void OnDisable()
    {
        this.GetComponent<PGVRInteractableObject_Grabbable>().OnGrabStarted -= ResetGrabbable_OnGrabStarted;
        this.GetComponent<PGVRInteractableObject_Grabbable>().OnGrabEnded -= ResetGrabbable_OnGrabEnded;
    }


    private void ResetGrabbable_OnGrabStarted(object sender, GrabStartEventArgs e)
    {
        if (resetRoutine != null)
            StopCoroutine(resetRoutine);
    }


    private void ResetGrabbable_OnGrabEnded(object sender, GrabEndEventArgs e)
    {
        resetRoutine = StartCoroutine(ResetRoutine());
    }


    private IEnumerator ResetRoutine()
    {
        yield return new WaitForSecondsRealtime(resetDelay);

        //reset grabbable
        this.transform.position = initialPosition;
        this.transform.rotation = initialRotation;

        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

}
