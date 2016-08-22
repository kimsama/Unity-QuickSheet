///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachineEditor.cs
///
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace UnityQuickSheet
{
    /// <summary>
    /// Custom editor script class for excel file setting.
    /// </summary>
    [CustomEditor(typeof(ExcelMachine))]
    public class ExcelMachineEditor : BaseMachineEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            machine = target as ExcelMachine;
            if (machine != null)
            {
                if (string.IsNullOrEmpty(ExcelSettings.Instance.RuntimePath) == false)
                    machine.RuntimeClassPath = ExcelSettings.Instance.RuntimePath;
                if (string.IsNullOrEmpty(ExcelSettings.Instance.EditorPath) == false)
                    machine.EditorClassPath = ExcelSettings.Instance.EditorPath;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExcelMachine machine = target as ExcelMachine;

            GUILayout.Label("Excel Spreadsheet Settings:", headerStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Label("File:", GUILayout.Width(50));

            string path = string.Empty;
            if (string.IsNullOrEmpty(machine.excelFilePath))
                path = Application.dataPath;
            else
                path = machine.excelFilePath;

            machine.excelFilePath = GUILayout.TextField(path, GUILayout.Width(250));
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                string folder = Path.GetDirectoryName(path);
#if UNITY_EDITOR_WIN
                path = EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
#else // for UNITY_EDITOR_OSX
                path = EditorUtility.OpenFilePanel("Open Excel file", folder, "xls");
#endif
                if (path.Length != 0)
                {
                    machine.SpreadSheetName = Path.GetFileName(path);

                    // the path should be relative not absolute one to make it work on any platform.
                    int index = path.IndexOf("Assets");
                    if (index >= 0)
                    {
                        // set relative path
                        machine.excelFilePath = path.Substring(index);

                        // pass absolute path
                        machine.SheetNames = new ExcelQuery(path).GetSheetNames();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error",
                            @"Wrong folder is selected.
                        Set a folder under the 'Assets' folder! \n
                        The excel file should be anywhere under  the 'Assets' folder", "OK");
                        return;
                    }
                }
            }
            GUILayout.EndHorizontal();

            // Failed to get sheet name so we just return not to make editor on going.
            if (machine.SheetNames.Length == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Error: Failed to retrieve the specified excel file.");
                EditorGUILayout.LabelField("If the excel file is opened, close it then reopen it again.");
                return;
            }

            // spreadsheet name should be read-only
            EditorGUILayout.TextField("Spreadsheet File: ", machine.SpreadSheetName);

            EditorGUILayout.Space();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Worksheet: ", GUILayout.Width(100));
                machine.CurrentSheetIndex = EditorGUILayout.Popup(machine.CurrentSheetIndex, machine.SheetNames);
                if (machine.SheetNames != null)
                    machine.WorkSheetName = machine.SheetNames[machine.CurrentSheetIndex];

                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    // reopen the excel file e.g) new worksheet is added so need to reopen.
                    machine.SheetNames = new ExcelQuery(machine.excelFilePath).GetSheetNames();

                    // one of worksheet was removed, so reset the selected worksheet index
                    // to prevent the index out of range error.
                    if (machine.SheetNames.Length <= machine.CurrentSheetIndex)
                    {
                        machine.CurrentSheetIndex = 0;

                        string message = "Worksheet was changed. Check the 'Worksheet' and 'Update' it again if it is necessary.";
                        EditorUtility.DisplayDialog("Info", message, "OK");
                    }
                }
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();

            if (machine.HasColumnHeader())
            {
                if (GUILayout.Button("Update"))
                    Import();
                if (GUILayout.Button("Reimport"))
                    Import(true);
            }
            else
            {
                if (GUILayout.Button("Import"))
                    Import();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            DrawHeaderSetting(machine);

            EditorGUILayout.Separator();

            GUILayout.Label("Path Settings:", headerStyle);

            machine.TemplatePath = EditorGUILayout.TextField("Template: ", machine.TemplatePath);
            machine.RuntimeClassPath = EditorGUILayout.TextField("Runtime: ", machine.RuntimeClassPath);
            machine.EditorClassPath = EditorGUILayout.TextField("Editor:", machine.EditorClassPath);
            //machine.DataFilePath = EditorGUILayout.TextField("Data:", machine.DataFilePath);

            machine.onlyCreateDataClass = EditorGUILayout.Toggle("Only DataClass", machine.onlyCreateDataClass);

            EditorGUILayout.Separator();

            if (GUILayout.Button("Generate"))
            {
                ScriptPrescription sp = Generate(machine);
                if (sp != null)
                {
                    Debug.Log("Successfully generated!");
                }
                else
                    Debug.LogError("Failed to create a script from excel.");
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(machine);
                //AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Import the specified excel file and prepare to set type of each cell.
        /// </summary>
        protected override void Import(bool reimport = false)
        {
            ExcelMachine machine = target as ExcelMachine;

            string path = machine.excelFilePath;
            string sheet = machine.WorkSheetName;

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "You should specify spreadsheet file first!",
                    "OK"
                );
                return;
            }

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "File at " + path + " does not exist.",
                    "OK"
                );
                return;
            }

            int startRowIndex = 0;
            string error = string.Empty;
            var titles = new ExcelQuery(path, sheet).GetTitle(startRowIndex, ref error);
            if (titles == null || !string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Error", error, "OK");
                return;
            }
            else
            {
                // check the column header is valid
                foreach(string column in titles)
                {
                    if (!IsValidHeader(column))
                    {
                        error = string.Format(@"Invalid column header name {0}. Any c# keyword should not be used for column header. Note it is not case sensitive.", column);
                        EditorUtility.DisplayDialog("Error", error, "OK");
                        return;
                    }
                }
            }

            List<string> titleList = titles.ToList();

            if (machine.HasColumnHeader() && reimport == false)
            {
                var headerDic = machine.ColumnHeaderList.ToDictionary(header => header.name);

                // collect non-changed column headers
                var exist = from t in titleList
                            where headerDic.ContainsKey(t) == true
                            select new ColumnHeader { name = t, type = headerDic[t].type, isArray = headerDic[t].isArray, OrderNO = headerDic[t].OrderNO };

                // collect newly added or changed column headers
                var changed = from t in titleList
                              where headerDic.ContainsKey(t) == false
                              select new ColumnHeader { name = t, type = CellType.Undefined, OrderNO = titleList.IndexOf(t) };

                // merge two list via LINQ
                var merged = exist.Union(changed).OrderBy(x => x.OrderNO);

                machine.ColumnHeaderList.Clear();
                machine.ColumnHeaderList = merged.ToList();
            }
            else
            {
                machine.ColumnHeaderList.Clear();

                if (titles != null)
                {
                    int i = 0;
                    foreach (string s in titles)
                    {
                        machine.ColumnHeaderList.Add(new ColumnHeader { name = s, type = CellType.Undefined, OrderNO = i });
                        i++;
                    }
                }
                else
                {
                    Debug.LogWarning("The WorkSheet [" + sheet + "] may be empty.");
                }
            }

            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Generate AssetPostprocessor editor script file.
        /// </summary>
        protected override void CreateAssetCreationScript(BaseMachine m, ScriptPrescription sp)
        {
            ExcelMachine machine = target as ExcelMachine;

            sp.className = machine.WorkSheetName;
            sp.dataClassName = machine.WorkSheetName + "Data";
            sp.worksheetClassName = machine.WorkSheetName;

            // where the imported excel file is.
            sp.importedFilePath = machine.excelFilePath;

            // path where the .asset file will be created.
            string path = Path.GetDirectoryName(machine.excelFilePath);
            path += "/" + machine.WorkSheetName + ".asset";
            sp.assetFilepath = path;
            sp.assetPostprocessorClass = machine.WorkSheetName + "AssetPostprocessor";
            sp.template = GetTemplate("PostProcessor");

            // write a script to the given folder.
            using (var writer = new StreamWriter(TargetPathForAssetPostProcessorFile(machine.WorkSheetName)))
            {
                writer.Write(new ScriptGenerator(sp).ToString());
                writer.Close();
            }
        }
    }
}