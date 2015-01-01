///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachine.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;

namespace UnityEditor
{
    internal class ExcelMachine : BaseMachine
    {
        public string excelFilePath;

        // need for editor
        public string[] SheetNames = { "" };
        // need for editor
        public int CurrentSheetIndex { get; set; }

        /// <summary>
        /// A menu item which create a 'ExcelMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/Excel Tool")]
        public static void CreateScriptMachineAsset()
        {
            ExcelMachine inst = ScriptableObject.CreateInstance<ExcelMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback("New ExcelMachine.asset");
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }

    }
}