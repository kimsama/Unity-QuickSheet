///////////////////////////////////////////////////////////////////////////////
///
/// GoogleMachineEditor.cs
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
/// An editor script class of GoogleMachine class.
/// </summary>
[CustomEditor(typeof(GoogleMachine))]
public class GoogleMachineEditor : BaseMachineEditor
{
    GoogleMachine scriptMachine;
    PropertyField[] databaseFields;

    

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

        scriptMachine = target as GoogleMachine;
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

        Rect rc;
        GUIStyle headerStyle = null;

        headerStyle = MakeHeader();

        GUILayout.Label("GoogleDrive Settings:", headerStyle);
        //rc = GUILayoutUtility.GetLastRect();
        //GUI.skin.box.Draw(rc, GUIContent.none, 0);

        GoogleDataSettings.Instance.Account = EditorGUILayout.TextField("Username", GoogleDataSettings.Instance.Account);
        GoogleDataSettings.Instance.Password = EditorGUILayout.PasswordField("Password", GoogleDataSettings.Instance.Password);

        EditorGUILayout.Separator ();

        GUILayout.Label("Script Path Settings:", headerStyle);
        //rc = GUILayoutUtility.GetLastRect();
        //GUI.skin.box.Draw(new Rect(rc.left, rc.top + rc.height, rc.width, 1f), GUIContent.none, 0);

        ExposeProperties.Expose(databaseFields);
        EditorGUILayout.Separator ();

        EditorGUILayout.Separator();

        if (GUILayout.Button("Import"))
        {
            Import();
        }

        EditorGUILayout.Separator();

        DrawHeaderSetting(scriptMachine);


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
            if (Generate() == null)
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
    protected override void Import()
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

    /// <summary>
    /// Generate custom asset creation editor script file.
    /// </summary>
    protected override ScriptPrescription Generate()
    {
        ScriptPrescription sp = base.Generate();
        if (sp != null)
            CreateAssetFileFunc(sp);

        return sp;
    }

}

