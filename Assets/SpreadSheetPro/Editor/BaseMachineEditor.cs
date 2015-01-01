///////////////////////////////////////////////////////////////////////////////
///
/// BaseMachineEditor.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;


/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(BaseMachine))]
public class BaseMachineEditor : Editor 
{
    BaseMachine machine;

    protected readonly string NoTemplateString = "No Template File Found";

    void OnEnable()
    {
        machine = target as BaseMachine;
    }

    protected virtual void Import()
    { 
    }

    /// <summary>
    /// Generate script files with the given templates.
    /// Total four files are generated, two for runtime and others for editor.
    /// </summary>
    protected virtual bool Generate()
    {
        ScriptPrescription sp = new ScriptPrescription();

        if (machine == null)
            machine = target as BaseMachine;

        if (machine.onlyCreateDataClass)
        {
            CreateDataClassScript(sp);
        }
        else
        {
            CreateScriptableObjectClassScript(sp);
            CreateScriptableObjectEditorClassScript(sp);
            CreateDataClassScript(sp);
            CreateAssetFileFunc(sp);
        }

        AssetDatabase.Refresh();

        return true;
    }

    /// <summary>
    /// Create a ScriptableObject class and write it down on the specified folder.
    /// </summary>
    protected void CreateScriptableObjectClassScript(ScriptPrescription sp)
    {
        sp.className = machine.WorkSheetName;
        sp.dataClassName = machine.WorkSheetName + "Data";
        sp.template = GetTemplate("ScriptableObjectClass");

        // check the directory path exists
        string fullPath = TargetPathForClassScript(machine.WorkSheetName);
        string folderPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog(
                "Warning",
                "The folder for runtime script files does not exist. Check the path " + folderPath + " exists.",
                "OK"
            );
            return;
        }

