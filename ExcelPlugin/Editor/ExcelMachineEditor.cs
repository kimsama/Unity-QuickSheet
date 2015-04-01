///////////////////////////////////////////////////////////////////////////////
///
/// ExcelMachineEditor.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Custom editor script class for excel file setting.
/// </summary>
[CustomEditor(typeof(ExcelMachine))]
public class ExcelMachineEditor : BaseMachineEditor
{
    void OnEnable()
    {
        machine = target as ExcelMachine;
    }

    public override void OnInspectorGUI()
    {
        ExcelMachine machine = target as ExcelMachine;

        GUIStyle headerStyle = MakeHeader();
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
            path = EditorUtility.OpenFilePanel("Open Excel file", folder, "excel files;*.xls;*.xlsx");
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

<<<<<<< HEAD
        // Failed to get sheet name so we just return not to make any trouble on the editor.
=======
        // Failed to get sheet name so we just return not to make editor on going.
>>>>>>> QuickSheet
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

        if (GUILayout.Button("Import"))
        {
            Import();
        }

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
    protected override void Import()
    {
        base.Import();

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

        if (machine.HasHeadColumn())
            machine.HeaderColumnList.Clear();

        var titles = new ExcelQuery(path, sheet).GetTitle();
        if (titles != null)
        {
            foreach (string s in titles)
            {
                HeaderColumn header = new HeaderColumn();
                header.name = s;
                machine.HeaderColumnList.Add(header);
            }
        }
        else
        {
            Debug.LogWarning("The WorkSheet [" + sheet + "] may be empty.");
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
