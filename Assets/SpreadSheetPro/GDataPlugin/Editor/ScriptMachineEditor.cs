///////////////////////////////////////////////////////////////////////////////
///
/// ScriptMachineEditor.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

// to resolve TlsException error.
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using GDataDB;
using GDataDB.Linq;

using GDataDB.Impl;
using Google.GData.Client;
using Google.GData.Spreadsheets;

/// <summary>
/// An editor script class of ScriptMachine class.
/// </summary>
[CustomEditor(typeof(ScriptMachine))]
public class ScriptMachineEditor : Editor
{
    ScriptMachine scriptMachine;
    PropertyField[] databaseFields;

    private readonly string NoTemplateString = "No Template File Found";

    // to resolve TlsException error
    public static bool Validator (object sender, X509Certificate certificate, 
                                  X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    void OnEnable()
    {
        // to resolve TlsException error
        ServicePointManager.ServerCertificateValidationCallback = Validator;

        scriptMachine = target as ScriptMachine;
        scriptMachine.ReInitialize();

        databaseFields = ExposeProperties.GetProperties(scriptMachine);
    }

    private Vector2 curretScroll = Vector2.zero;

    /// <summary>
    /// Draw custom UI.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (GoogleDataSettings.Instance == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "", "CN EntryError", GUILayout.Width(20));
            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(12));
            GUILayout.Label("Check the GoogleDataSetting.asset file exists or its path is correct.", GUILayout.Height(20));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        GUILayout.Label("GoogleDrive Settings");
        GoogleDataSettings.Instance.Account = EditorGUILayout.TextField("Username", GoogleDataSettings.Instance.Account);
        GoogleDataSettings.Instance.Password = EditorGUILayout.PasswordField("Password", GoogleDataSettings.Instance.Password);

        EditorGUILayout.Separator ();

        GUILayout.Label("Script Path Settings");
        ExposeProperties.Expose(databaseFields);
        EditorGUILayout.Separator ();

        EditorGUILayout.Separator();

        if (GUILayout.Button("Import"))
        {
            Import();
        }

        EditorGUILayout.Separator();

        if (scriptMachine.HasHeadColumn())
        {
            EditorGUILayout.LabelField("type settings");
            //curretScroll = EditorGUILayout.BeginScrollView(curretScroll, false, false);
            EditorGUILayout.BeginVertical("box");
            
            string lastCellName = string.Empty;
            foreach (HeaderColumn header in scriptMachine.HeaderColumnList)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(header.name);
                header.type = (CellType)EditorGUILayout.EnumPopup(header.type, GUILayout.MaxWidth(100));
                GUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();
        }

        // force save changed type.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(GoogleDataSettings.Instance);
            EditorUtility.SetDirty(scriptMachine);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.Separator();

        scriptMachine.onlyCreateDataClass = EditorGUILayout.Toggle ("Only DataClass", scriptMachine.onlyCreateDataClass);

        EditorGUILayout.Separator ();

