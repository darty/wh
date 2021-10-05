using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PGLibrary.PGVR
{
    [CustomEditor(typeof(UidComponent))]
    public class UidComponentEditor : Editor
    {
        private UidComponent uidComponent;


        void OnEnable()
        {
            uidComponent = target as UidComponent;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate UID"))
            {
                uidComponent.GenerateUid();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}