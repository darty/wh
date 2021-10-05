using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UidComponent : MonoBehaviour
{
    [SerializeField]
    private string uid;


    public string Uid { get { return uid; } }

    public void GenerateUid()
    {
        this.uid = System.Guid.NewGuid().ToString();
    }


}


