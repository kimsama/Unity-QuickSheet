///////////////////////////////////////////////////////////////////////////////
///
/// ExcelSettingsEditor.cs
/// 
/// (c)2015 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityQuickSheet
{
    /// <summary>
    /// Editor script class for ExcelSettings scriptable object to hide password of google account.
    /// </summary>
    [CustomEditor(typeof(ExcelSettings))]
    public class ExcelSettingsEditor : Editor
    {
        public void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            GUIStyle headerStyle = GUIHelper.MakeHeader();
            GUILayout.Label("Excel Settings", headerStyle);

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Template Path: ", GUILayout.Width(100));
            ExcelSettings.Instance.TemplatePath = GUILayout.TextField(ExcelSettings.Instance.TemplatePath);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Runtime Path: ", GUILayout.Width(100));
            ExcelSettings.Instance.RuntimePath = GUILayout.TextField(ExcelSettings.Instance.RuntimePath);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Editor Path: ", GUILayout.Width(100));
            ExcelSettings.Instance.EditorPath = GUILayout.TextField(ExcelSettings.Instance.EditorPath);
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ExcelSettings.Instance);
            }
        }
    }
}
