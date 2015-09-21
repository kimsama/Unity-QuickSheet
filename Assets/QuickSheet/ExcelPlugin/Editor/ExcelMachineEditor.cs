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

/// <summary>
/// Custom editor script class for excel file setting.
/// </summary>
[CustomEditor(typeof(ExcelMachine))]
public class ExcelMachineEditor : BaseMachineEditor
{
    void OnEnable()
    {
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
        ExcelMachine machine = target as ExcelMachine;

        GUIStyle headerStyle = GUIHelper.MakeHeader();
        GUILayout.Label("Excel Settings:", headerStyle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("File:", GUILayout.Width(50));

        string path = "";
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
#else
            path = EditorUtility.OpenFilePanel("Open Excel file", folder, "xls");
#endif
            if (path.Length != 0)
            {
                machine.SpreadSheetName = Path.GetFileName(path);

                // the path should be relative not absolute one.
                int index = path.IndexOf("Assets");
                machine.excelFilePath = path.Substring(index);

                machine.SheetNames = new ExcelQuery(path).GetSheetNames();
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

        machine.SpreadSheetName = EditorGUILayout.TextField("Spreadsheet File: ", machine.SpreadSheetName);
        machine.CurrentSheetIndex = EditorGUILayout.Popup(machine.CurrentSheetIndex, machine.SheetNames);
        if (machine.SheetNames != null)
            machine.WorkSheetName = machine.SheetNames[machine.CurrentSheetIndex];

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();

        if (machine.HasHeadColumn())
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
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Import the specified excel file and prepare to set type of each cell.
    /// </summary>
    protected override void Import(bool reimport = false)
    {
        base.Import(reimport);

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

        var titles = new ExcelQuery(path, sheet).GetTitle();
        List<string> titleList = titles.ToList();

        if (machine.HasHeadColumn() && reimport == false)
        { 
            var headerDic = machine.HeaderColumnList.ToDictionary(header => header.name);

            // collect non changed header columns
            var exist = from t in titleList
                        where headerDic.ContainsKey(t) == true
                        select new HeaderColumn { name = t, type = headerDic[t].type, OrderNO = headerDic[t].OrderNO };

            // collect newly added or changed header columns
            var changed = from t in titleList
                          where headerDic.ContainsKey(t) == false
                          select new HeaderColumn { name = t, type = CellType.Undefined, OrderNO = titleList.IndexOf(t) };

            // merge two
            var merged = exist.Union(changed).OrderBy(x => x.OrderNO);

            machine.HeaderColumnList.Clear();
            machine.HeaderColumnList = merged.ToList();
        }
        else
        {
            machine.HeaderColumnList.Clear();

            if (titles != null)
            {
                int i = 0;
                foreach (string s in titles)
                {
                    machine.HeaderColumnList.Add(new HeaderColumn { name = s, type = CellType.Undefined, OrderNO = i});
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
            writer.Write(new NewScriptGenerator(sp).ToString());
            writer.Close();
        }
    }
}