        if (GUILayout.Button("Generate"))
        {
            if (!Generate())
                Debug.LogError("Failed to create a script from Google.");
        }
    }

    /// <summary>
    /// A delegate called on each of a cell query.
    /// </summary>
    delegate void OnEachCell(CellEntry cell);

    /// <summary>
    /// Connect to google-spreadsheet with the specified account and password 
    /// then query cells and call the given callback.
    /// </summary>
    private void DoCellQuery(OnEachCell onCell)
    {
        // first we need to connect to the google-spreadsheet to get all the first row of the cells
        // which are used for the properties of data class.
        var client = new DatabaseClient(GoogleDataSettings.Instance.Account,
                                        GoogleDataSettings.Instance.Password);

        if (string.IsNullOrEmpty(scriptMachine.SpreadSheetName))
            return;
        if (string.IsNullOrEmpty(scriptMachine.WorkSheetName))
            return;

        var db = client.GetDatabase(scriptMachine.SpreadSheetName);
        if (db == null)
        {
            Debug.LogError("The given spreadsheet does not exist.");
            return;
        }

        // retrieves all cells
        var worksheet = ((Database)db).GetWorksheetEntry(scriptMachine.WorkSheetName);

        // Fetch the cell feed of the worksheet.
        CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
        var cellFeed = client.SpreadsheetService.Query(cellQuery);

        // Iterate through each cell, printing its value.
        foreach (CellEntry cell in cellFeed.Entries)
        {
            if (onCell != null)
                onCell(cell);
        }
    }

    /// <summary>
    /// Connect to the google spreadsheet and retrieves its header columns.
    /// </summary>
    private void Import()
    {
        if (scriptMachine.HasHeadColumn())
            scriptMachine.HeaderColumnList.Clear();

        Regex re = new Regex(@"\d+");

        DoCellQuery( (cell)=>{
            // get numerical value from a cell's address in A1 notation
            // only retrieves first column of the worksheet 
            // which is used for member fields of the created data class.
            Match m = re.Match(cell.Title.Text);
            if (int.Parse(m.Value) > 1)
                return;

            // add cell's displayed value to the list.
            //fieldList.Add(new MemberFieldData(cell.Value.Replace(" ", "")));
            HeaderColumn header = new HeaderColumn();
            header.name = cell.Value;
            scriptMachine.HeaderColumnList.Add(header);
        });

        EditorUtility.SetDirty(scriptMachine);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Generate script files with the given templates.
    /// Total four files are generated, two for runtime and others for editor.
    /// </summary>
    private bool Generate()
    {
        ScriptPrescription sp = new ScriptPrescription ();

        if (scriptMachine.onlyCreateDataClass) 
        {
            CreateDataClassScript (sp);
        } 
        else 
        {
            CreateScriptableObjectClassScript (sp);
            CreateScriptableObjectEditorClassScript (sp);
            CreateDataClassScript (sp);
            CreateAssetFileFunc(sp);
        }

        AssetDatabase.Refresh ();

        return true;
    }

    /// <summary>
    /// Create a ScriptableObject class and write it down on the specified folder.
    /// </summary>
    private void CreateScriptableObjectClassScript(ScriptPrescription sp)
    {
        sp.className = scriptMachine.WorkSheetName;
        sp.dataClassName = scriptMachine.WorkSheetName + "Data";
        sp.template = GetTemplate ("ScriptableObjectClass");
        
        // check the directory path exists
        string fullPath = TargetPathForClassScript (scriptMachine.WorkSheetName);
        string folderPath = Path.GetDirectoryName (fullPath);
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog (
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
        catch(System.Exception e)
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
    private void CreateScriptableObjectEditorClassScript(ScriptPrescription sp)
    {
        sp.className = scriptMachine.WorkSheetName + "Editor";
        sp.worksheetClassName = scriptMachine.WorkSheetName;
        sp.dataClassName = scriptMachine.WorkSheetName + "Data";
        sp.template = GetTemplate ("ScriptableObjectEditorClass");

        // check the directory path exists
        string fullPath = TargetPathForEditorScript (scriptMachine.WorkSheetName);
        string folderPath = Path.GetDirectoryName (fullPath);
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog (
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
        catch(System.Exception e)
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
    private void CreateDataClassScript(ScriptPrescription sp)
    {
        // check the directory path exists
        string fullPath = TargetPathForData(scriptMachine.WorkSheetName);
        string folderPath = Path.GetDirectoryName (fullPath);
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog (
                "Warning",
                "The folder for runtime script files does not exist. Check the path " + folderPath + " exists.",
                "OK"
                );
            return;
        }

        List<MemberFieldData> fieldList = new List<MemberFieldData>();

        //FIXME: replace ValueType to CellType and support Enum type.
        foreach (HeaderColumn header in scriptMachine.HeaderColumnList)
        {
            MemberFieldData member = new MemberFieldData();
            member.Name = header.name;
            member.type = header.type;

            fieldList.Add(member);
        }

        sp.className = scriptMachine.WorkSheetName + "Data";
        sp.template = GetTemplate ("DataClass");
        
        sp.memberFields = fieldList.ToArray ();
        
        // write a script to the given folder.		
        using (var writer = new StreamWriter(fullPath))
        {
            writer.Write(new NewScriptGenerator(sp).ToString());
            writer.Close();
        }
    }

    /// <summary>
    /// Translate type of the member fields directly from google spreadsheet's header column.
    /// NOTE: This needs header column to be formatted with colon.  e.g. "Name : string"
    /// </summary>
    [System.Obsolete("Use CreateDataClassScript instead of CreateDataClassScriptFromSpreadSheet.")]
    private void CreateDataClassScriptFromSpreadSheet(ScriptPrescription sp)
    {
        List<MemberFieldData> fieldList = new List<MemberFieldData>();
        
        Regex re = new Regex(@"\d+");
        DoCellQuery((cell) => {
            // get numerical value from a cell's address in A1 notation
            // only retrieves first column of the worksheet 
            // which is used for member fields of the created data class.
            Match m = re.Match(cell.Title.Text);
            if (int.Parse(m.Value) > 1)
                return;

            // add cell's displayed value to the list.
            fieldList.Add(new MemberFieldData(cell.Value.Replace(" ", "")));
        });
        
        sp.className = scriptMachine.WorkSheetName + "Data";
        sp.template = GetTemplate("DataClass");

        sp.memberFields = fieldList.ToArray();

        // write a script to the given folder.		
        using (var writer = new StreamWriter(TargetPathForData(scriptMachine.WorkSheetName)))
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
        sp.className = scriptMachine.WorkSheetName;
        sp.worksheetClassName = scriptMachine.WorkSheetName;
        sp.assetFileCreateFuncName = "Create" + scriptMachine.WorkSheetName + "AssetFile";
        sp.template = GetTemplate ("AssetFileClass");
        
        // write a script to the given folder.		
        using (var writer = new StreamWriter(TargetPathForAssetFileCreateFunc(scriptMachine.WorkSheetName)))
        {
            writer.Write(new NewScriptGenerator(sp).ToString());
            writer.Close();
        }
    }

    /// <summary>
    /// e.g. "Assets/Script/Data/Runtime/Item.cs"
    /// </summary>
    private string TargetPathForClassScript(string worksheetName)
    {
        return Path.Combine ("Assets/" + scriptMachine.RuntimeClassPath, worksheetName + "." + "cs");
    }

    /// <summary>
    /// e.g. "Assets/Script/Data/Editor/ItemEditor.cs"
    /// </summary>
    private string TargetPathForEditorScript(string worksheetName)
    {
        return Path.Combine ("Assets/" + scriptMachine.EditorClassPath, worksheetName + "Editor" + "." + "cs");
    }

    /// <summary>
    /// data class script file has 'WorkSheetNameData' for its filename.
    /// e.g. "Assets/Script/Data/Runtime/ItemData.cs"
    /// </summary>
    private string TargetPathForData(string worksheetName)
    {
        return Path.Combine ("Assets/" + scriptMachine.RuntimeClassPath, worksheetName + "Data" + "." + "cs");
    }

    /// <summary>
    /// e.g. "Assets/Script/Data/Editor/ItemAssetCreator.cs"
    /// </summary>
    private string TargetPathForAssetFileCreateFunc(string worksheetName)
    {
        return Path.Combine ("Assets/"+ scriptMachine.EditorClassPath, worksheetName + "AssetCreator" + "." + "cs");
    }

    /// <summary>
    /// Retrieves all ascii text in the given template file.
    /// </summary>
    private string GetTemplate (string nameWithoutExtension)
    {
        string path = Path.Combine (GetAbsoluteCustomTemplatePath (), nameWithoutExtension + ".txt");
        if (File.Exists (path))
            return File.ReadAllText (path);
        
        path = Path.Combine (GetAbsoluteBuiltinTemplatePath (), nameWithoutExtension + ".txt");
        if (File.Exists (path))
            return File.ReadAllText (path);
        
        return NoTemplateString;
    }

    /// <summary>
    /// e.g. "Assets/SpreadSheetPro/Templates"
    /// </summary>
    private string GetAbsoluteCustomTemplatePath ()
    {
        return Path.Combine(Application.dataPath, scriptMachine.TemplatePath);
    }

    /// <summary>
    /// e.g. "C:/Program File(x86)/Unity/Editor/Data"
    /// </summary>
    private string GetAbsoluteBuiltinTemplatePath ()
    {
        return Path.Combine(EditorApplication.applicationContentsPath, scriptMachine.TemplatePath);
    }

}

