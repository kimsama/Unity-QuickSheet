using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Data.Excel;

[CustomEditor(typeof(ExcelMachine))]
public class ExcelMachineEditor : BaseMachineEditor
{

    public override void OnInspectorGUI()
    {
        ExcelMachine machine = target as ExcelMachine;

        GUIStyle headerStyle = MakeHeader();
        GUILayout.Label("Excel Settings:", headerStyle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("File:", GUILayout.Width(50));
        machine.excelFilePath = GUILayout.TextField(machine.excelFilePath, GUILayout.Width(250));
        if (GUILayout.Button("...", GUILayout.Width(20)))
        {
            string path = EditorUtility.OpenFilePanel("Open Excel file", "", "xls");
            if (path.Length != 0)
            {
                machine.SpreadSheetName = Path.GetFileName(path);
                machine.excelFilePath = path;
            }
        }
        GUILayout.EndHorizontal();

        machine.SpreadSheetName = EditorGUILayout.TextField("Spreadsheet: ", machine.SpreadSheetName);
        machine.WorkSheetName = EditorGUILayout.TextField("Worksheet: ", machine.WorkSheetName);

        EditorGUILayout.Separator();

        GUILayout.Label("Path Settings:", headerStyle);

        machine.TemplatePath = EditorGUILayout.TextField("Template: ", machine.TemplatePath);
        machine.RuntimeClassPath = EditorGUILayout.TextField("Runtime Class: ", machine.RuntimeClassPath);
        machine.EditorClassPath = EditorGUILayout.TextField("Editor Class:", machine.EditorClassPath);

        EditorGUILayout.Separator();

        if (GUILayout.Button("Import"))
        {
            Import();
        }

        EditorGUILayout.Separator();

        DrawHeaderSetting(machine);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(machine);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.Separator();

        machine.onlyCreateDataClass = EditorGUILayout.Toggle("Only DataClass", machine.onlyCreateDataClass);

        EditorGUILayout.Separator();

        if (GUILayout.Button("Generate"))
        {
            if (!Generate())
                Debug.LogError("Failed to create a script from excel.");
        }

        if (GUILayout.Button("Test"))
        {
            string path = machine.excelFilePath;
            string sheet = machine.WorkSheetName;

            var title = new ExcelQuery(path, sheet).GetTitle();
            foreach (string s in title)
                Debug.Log("title: " + s);

            var list = new ExcelQuery(path, sheet).Deserialize<FighterData>();
            int i = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void Import()
    {
        base.Import();

        ExcelMachine machine = target as ExcelMachine;

        string path = machine.excelFilePath;
        string sheet = machine.WorkSheetName;

        if (string.IsNullOrEmpty(path))
        {
            //TODO: show error
            return;
        }

        if (!File.Exists(path))
        {
            //TODO: show error
            return;
        }

        if (machine.HasHeadColumn())
            machine.HeaderColumnList.Clear();

        //var excel = ExcelReader.Open(machine.excelFilePath);
        //string[] titles = excel.GetTitle(machine.WorkSheetName);

        var titles = new ExcelQuery(path, sheet).GetTitle();
        foreach(string s in titles)
        {
            HeaderColumn header = new HeaderColumn();
            header.name = s;
            machine.HeaderColumnList.Add(header);
        }

        EditorUtility.SetDirty(machine);
        AssetDatabase.SaveAssets();
    }

}
