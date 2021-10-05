using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




namespace PGLibrary.PGVR
{
    [CustomEditor(typeof(PGVRHandCollider))]
    public class PGVRHandColliderEditor : Editor
    {
        private PGVRHandCollider handCollider;


        void OnEnable()
        {
            handCollider = target as PGVRHandCollider;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current touches", EditorStyles.boldLabel);

            foreach(KeyValuePair<PGVRTouchable, PGVRHandCollider.TouchData> kvp in handCollider.touchDict)
            {
 
                EditorGUILayout.LabelField(kvp.Key.name + " - contacts: " + kvp.Value.contacts);
            }

 
        }

    }
}