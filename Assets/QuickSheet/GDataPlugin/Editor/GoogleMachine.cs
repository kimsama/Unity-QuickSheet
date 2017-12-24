///////////////////////////////////////////////////////////////////////////////
///
/// GoogleMachine.cs
///
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityQuickSheet
{
    /// <summary>
    /// A class for various setting to import google spreadsheet data and generated related script files.
    /// </summary>
    internal class GoogleMachine : BaseMachine
    {
        [SerializeField]
        public static string generatorAssetPath = "Assets/QuickSheet/GDataPlugin/Tool/";
        [SerializeField]
        public static string assetFileName = "GoogleMachine.asset";


        public string AccessCode = "";

        /// <summary>
        /// Note: Called when the asset file is created.
        /// </summary>
        private void Awake() 
        {
            // excel and google plugin have its own template files,
            // so we need to set the different path when the asset file is created.
            TemplatePath = GoogleDataSettings.Instance.TemplatePath;
        }

        /// <summary>
        /// A menu item which create a 'GoogleMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/QuickSheet/Tools/Google")]
        public static void CreateGoogleMachineAsset()
        {
            GoogleMachine inst = ScriptableObject.CreateInstance<GoogleMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(ImportSettingFilename);
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }
    }
}