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
    protected BaseMachine machine;

    protected readonly string NoTemplateString = "No Template File Found";

    protected virtual void Import(bool reimport = false)
    {
        Debug.LogWarning("!!! It should be implemented in the derived class !!!");
    }

    /// <summary>
    /// Generate script files with the given templates.
    /// Total four files are generated, two for runtime and others for editor.
    /// </summary>
    protected virtual ScriptPrescription Generate(BaseMachine m)
    {
        if (m == null)
            return null;

        ScriptPrescription sp = new ScriptPrescription();

        if (m.onlyCreateDataClass)
        {
            CreateDataClassScript(m, sp);
        }
        else
        {
            CreateScriptableObjectClassScript(m, sp);
            CreateScriptableObjectEditorClassScript(m, sp);
            CreateDataClassScript(m, sp);
            CreateAssetCreationScript(m, sp);
        }

        AssetDatabase.Refresh();

        return sp;
    }

    /// <summary>
    /// Create a ScriptableObject class and write it down on the specified folder.
    /// </summary>
    protected void CreateScriptableObjectClassScript(BaseMachine machine, ScriptPrescription sp)
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
    protected void CreateScriptableObjectEditorClassScript(BaseMachine machine, ScriptPrescription sp)
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
    protected void CreateDataClassScript(BaseMachine machine, ScriptPrescription sp)
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
            member.IsArrayType = header.isArray;

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

    protected virtual void CreateAssetCreationScript(BaseMachine m, ScriptPrescription sp)
    {
        Debug.LogWarning("!!! It should be implemented in the derived class !!!");
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
    /// AssetPostprocessor class should be under "Editor" folder.
    /// </summary>
    protected string TargetPathForAssetPostProcessorFile(string worksheetName)
    {
        return Path.Combine("Assets/" + machine.EditorClassPath, worksheetName + "AssetPostProcessor" + "." + "cs");
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
    /// e.g. "Assets/QuickSheet/Templates"
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

    protected void DrawHeaderSetting(BaseMachine m)
    {
        if (m.HasHeadColumn())
        {
            //EditorGUILayout.LabelField("type settings");
            GUIStyle headerStyle = GUIHelper.MakeHeader();
            GUILayout.Label("Type Settings:", headerStyle);

            //curretScroll = EditorGUILayout.BeginScrollView(curretScroll, false, false);
            EditorGUILayout.BeginVertical("box");

            //string lastCellName = string.Empty;
            foreach (HeaderColumn header in m.HeaderColumnList)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(header.name, GUILayout.MaxWidth(250));
                header.type = (CellType)EditorGUILayout.EnumPopup(header.type, GUILayout.MaxWidth(150));
                GUILayout.Space(20);
                EditorGUILayout.LabelField("array:", GUILayout.MaxWidth(40));
                header.isArray = EditorGUILayout.Toggle(header.isArray, GUILayout.MaxWidth(50));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();
        }
    }
}
