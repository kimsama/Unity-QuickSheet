///////////////////////////////////////////////////////////////////////////////
///
/// GoogleDataSettings.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// A class to manage google account setting.
/// </summary>
public class GoogleDataSettings : ScriptableObject 
{
    public string AssetPath = "Assets/QuickSheet/GDataPlugin/Editor/";

    [SerializeField]
    public static string AssetFileName = "GoogleDataSettings.asset";

    public string JsonFilePath
    {
        get { return jsonFilePath; }
        set
        {
            if (string.IsNullOrEmpty(value) == false)
                jsonFilePath = value;
        }
    }
    private string jsonFilePath = string.Empty;

    /// <summary>
    /// A property which specifies or retrieves generated runtime class path.
    /// </summary>
    public string RuntimePath
    {
        get { return runTimePath; }
        set
        {
            if (runTimePath != value)
                runTimePath = value;
        }
    }

    [SerializeField]
    private string runTimePath = string.Empty;

    /// <summary>
    /// A property which specifies or retrieves generated editor class path.
    /// </summary>
    public string EditorPath
    {
        get { return editorPath; }
        set
        {
            if (editorPath != value)
                editorPath = value;
        }
    }

    [SerializeField]
    private string editorPath = string.Empty;

    [System.Serializable]
    public struct OAuth2JsonData
    {
        public string client_id;
        public string auth_uri;
        public string token_uri;
        public string auth_provider_x509_cert_url;
        public string client_secret;
        public List<string> redirect_uris;
    };

    public OAuth2JsonData OAuth2Data;

    // enter Access Code after getting it from auth url
    [SerializeField]
    public string _AccessCode = "Paste AcecessCode here!";

    // enter Auth 2.0 Refresh Token and AccessToken after succesfully authorizing with Access Code
    [SerializeField]
    public string _RefreshToken = "";
    [SerializeField]
    public string _AccessToken = "";

    /// <summary>
    /// A singleton instance.
    /// </summary>
    private static GoogleDataSettings s_Instance;

    /// <summary>
    /// Create new account setting asset file if there is already one then select it.
    /// </summary>
    [MenuItem("Assets/Create/QuickSheet/Setting/GoogleData Setting")]
    public static void CreateGoogleDataSetting()
    {
        GoogleDataSettings.Create();
    }

    /// <summary>
    /// Select currently exist account setting asset file.
    /// </summary>
    [MenuItem("Edit/Project Settings/QuickSheet/Select Google Data Setting")]
    public static void Edit()
    {
        Selection.activeObject = Instance;
        if (Selection.activeObject == null)
        {
            Debug.LogError("No GoogleDataSettings.asset file is found. Create setting file first.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        //if (OAuth2Data.client_id == null)
        //    OAuth2Data = new OAuth2JsonData();
    }
    
    /// <summary>
    /// Checks GoogleDataSetting.asset file exist at the specified path(AssetPath+AssetFileName).
    /// </summary>
    public bool CheckPath()
    {
        string file = AssetDatabase.GetAssetPath(Selection.activeObject);
        string assetFile = AssetPath + GoogleDataSettings.AssetFileName;

        return (file == assetFile) ? true : false;
    }

    /// <summary>
    /// A property for singleton.
    /// </summary>
    public static GoogleDataSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                // A tricky way to assess non-static member in the static method.
                GoogleDataSettings temp = new GoogleDataSettings();
                string path = temp.AssetPath + GoogleDataSettings.AssetFileName;

                s_Instance = (GoogleDataSettings)AssetDatabase.LoadAssetAtPath (path, typeof (GoogleDataSettings));
                if (s_Instance == null)
                {
                    Debug.LogWarning("No account setting file is at " + path + " You need to create a new one or modify its path.");
                }
            }
            return s_Instance;
        }

    }
    
    /// <summary>
    /// Create account setting asset file if it does not exist.
    /// </summary>
    public static GoogleDataSettings Create()
    {
        string filePath = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
        s_Instance = (GoogleDataSettings)AssetDatabase.LoadAssetAtPath(filePath, typeof(GoogleDataSettings));
                        
        if (s_Instance == null)
        {
            s_Instance = CreateInstance<GoogleDataSettings> ();

            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
            AssetDatabase.CreateAsset(s_Instance, path);

            s_Instance.AssetPath = Path.GetDirectoryName(path);
            s_Instance.AssetPath += "/";

            // saves file path of the created asset.
            EditorUtility.SetDirty(s_Instance);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog (
                "Validate Settings",
                "Default google data settings file has been created for accessing Google project page. You should validate these before proceeding.",
                "OK"
            );
        }
        else
        {
            Debug.LogWarning("Already exist at " + filePath);
        }

        Selection.activeObject = s_Instance;
        
        return s_Instance;
    }
}
