///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachine.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UnityQuickSheet
{
    /// <summary>
    /// A class for various setting to read excel file and generated related script files.
    /// </summary>
    internal class ExcelMachine : BaseMachine
    {
        /// <summary>
        /// where the .xls or .xlsx file is. The path should start with "Assets/". 
        /// </summary>
        public string excelFilePath;

        // both are needed for popup editor control.
        public string[] SheetNames = { "" };
        public int CurrentSheetIndex 
        {
            get { return currentSelectedSheet; }
            set { currentSelectedSheet = value;} 
        }

        [SerializeField]
        protected int currentSelectedSheet = 0;

        // excel and google plugin have its own template files, 
        // so we need to set the different path when the asset file is created.
        private readonly string excelTemplatePath = "QuickSheet/ExcelPlugin/Templates";

        /// <summary>
        /// Note: Called when the asset file is selected.
        /// </summary>
        protected new void OnEnable()
        {
            base.OnEnable();

            TemplatePath = excelTemplatePath;
        }

        /// <summary>
        /// A menu item which create a 'ExcelMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/QuickSheet/Tools/Excel")]
        public static void CreateScriptMachineAsset()
        {
            ExcelMachine inst = ScriptableObject.CreateInstance<ExcelMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback(ImportSettingFilename);
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }
    }
}