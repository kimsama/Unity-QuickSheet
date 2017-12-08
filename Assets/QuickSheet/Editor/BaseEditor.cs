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
        protected bool isGUISkinInitialized = false;

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
        /// Create and initialize a new gui style which can be used for representing 
        /// each element of dataArray.
        /// </summary>
        protected void InitGUIStyle()
        {
            box = new GUIStyle("box");
            box.normal.background = Resources.Load(EditorGUIUtility.isProSkin ? "brown" : "lightSkinBox", typeof(Texture2D)) as Texture2D;
            box.border = new RectOffset(4, 4, 4, 4);
            box.margin = new RectOffset(3, 3, 3, 3);
            box.padding = new RectOffset(4, 4, 4, 4);
        }

        /// <summary>
        /// Draw serialized properties on the Inspector view.
        /// </summary>
        /// <param name="useGUIStyle"></param>
        protected void DrawInspector(bool useGUIStyle = true)
        {
            // Draw 'spreadsheet' and 'worksheet' name.
            EditorGUILayout.TextField(spreadsheetProp.name, spreadsheetProp.stringValue);
            EditorGUILayout.TextField(worksheetProp.name, worksheetProp.stringValue);

            // Draw properties of the data class.
            if (useGUIStyle && !isGUISkinInitialized)
            {
                isGUISkinInitialized = true;
                InitGUIStyle();
            }

            if (useGUIStyle)
                GUIHelper.DrawSerializedProperty(serializedData, box);
            else
                GUIHelper.DrawSerializedProperty(serializedData);
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