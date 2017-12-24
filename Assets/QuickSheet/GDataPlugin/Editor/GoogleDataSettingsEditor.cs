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
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace UnityQuickSheet
{
    /// <summary>
    /// Resolve TlsException error.
    /// </summary>
    public class UnsafeSecurityPolicy
    {
        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            //Debug.Log("Validation successful!");
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
        public void OnEnable()
        {
            // resolve TlsException error
            UnsafeSecurityPolicy.Instate();
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            GUIStyle headerStyle = GUIHelper.MakeHeader();
            GUILayout.Label("GoogleSpreadsheet Settings", headerStyle);

            EditorGUILayout.Separator();

            GUIStyle helpBoxStyle = GUI.skin.GetStyle("HelpBox");
            helpBoxStyle.richText = true;
            const string infoMsg = "Copying <b>'client_id'</b> and <b>'client_secret'</b> from Google Developer Console and pasting that into the textfields without specifying json file is also working, if you don't want to install oauth2 json file on the local disk.";
            GUIHelper.HelpBox(infoMsg, MessageType.Info);

            const int LabelWidth = 90;

            using (new GUILayout.HorizontalScope())
            {
                GoogleDataSettings.useOAuth2JsonFile = GUILayout.Toggle(GoogleDataSettings.useOAuth2JsonFile, " I have OAuth2 JSON file");

                // reset client_id and client_secret and empty its textfields.
                if (GUILayout.Button("Reset", GUILayout.Width(60)))
                {
                    GoogleDataSettings.Instance.OAuth2Data.client_id = string.Empty;
                    GoogleDataSettings.Instance.OAuth2Data.client_secret = string.Empty;
                    GoogleDataSettings.Instance._AccessCode = string.Empty;

                    // retrieves from google developer center.
                    GoogleDataSettings.Instance._RefreshToken = string.Empty;
                    GoogleDataSettings.Instance._AccessToken = string.Empty;
                }
            }
            if (GoogleDataSettings.useOAuth2JsonFile)
            {
                GUILayout.BeginHorizontal(); // Begin json file setting
                GUILayout.Label("JSON File:", GUILayout.Width(LabelWidth));

                string path = "";
                if (string.IsNullOrEmpty(GoogleDataSettings.Instance.JsonFilePath))
                    path = Application.dataPath;
                else
                    path = GoogleDataSettings.Instance.JsonFilePath;

                GoogleDataSettings.Instance.JsonFilePath = GUILayout.TextField(path, GUILayout.Width(250));
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

                        //HACK: reported a json file which has no "installed" property
                        //var oauthData = JObject.Parse(jsonData).SelectToken("installed").ToString();
                        //GoogleDataSettings.Instance.OAuth2Data = JsonConvert.DeserializeObject<GoogleDataSettings.OAuth2JsonData>(oauthData);

                        //HACK: assume the parsed json string contains only one property value: JObject.Parse(jsonData).Count == 1
                        JObject jo = JObject.Parse(jsonData);
                        var propertyValues = jo.PropertyValues();
                        foreach (JToken token in propertyValues)
                        {
                            string val = token.ToString();
                            GoogleDataSettings.Instance.OAuth2Data = JsonConvert.DeserializeObject<GoogleDataSettings.OAuth2JsonData>(val);
                        }

                        GoogleDataSettings.Instance.JsonFilePath = path;

                        // force to save the setting.
                        EditorUtility.SetDirty(GoogleDataSettings.Instance);
                        AssetDatabase.SaveAssets();
                    }
                }
                GUILayout.EndHorizontal(); // End json file setting.
            }

            EditorGUILayout.Separator();

            if (GoogleDataSettings.Instance.OAuth2Data.client_id == null)
                GoogleDataSettings.Instance.OAuth2Data.client_id = string.Empty;
            if (GoogleDataSettings.Instance.OAuth2Data.client_secret == null)
                GoogleDataSettings.Instance.OAuth2Data.client_secret = string.Empty;

            // client_id for OAuth2
            GUILayout.BeginHorizontal();
            GUILayout.Label("Client ID: ", GUILayout.Width(LabelWidth));
            GoogleDataSettings.Instance.OAuth2Data.client_id = GUILayout.TextField(GoogleDataSettings.Instance.OAuth2Data.client_id);
            GUILayout.EndHorizontal();

            // client_secret for OAuth2
            GUILayout.BeginHorizontal();
            GUILayout.Label("Client Secret: ", GUILayout.Width(LabelWidth));
            GoogleDataSettings.Instance.OAuth2Data.client_secret = GUILayout.TextField(GoogleDataSettings.Instance.OAuth2Data.client_secret);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            if (GUILayout.Button("Start Authentication"))
            {
                GDataDB.Impl.GDataDBRequestFactory.InitAuthenticate();
            }

            GoogleDataSettings.Instance._AccessCode = EditorGUILayout.TextField("AccessCode", GoogleDataSettings.Instance._AccessCode);
            if (GUILayout.Button("Finish Authentication"))
            {
                try
                {
                    GDataDB.Impl.GDataDBRequestFactory.FinishAuthenticate();
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error", e.Message, "OK");
                }
            }
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Template Path: ", GUILayout.Width(LabelWidth));
            GoogleDataSettings.Instance.TemplatePath = GUILayout.TextField(GoogleDataSettings.Instance.TemplatePath);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Runtime Path: ", GUILayout.Width(LabelWidth));
            GoogleDataSettings.Instance.RuntimePath = GUILayout.TextField(GoogleDataSettings.Instance.RuntimePath);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Editor Path: ", GUILayout.Width(LabelWidth));
            GoogleDataSettings.Instance.EditorPath = GUILayout.TextField(GoogleDataSettings.Instance.EditorPath);
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(GoogleDataSettings.Instance);
            }
        }
    }
}