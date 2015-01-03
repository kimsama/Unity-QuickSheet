///////////////////////////////////////////////////////////////////////////////
///
/// GoogleDataSettingsEditor.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Editor script class for GoogleDataSettings scriptable object to hide password of google account.
/// </summary>
[CustomEditor(typeof(GoogleDataSettings))]
public class GoogleDataSettingsEditor : Editor 
{
    GoogleDataSettings setting;

    public void OnEnable()
    {
        setting = target as GoogleDataSettings;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        GUILayout.Label("GoogleSpreadsheet Settings");
        
        // path and asset file name which contains a google account and password.
        setting.AssetPath = GUILayout.TextField(setting.AssetPath, 120);
        GoogleDataSettings.AssetFileName = GUILayout.TextField(GoogleDataSettings.AssetFileName, 120);		       
        
        if (setting.CheckPath())
        {
            // account and passwords setting, this should be specified before you're trying to connect a google spreadsheet.
            setting.Account = GUILayout.TextField(setting.Account, 100);
            setting.Password = GUILayout.PasswordField(setting.Password, "*"[0], 25);
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "", "CN EntryError", GUILayout.Width(20));
            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(12));
            GUILayout.Label("Correct the path of the GoogleDataSetting.asset file.", GUILayout.Height(20));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssets();
        }
    }
}
