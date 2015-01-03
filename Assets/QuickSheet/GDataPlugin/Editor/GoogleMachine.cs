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

namespace UnityEditor
{


    /// <summary>
    /// 
    /// </summary>
    internal class GoogleMachine : BaseMachine
    {
        [SerializeField]
        public static string generatorAssetPath = "Assets/SpreadSheetPro/GDataPlugin/Tool/";
        [SerializeField]
        public static string assetFileName = "GoogleMachine.asset";



        /// <summary>
        /// Called when the asset file is selected.
        /// </summary>
        void OnEnable()
        {
            base.OnEnable();
        }



        /// <summary>
        /// A menu item which create a 'GoogleMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/Spreadsheet Tools/Goolgle")]
        public static void CreateGoogleMachineAsset()
        {
            GoogleMachine inst = ScriptableObject.CreateInstance<GoogleMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback("New GoogleMachine.asset");
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }
    }
}