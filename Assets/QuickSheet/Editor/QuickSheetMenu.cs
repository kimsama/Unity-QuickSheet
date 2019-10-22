using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityQuickSheet
{
    public static class QuickSheetMenu
    {
        /// <summary>
        /// Select currently exist account setting asset file.
        /// </summary>
        [MenuItem("QuickSheet/Excel Setting")]
        public static void SelectExcelSetting()
        {
            Selection.activeObject = ExcelSettings.Instance;
            if (Selection.activeObject == null)
            {
                Debug.LogError(@"No ExcelSetting.asset file is found. Create setting file first. See the menu at 'Create/QuickSheet/Setting/Excel Setting'.");
            }
        }
        
        /// <summary>
        /// Select currently exist account setting asset file.
        /// </summary>
        [MenuItem("QuickSheet/Google Data Setting")]
        public static void SelectGoogleDataSetting()
        {
            Selection.activeObject = GoogleDataSettings.Instance;
            if (Selection.activeObject == null)
            {
                Debug.LogError("No GoogleDataSettings.asset file is found. Create setting file first.");
            }
        }

        /// <summary>
        /// Setup selected excel files
        /// 1.Create Machine Asset
        /// 2.Reimport Machine
        /// 3.Generate Class
        /// 4.Reimport Excel file
        /// </summary>
        [MenuItem("QuickSheet/Setup Select Excels")]
        public static void SetupSelectExcels()
        {
            if (Selection.assetGUIDs.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Select excel files to setup!", "OK");
                return;
            }
            
            var selectObjs = new List<Object>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsExcel(path)) continue;
                
                ExcelSettings.Instance._waitImportPath.Add(path);
                
                var excelQuery = new ExcelQuery(path);
                var sheets = excelQuery.GetSheetNames();
                for (int i = 0; i < sheets.Length; i++)
                {
                    var machine = ExcelMachine.CreateScriptMachineAsset();
                    machine.excelFilePath = path;
                    machine.SheetNames = sheets;
                    machine.WorkSheetName = sheets[i];
                    machine.CurrentSheetIndex = i;
                    machine.SpreadSheetName = Path.GetFileName(path);
                    
                    ReimportMachine(machine, true);
                    BaseMachineEditor.CreateGenerateDirectory(machine);
                    GenerateMachine(machine, false);
                    RenameMachineAsset(machine);
                    
                    selectObjs.Add(machine);
                    Debug.LogFormat("Setup finished! file:{0}, Sheet:{1}", machine.SpreadSheetName, sheets[i]);
                }
            }

            Selection.objects = selectObjs.ToArray();
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// After generate script reload, reimport excel file to generate data asset and update it
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() 
        {
            foreach (var path in ExcelSettings.Instance._waitImportPath)
            {
                AssetDatabase.ImportAsset(path);
            }
            ExcelSettings.Instance._waitImportPath.Clear();
        }

        /// <summary>
        /// Refresh all excel files in project
        /// refresh when change excel header column
        /// </summary>
        [MenuItem("QuickSheet/Refresh All")]
        public static void RefreshAll()
        {
            foreach (var machine in Util.FindAssetsByType<BaseMachine>())
            { 
                ReimportMachine(machine, true);
                GenerateMachine(machine, true);
            }
            
            var files = Directory.GetFiles(Application.dataPath, "*.xls?", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!IsExcel(file) || Path.GetFileName(file).StartsWith("~$")) continue;
                var relativePath = "Assets" + file.Replace(Application.dataPath, "");
                Debug.Log(relativePath);
                AssetDatabase.ImportAsset(relativePath);
            }
            AssetDatabase.Refresh();
        }

        private static void GenerateMachine(BaseMachine machine, bool onlyDataClass)
        {
            var editor = Editor.CreateEditor(machine) as BaseMachineEditor;
            if (editor == null) return;
            machine.onlyCreateDataClass = onlyDataClass;
            editor.Generate(machine);
        }

        private static void ReimportMachine(BaseMachine machine, bool reimport)
        {
            var editor = Editor.CreateEditor(machine) as BaseMachineEditor;
            if (editor == null) return;
            editor.Import(reimport);
        }

        private static void RenameMachineAsset(ExcelMachine machine)
        {
            var name = new StringBuilder();
            name.Append(machine.SpreadSheetName.Split('.')[0])
                .Append("-")
                .Append(machine.WorkSheetName)
                .Append("-")
                .Append("Importer");
            var err = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(machine), name.ToString());
            if (!string.IsNullOrEmpty(err)) Debug.LogWarning("Rename failed " + err);
        }
        
        private static bool IsExcel(string path)
        {
            var ext = Path.GetExtension(path);
            return ext == ".xls" || ext == ".xlsx";
        }
    }
}