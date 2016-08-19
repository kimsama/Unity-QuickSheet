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

namespace UnityQuickSheet
{
    /// <summary>
    /// A class manages excel setting.
    /// </summary>
    public class ExcelSettings : ScriptableObject
    {
        // A path of default setting file is located.
        public static string AssetPath = "Assets/QuickSheet/ExcelPlugin/Editor/";

        // A filename of setting .asset file.
        public static readonly string AssetFileName = "ExcelSettings.asset";

        /// <summary>
        /// A path where generated ScriptableObject derived class and its data class script files are to be put.
        /// </summary>
        public string RuntimePath = string.Empty;

        /// <summary>
        /// A path where generated editor script files are to be put.
        /// </summary>
        public string EditorPath = string.Empty;

        /// <summary>
        /// A singleton instance.
        /// </summary>
        private static ExcelSettings s_Instance = null;

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
                Debug.LogError(@"No ExcelSetting.asset file is found. Create setting file first. See the menu at 'Assets/Create/QuickSheet/Setting/Excel Setting'.");
            }
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
        /// A property for a singleton instance.
        /// </summary>
        public static ExcelSettings Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    string path = ExcelSettings.AssetPath + ExcelSettings.AssetFileName;
                    s_Instance = AssetDatabase.LoadAssetAtPath(path, typeof(ExcelSettings)) as ExcelSettings;
                    if (s_Instance == null)
                    {
                        string title = string.Format(@"No {0} is found!", AssetFileName);
                        string message = string.Format(@"No {0} is found at '{1}'. \n Press 'Create' button to create a default one.", AssetFileName, AssetPath);
                        bool ok = EditorUtility.DisplayDialog(
                            title,
                            message,
                            "Create",
                            "Cancel"
                        );

                        // create excel setting .asset file if it does not exist under the asset path.
                        if (ok)
                            s_Instance = ExcelSettings.Create();
                    }
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Create .asset file for excel setting.
        /// </summary>
        public static ExcelSettings Create()
        {
            string filePath = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
            s_Instance = (ExcelSettings)AssetDatabase.LoadAssetAtPath(filePath, typeof(ExcelSettings));

            if (s_Instance == null)
            {
                s_Instance = CreateInstance<ExcelSettings>();

                string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(AssetFileName);
                AssetDatabase.CreateAsset(s_Instance, path);

                ExcelSettings.AssetPath = Path.GetDirectoryName(path);
                ExcelSettings.AssetPath += "/";

                // saves file path of the created asset.
                EditorUtility.SetDirty(s_Instance);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog(
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
}
