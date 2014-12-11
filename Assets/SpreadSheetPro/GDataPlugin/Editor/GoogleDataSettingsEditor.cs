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
        GUILayout.Label("GoogleSpreadsheet Settings");
        
        // path and asset file name which contains a google account and password.
        GoogleDataSettings.AssetPath = GUILayout.TextField(GoogleDataSettings.AssetPath, 120);
        GoogleDataSettings.AssetFileName = GUILayout.TextField(GoogleDataSettings.AssetFileName, 120);
        
        // account and passwords setting, this should be specified before you're trying to connect a google spreadsheet.
        setting.Account = GUILayout.TextField(setting.Account, 100);
        setting.Password = GUILayout.PasswordField (setting.Password, "*"[0], 25);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssets();
        }
    }
}
