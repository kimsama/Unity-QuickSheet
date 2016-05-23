///////////////////////////////////////////////////////////////////////////////
///
/// BaseExelEditor.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityQuickSheet
{
    /// <summary>
    /// Base class of the created .asset ScriptableObject class.
    /// </summary>
    public class BaseExcelEditor<T> : Editor
    {

        // to reflect properties on the Inspector view.
        protected PropertyField[] databaseFields;
        protected PropertyField[] dataFields;

        protected List<PropertyField[]> pInfoList = new List<PropertyField[]>();

        GUIStyle brown;
        bool isInitialized = false;

        public virtual void OnEnable()
        {
        }

        private void InitGUISkin()
        {
            brown = new GUIStyle("box");
            brown.normal.background = Resources.Load("brown", typeof(Texture2D)) as Texture2D;
            brown.border = new RectOffset(4, 4, 4, 4);
            brown.margin = new RectOffset(3, 3, 3, 3);
            brown.padding = new RectOffset(4, 4, 4, 4);
        }

        public override void OnInspectorGUI()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                InitGUISkin();
            }

            if (GUILayout.Button("Update"))
            {
                if (!Load())
                {
                    const string error1 = "\n- Check the path of the 'Sheet Name' and the file is exist at the path.";
                    const string error2 = "\n- Also check the excel file has the sheet which matches with 'Worksheet Name'.";
                    EditorUtility.DisplayDialog(
                        "Error",
                        "Failed to import and update the excel file." + error1 + error2,
                        "OK"
                    );
                }
            }

            if (target == null)
                return;

            //this.DrawDefaultInspector();
            ExposeProperties.Expose(databaseFields);

            foreach (PropertyField[] p in pInfoList)
            {
                GUILayout.BeginVertical(brown);
                ExposeProperties.Expose(p);
                GUILayout.EndVertical();
            }
        }

        /// 
        /// Called when 'Update' button is pressed. It should be reimplemented in the derived class.
        /// 
        public virtual bool Load()
        {
            return false;
        }
    }
}