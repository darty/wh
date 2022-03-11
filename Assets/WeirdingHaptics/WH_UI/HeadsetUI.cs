using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetUI : MonoBehaviour
{
    public GameObject pivotCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = pivotCamera.transform.position + pivotCamera.transform.forward*0.3f;
        this.transform.LookAt(pivotCamera.transform.position);
    }
}
