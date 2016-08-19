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
        ExcelSettings setting;

        public void OnEnable()
        {
            setting = target as ExcelSettings;
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            GUIStyle headerStyle = GUIHelper.MakeHeader();
            GUILayout.Label("Excel Settings", headerStyle);

            EditorGUILayout.Separator();

            // paths for runtime and editor folder which will contain generated script files.
            GUILayout.BeginHorizontal();
            GUILayout.Label("Setting FilePath: ", GUILayout.Width(110));
            // prevent to modify by manual
            GUILayout.TextField(ExcelSettings.AssetPath, 120);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Setting FileName: ", GUILayout.Width(110));
            // read-only
            GUILayout.TextField(ExcelSettings.AssetFileName, 120);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            if (setting.CheckPath())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Runtime Path: ", GUILayout.Width(100));
                setting.RuntimePath = GUILayout.TextField(setting.RuntimePath);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Editor Path: ", GUILayout.Width(100));
                setting.EditorPath = GUILayout.TextField(setting.EditorPath);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Toggle(true, "", "CN EntryError", GUILayout.Width(20));
                GUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(12));
                GUILayout.Label("Correct the path of the ExcelSetting.asset file.", GUILayout.Height(20));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(setting);
            }
        }
    }
}