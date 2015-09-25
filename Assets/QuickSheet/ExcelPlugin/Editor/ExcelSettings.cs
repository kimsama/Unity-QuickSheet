///////////////////////////////////////////////////////////////////////////////
///
/// ExcelSettings.cs
/// 
/// (c)2015 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

/// <summary>
/// A class to manage excel setting.
/// </summary>
public class ExcelSettings : ScriptableObject 
{
    public string AssetPath = "Assets/QuickSheet/ExcelPlugin/Editor/";

    [SerializeField]
    public static string AssetFileName = "ExcelSettings.asset";

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

    /// <summary>
    /// A singleton instance.
    /// </summary>
    private static ExcelSettings s_Instance;

    /// <summary>
    /// Create new account setting asset file if there is already one then select it.
    /// </summary>
    [MenuItem("Assets/Create/QuickSheet/Setting/Excel Setting")]
    public static void CreateExcelSetting()
    {
        ExcelSettings.Create();
    }

    /// <summary>
    /// Select currently exist account setting asset file.
    /// </summary>
    [MenuItem("Edit/Project Settings/QuickSheet/Select Excel Setting")]
    public static void Edit()
    {
        Selection.activeObject = Instance;
        if (Selection.activeObject == null)
        {
            Debug.LogError("No ExcelSetting.asset file is found. Create setting file first. See the menu at 'Assets/Create/QuickSheet/Setting/Excel Setting'.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
    }
    
    /// <summary>
    /// Checks ExcelSetting.asset file exist at the specified path(AssetPath+AssetFileName).
    /// </summary>
    public bool CheckPath()
    {
        string file = AssetDatabase.GetAssetPath(Selection.activeObject);
        string assetFile = AssetPath + ExcelSettings.AssetFileName;

        return (file == assetFile) ? true : false;
    }

    /// <summary>
    /// A property for singleton.
    /// </summary>
    public static ExcelSettings Instance
    {
        get
        {
            if (s_Instance == null)
            {
                // A tricky way to assess non-static member in the static method.
                ExcelSettings temp = new ExcelSettings();
                string path = temp.AssetPath + ExcelSettings.AssetFileName;

                s_Instance = AssetDatabase.LoadAssetAtPath (path, typeof (ExcelSettings)) as ExcelSettings;
                if (s_Instance == null)
                {
                    Debug.LogWarning("No exel setting file is at " + path + " You need to create a new one or modify its path.");
                }
            }
            return s_Instance;
        }

    }
    
    /// <summary>
    /// Create account setting asset file if it does not exist.
    /// </summary>
    public static ExcelSettings Create()
    {
        string filePath = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
        s_Instance = (ExcelSettings)AssetDatabase.LoadAssetAtPath(filePath, typeof(ExcelSettings));
                        
        if (s_Instance == null)
        {
            s_Instance = CreateInstance<ExcelSettings> ();

            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
            AssetDatabase.CreateAsset(s_Instance, path);

            s_Instance.AssetPath = Path.GetDirectoryName(path);
            s_Instance.AssetPath += "/";

            // saves file path of the created asset.
            EditorUtility.SetDirty(s_Instance);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog (
                "Validate Settings",
                "Default excel settings file has been created for accessing excel spreadsheet. Set valid runtime editor paths before proceeding.",
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

