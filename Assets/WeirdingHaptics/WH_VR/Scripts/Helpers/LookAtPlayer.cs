using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    public class LookAtPlayer : MonoBehaviour
    {


        private void LateUpdate()
        {
            PGVRPlayer player = PGVRPlayer.Instance;
            Vector3 cameraPosition = player.HmdTransform.position;
            Quaternion lookRotation = Quaternion.LookRotation(this.transform.position - cameraPosition);

            this.transform.rotation = lookRotation;
        }
    }
}
