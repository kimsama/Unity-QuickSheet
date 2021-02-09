///////////////////////////////////////////////////////////////////////////////
///
/// BaseEditor.cs
/// 
/// (c)2016 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace UnityQuickSheet
{
    /// <summary>
    /// Base class which draws properties of the created scriptableobject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseEditor<T> : Editor where T : ScriptableObject
    {
        protected SerializedObject targetObject;
        protected SerializedProperty spreadsheetProp;
        protected SerializedProperty worksheetProp;
        protected SerializedProperty serializedData;

        protected GUIStyle box;

        /// <summary>
        /// Create SerialliedObject and initialize related SerializedProperty objects 
        /// which are needed to draw data on the Inspector view.
        /// </summary>
        public virtual void OnEnable()
        {
            T t = (T)target;
            targetObject = new SerializedObject(t);

            spreadsheetProp = targetObject.FindProperty("SheetName");
            if (spreadsheetProp == null)
                Debug.LogError("Failed to find 'SheetName' property.");

            worksheetProp = targetObject.FindProperty("WorksheetName");
            if (worksheetProp == null)
                Debug.LogError("Failed to find 'WorksheetName' property.");

            serializedData = targetObject.FindProperty("dataArray");
            if (serializedData == null)
                Debug.LogError("Failed to find 'dataArray' member field.");
        }

        /// <summary>
        /// Draw serialized properties on the Inspector view.
        /// </summary>
        /// <param name="useGUIStyle"></param>
        protected void DrawInspector(bool useGUIStyle = true)
        {
            // Draw 'spreadsheet' and 'worksheet' name.
            EditorGUILayout.PropertyField(spreadsheetProp);
            EditorGUILayout.PropertyField(worksheetProp);

            EditorGUILayout.PropertyField(serializedData, true);
        }

        /// <summary>
        /// Called when 'Update'(or 'Download' for google data) button is pressed. 
        /// It should be reimplemented in the derived class.
        /// </summary>
        public virtual bool Load()
        {
            throw new NotImplementedException();
        }
    }
}