        StreamWriter writer = null;
        try
        {
            // write a script to the given folder.		
            writer = new StreamWriter(fullPath);
            writer.Write(new NewScriptGenerator(sp).ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
            }
        }
    }

    /// <summary>
    /// Create a ScriptableObject editor class and write it down on the specified folder.
    /// </summary>
    protected void CreateScriptableObjectEditorClassScript(ScriptPrescription sp)
    {
        sp.className = machine.WorkSheetName + "Editor";
        sp.worksheetClassName = machine.WorkSheetName;
        sp.dataClassName = machine.WorkSheetName + "Data";
        sp.template = GetTemplate("ScriptableObjectEditorClass");

        // check the directory path exists
        string fullPath = TargetPathForEditorScript(machine.WorkSheetName);
        string folderPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog(
                "Warning",
                "The folder for editor script files does not exist. Check the path " + folderPath + " exists.",
                "OK"
                );
            return;
        }

        StreamWriter writer = null;
        try
        {
            // write a script to the given folder.		
            writer = new StreamWriter(fullPath);
            writer.Write(new NewScriptGenerator(sp).ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
            }
        }
    }

    /// <summary>
    /// Create a data class which describes the spreadsheet and write it down on the specified folder.
    /// </summary>
    protected void CreateDataClassScript(ScriptPrescription sp)
    {
        // check the directory path exists
        string fullPath = TargetPathForData(machine.WorkSheetName);
        string folderPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog(
                "Warning",
                "The folder for runtime script files does not exist. Check the path " + folderPath + " exists.",
                "OK"
                );
            return;
        }

        List<MemberFieldData> fieldList = new List<MemberFieldData>();

        //FIXME: replace ValueType to CellType and support Enum type.
        foreach (HeaderColumn header in machine.HeaderColumnList)
        {
            MemberFieldData member = new MemberFieldData();
            member.Name = header.name;
            member.type = header.type;

            fieldList.Add(member);
        }

        sp.className = machine.WorkSheetName + "Data";
        sp.template = GetTemplate("DataClass");

        sp.memberFields = fieldList.ToArray();

        // write a script to the given folder.		
        using (var writer = new StreamWriter(fullPath))
        {
            writer.Write(new NewScriptGenerator(sp).ToString());
            writer.Close();
        }
    }

    /// 
    /// Create utility class which has menu item function to create an asset file.
    /// 
    private void CreateAssetFileFunc(ScriptPrescription sp)
    {
        sp.className = machine.WorkSheetName;
        sp.worksheetClassName = machine.WorkSheetName;
        sp.assetFileCreateFuncName = "Create" + machine.WorkSheetName + "AssetFile";
        sp.template = GetTemplate("AssetFileClass");

        // write a script to the given folder.		
        using (var writer = new StreamWriter(TargetPathForAssetFileCreateFunc(machine.WorkSheetName)))
        {
            writer.Write(new NewScriptGenerator(sp).ToString());
            writer.Close();
        }
    }

    /// <summary>
    /// e.g. "Assets/Script/Data/Runtime/Item.cs"
    /// </summary>
    protected string TargetPathForClassScript(string worksheetName)
    {
        return Path.Combine("Assets/" + machine.RuntimeClassPath, worksheetName + "." + "cs");
    }

    /// <summary>
    /// e.g. "Assets/Script/Data/Editor/ItemEditor.cs"
    /// </summary>
    protected string TargetPathForEditorScript(string worksheetName)
    {
        return Path.Combine("Assets/" + machine.EditorClassPath, worksheetName + "Editor" + "." + "cs");
    }

    /// <summary>
    /// data class script file has 'WorkSheetNameData' for its filename.
    /// e.g. "Assets/Script/Data/Runtime/ItemData.cs"
    /// </summary>
    protected string TargetPathForData(string worksheetName)
    {
        return Path.Combine("Assets/" + machine.RuntimeClassPath, worksheetName + "Data" + "." + "cs");
    }

    /// <summary>
    /// e.g. "Assets/Script/Data/Editor/ItemAssetCreator.cs"
    /// </summary>
    protected string TargetPathForAssetFileCreateFunc(string worksheetName)
    {
        return Path.Combine("Assets/" + machine.EditorClassPath, worksheetName + "AssetCreator" + "." + "cs");
    }

    /// <summary>
    /// Retrieves all ascii text in the given template file.
    /// </summary>
    protected string GetTemplate(string nameWithoutExtension)
    {
        string path = Path.Combine(GetAbsoluteCustomTemplatePath(), nameWithoutExtension + ".txt");
        if (File.Exists(path))
            return File.ReadAllText(path);

        path = Path.Combine(GetAbsoluteBuiltinTemplatePath(), nameWithoutExtension + ".txt");
        if (File.Exists(path))
            return File.ReadAllText(path);

        return NoTemplateString;
    }

    /// <summary>
    /// e.g. "Assets/SpreadSheetPro/Templates"
    /// </summary>
    protected string GetAbsoluteCustomTemplatePath()
    {
        return Path.Combine(Application.dataPath, machine.TemplatePath);
    }

    /// <summary>
    /// e.g. "C:/Program File(x86)/Unity/Editor/Data"
    /// </summary>
    protected string GetAbsoluteBuiltinTemplatePath()
    {
        return Path.Combine(EditorApplication.applicationContentsPath, machine.TemplatePath);
    }

    protected GUIStyle MakeHeader()
    {
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 12;
        headerStyle.fontStyle = FontStyle.Bold;

        return headerStyle;
    }

    protected void DrawHeaderSetting(BaseMachine machine)
    {
        //BaseMachine machine = target as BaseMachine;

        if (machine.HasHeadColumn())
        {
            //EditorGUILayout.LabelField("type settings");
            GUIStyle headerStyle = MakeHeader();
            GUILayout.Label("Type Settings:", headerStyle);

            //curretScroll = EditorGUILayout.BeginScrollView(curretScroll, false, false);
            EditorGUILayout.BeginVertical("box");

            string lastCellName = string.Empty;
            foreach (HeaderColumn header in machine.HeaderColumnList)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(header.name);
                header.type = (CellType)EditorGUILayout.EnumPopup(header.type, GUILayout.MaxWidth(100));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();
        }
    }
}
