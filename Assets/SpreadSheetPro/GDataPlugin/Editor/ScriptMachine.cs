///////////////////////////////////////////////////////////////////////////////
///
/// ScriptMachine.cs
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
    internal class ScriptMachine : BaseMachine
    {
        [SerializeField]
        public static string generatorAssetPath = "Assets/SpreadSheetPro/GDataPlugin/Tool/";
        [SerializeField]
        public static string assetFileName = "ScriptMachine.asset";



        /// <summary>
        /// Called when the asset file is selected.
        /// </summary>
        void OnEnable()
        {
            base.OnEnable();
        }



        /// <summary>
        /// A menu item which create a 'ScriptMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/ScriptMachine")]
        public static void CreateScriptMachineAsset()
        {
            ScriptMachine inst = ScriptableObject.CreateInstance<ScriptMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback("New ScriptMachine.asset");
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }
    }
}