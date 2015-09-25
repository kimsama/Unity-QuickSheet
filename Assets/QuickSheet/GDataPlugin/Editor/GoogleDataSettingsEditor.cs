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
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class UnsafeSecurityPolicy
{
    public static bool Validator( object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
    {
        Debug.Log("Validation successful!");
        return true;
    }

    public static void Instate()
    {
        ServicePointManager.ServerCertificateValidationCallback = Validator;
    }
}

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

        UnsafeSecurityPolicy.Instate();
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        GUIStyle headerStyle = GUIHelper.MakeHeader();
        GUILayout.Label("GoogleSpreadsheet Settings", headerStyle);

        EditorGUILayout.Separator();

        // path and asset file name which contains a google account and password.
        GUILayout.BeginHorizontal();
        GUILayout.Label("Setting FilePath: ", GUILayout.Width(110));
        setting.AssetPath = GUILayout.TextField(setting.AssetPath, 120);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Setting FileName: ", GUILayout.Width(110));
        GoogleDataSettings.AssetFileName = GUILayout.TextField(GoogleDataSettings.AssetFileName, 120);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        if (setting.CheckPath())
        {
            const int LabelWidth = 90;

            GUILayout.BeginHorizontal(); // Begin json file setting
            GUILayout.Label("JSON File:", GUILayout.Width(LabelWidth));

            string path = "";
            if (string.IsNullOrEmpty(setting.JsonFilePath))
                path = Application.dataPath;
            else
                path = setting.JsonFilePath;

            setting.JsonFilePath = GUILayout.TextField(path, GUILayout.Width(250));
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                string folder = Path.GetDirectoryName(path);
                path = EditorUtility.OpenFilePanel("Open JSON file", folder, "json");
                if (path.Length != 0)
                {
                    StringBuilder builder = new StringBuilder();
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string s = "";
                        while (s != null)
                        {
                            s = sr.ReadLine();
                            builder.Append(s);
                        }
                    }

                    string jsonData = builder.ToString();

                    var oauthData = JObject.Parse(jsonData).SelectToken("installed").ToString();
                    GoogleDataSettings.Instance.OAuth2Data = JsonConvert.DeserializeObject<GoogleDataSettings.OAuth2JsonData>(oauthData);

                    setting.JsonFilePath = path;

                    // force to save the setting.
                    EditorUtility.SetDirty(setting);
                    AssetDatabase.SaveAssets();
                }
            }
            GUILayout.EndHorizontal(); // End json file setting.

            if (setting.OAuth2Data.client_id == null)
                setting.OAuth2Data.client_id = string.Empty;
            if (setting.OAuth2Data.client_secret == null)
                setting.OAuth2Data.client_secret = string.Empty;

            // client_id for OAuth2
            GUILayout.BeginHorizontal();
            GUILayout.Label("Client ID: ", GUILayout.Width(LabelWidth));
            setting.OAuth2Data.client_id = GUILayout.TextField(setting.OAuth2Data.client_id);
            GUILayout.EndHorizontal();

            // client_secret for OAuth2
            GUILayout.BeginHorizontal();
            GUILayout.Label("Client Secret: ", GUILayout.Width(LabelWidth));
            setting.OAuth2Data.client_secret = GUILayout.TextField(setting.OAuth2Data.client_secret);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            if (GUILayout.Button("Start Authenticate"))
            {
                GDataDB.Impl.GDataDBRequestFactory.InitAuthenticate();
            }

            GoogleDataSettings.Instance._AccessCode = EditorGUILayout.TextField("AccessCode", GoogleDataSettings.Instance._AccessCode);
            if (GUILayout.Button("Finish Authenticate"))
            {
                GDataDB.Impl.GDataDBRequestFactory.FinishAuthenticate();
            }
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Runtime Path: ", GUILayout.Width(LabelWidth));
            setting.RuntimePath = GUILayout.TextField(setting.RuntimePath);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Editor Path: ", GUILayout.Width(LabelWidth));
            setting.EditorPath = GUILayout.TextField(setting.EditorPath);
            GUILayout.EndHorizontal();
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
