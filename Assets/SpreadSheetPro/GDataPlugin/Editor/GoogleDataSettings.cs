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

/// <summary>
/// A class to manage google account setting.
/// </summary>
public class GoogleDataSettings : ScriptableObject 
{
    [SerializeField]
    public static string AssetPath = "Assets/SpreadSheetPro/SpreadSheetPro/GDataPlugin/Editor/";
    [SerializeField]
    public static string AssetFileName = "GoogleDataSettings.asset";

    /// <summary>
    /// A property which specifies or retrieves account.
    /// </summary>
    public string Account
    {
        get { return Instance.account; }
        set
        {
            if (s_Instance != null && Instance.account != value)
                Instance.account = value;
        }
    }

    [SerializeField]
    private string account = "account@gmail.com"; // your google acccount.

    /// <summary>
    /// A property which specifies or retrieves password.
    /// </summary>
    public string Password
    {
        get { return Instance.password; }
        set
        {
            if (s_Instance != null && Instance.password != value)
                Instance.password = value;
        }
    }

    [SerializeField]
    private string password = "";

    /// <summary>
    /// A singleton instance.
    /// </summary>
    private static GoogleDataSettings s_Instance;

    /// <summary>
    /// Create new account setting asset file if there is already one then select it.
    /// </summary>
    [MenuItem("Assets/Create/GoogleDataSetting")]
    public static void CreateGoogleDataSetting()
    {
        GoogleDataSettings.Create();
    }

    /// <summary>
    /// Select currently exist account setting asset file.
    /// </summary>
    [MenuItem("Edit/Project Settings/Google Data Settings")]
    public static void Edit()
    {
        Selection.activeObject = Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
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
                string filePath = AssetPath + AssetFileName;
                s_Instance = (GoogleDataSettings)AssetDatabase.LoadAssetAtPath (filePath, typeof (GoogleDataSettings));
                
                if (s_Instance == null)
                {
                    //HACK: DO NOT create googlesettings.asset at the start of the unity editor.
                    //      If it does, it crashes.
                    EditorUtility.DisplayDialog (
                        "Warning",
                        "No account setting file is found. You need to create GoogleDataSettings.asset file.",
                        "OK"
                    );
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
        string filePath = AssetPath + AssetFileName;
        s_Instance = (GoogleDataSettings)AssetDatabase.LoadAssetAtPath(filePath, typeof(GoogleDataSettings));
                        
        if (s_Instance == null)
        {
            s_Instance = CreateInstance<GoogleDataSettings> ();

            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
            AssetDatabase.CreateAsset(s_Instance, path);

            AssetPath = Path.GetDirectoryName(path);
            AssetPath += "/";

            // saves file path of the created asset.
            EditorUtility.SetDirty(s_Instance);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog (
                "Validate Settings",
                "Default google dasa settings have been created for accessing Google project page. You should validate these before proceeding.",
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
