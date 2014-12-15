using UnityEngine;
using System.Collections;

namespace UnityEditor
{
    

    internal class ExcelMachine : BaseMachine
    {
        public string excelFilePath;

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