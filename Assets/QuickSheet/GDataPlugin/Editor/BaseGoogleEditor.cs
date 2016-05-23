///////////////////////////////////////////////////////////////////////////////
///
/// BaseGoogleEditor.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

// to resolve TlsException error.
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace UnityQuickSheet
{
    /// 
    /// A BaseEditor class.
    /// 
    public class BaseGoogleEditor<T> : Editor
    {
        // property draw
        protected PropertyField[] databaseFields;
        protected PropertyField[] dataFields;

        protected List<PropertyField[]> pInfoList = new List<PropertyField[]>();

        GUIStyle brown;
        bool isInitialized = false;

        /// 
        /// Actively ignore security concerns to resolve TlsException error.
        /// 
        /// See: http://www.mono-project.com/UsingTrustedRootsRespectfully
        ///
        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public virtual void OnEnable()
        {
            // resolves TlsException error
            ServicePointManager.ServerCertificateValidationCallback = Validator;

            GoogleDataSettings settings = GoogleDataSettings.Instance;
            if (settings != null)
            {
                if (string.IsNullOrEmpty(settings.OAuth2Data.client_id) ||
                    string.IsNullOrEmpty(settings.OAuth2Data.client_secret))
                    Debug.LogWarning("Client_ID and Client_Sceret is empty. Reload .json file.");

                if (string.IsNullOrEmpty(settings._AccessCode))
                    Debug.LogWarning("AccessCode is empty. Redo authenticate again.");
            }
            else
            {
                Debug.LogError("Failed to get google data settings. See the google data setting if it has correct path.");
                return;
            }
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
            if (target == null)
                return;

            if (!isInitialized)
            {
                isInitialized = true;
                InitGUISkin();
            }

            if (GUILayout.Button("Download"))
            {
                if (!Load())
                    Debug.LogError("Failed to Load data from Google.");
            }

            EditorGUILayout.Separator();

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
        /// Should be reimplemented in derived class.
        /// 
        public virtual bool Load()
        {
            return true;
        }

        protected List<int> SetArrayValue(string from)
        {
            List<int> tmp = new List<int>();

            CsvParser parser = new CsvParser(from);

            foreach (string s in parser)
            {
                Debug.Log("parsed value: " + s);
                tmp.Add(int.Parse(s));
            }

            return tmp;
        }

        /*
        static string[] SplitCamelCase(string stringToSplit)
        {
            if (!string.IsNullOrEmpty(stringToSplit))
            {
                List<string> words = new List<string>();

                string temp = string.Empty;
                
                foreach (char ch in stringToSplit)
                {
                    if (ch >= 'a' && ch <= 'z')
                        temp = temp + ch;
                    else
                    {
                        words.Add(temp);
                        temp = string.Empty + ch;
                    }
                }
                words.Add(temp);
                return words.ToArray();
            }
            else
                return null;
        }
        */

        public static string SplitCamelCase(string inputCamelCaseString)
        {
            string sTemp = Regex.Replace(inputCamelCaseString, "([A-Z][a-z])", " $1", RegexOptions.Compiled).Trim();
            return Regex.Replace(sTemp, "([A-Z][A-Z])", " $1", RegexOptions.Compiled).Trim();
        }
    }
}