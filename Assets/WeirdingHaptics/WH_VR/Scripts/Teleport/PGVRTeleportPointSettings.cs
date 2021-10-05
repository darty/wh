using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    [CreateAssetMenu(fileName = "New PGVRTeleportPointSettings", menuName = "PGVRTeleportPointSettings")]
    public class PGVRTeleportPointSettings : ScriptableObject
    {
        [Header("TeleportPoint")]
        [SerializeField]
        private Material pointVisibleMaterial;
        [SerializeField]
        private Material pointLockedMaterial;
        [SerializeField]
        private Material pointHighlightedMaterial;

        [Header("TeleportArea")]
        [SerializeField]
        private Material areaVisibleMaterial;
        [SerializeField]
        private Material areaLockedMaterial;
        [SerializeField]
        private Material areaHighlightedMaterial;

        //point
        public Material PointVisibleMaterial { get => pointVisibleMaterial; }
        public Material PointLockedMaterial { get => pointLockedMaterial; }
        public Material PointHighlightedMaterial { get => pointHighlightedMaterial; }

        //area
        public Material AreaVisibleMaterial { get => areaVisibleMaterial; }
        public Material AreaLockedMaterial { get => areaLockedMaterial; }
        public Material AreaHighlightedMaterial { get => areaHighlightedMaterial; }
    }
}
