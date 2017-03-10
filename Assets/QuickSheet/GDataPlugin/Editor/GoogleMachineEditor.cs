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
using System.Linq;

// to resolve TlsException error.
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using GDataDB;
using GDataDB.Linq;

using GDataDB.Impl;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace UnityQuickSheet
{
    /// <summary>
    /// An editor script class of GoogleMachine class.
    /// </summary>
    [CustomEditor(typeof(GoogleMachine))]
    public class GoogleMachineEditor : BaseMachineEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            // resolve TlsException error
            UnsafeSecurityPolicy.Instate();

            machine = target as GoogleMachine;
            if (machine != null)
            {
                machine.ReInitialize();

                // Specify paths with one on the GoogleDataSettings.asset file.
                if (string.IsNullOrEmpty(GoogleDataSettings.Instance.RuntimePath) == false)
                    machine.RuntimeClassPath = GoogleDataSettings.Instance.RuntimePath;
                if (string.IsNullOrEmpty(GoogleDataSettings.Instance.EditorPath) == false)
                    machine.EditorClassPath = GoogleDataSettings.Instance.EditorPath;
            }
        }

        /// <summary>
        /// Draw custom UI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

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

            GUILayout.Label("Google Spreadsheet Settings:", headerStyle);

            EditorGUILayout.Separator();

            GUILayout.Label("Script Path Settings:", headerStyle);
            machine.SpreadSheetName = EditorGUILayout.TextField("SpreadSheet Name: ", machine.SpreadSheetName);
            machine.WorkSheetName = EditorGUILayout.TextField("WorkSheet Name: ", machine.WorkSheetName);

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

            machine.onlyCreateDataClass = EditorGUILayout.Toggle("Only DataClass", machine.onlyCreateDataClass);

            EditorGUILayout.Separator();

            if (GUILayout.Button("Generate"))
            {
                if (string.IsNullOrEmpty(machine.SpreadSheetName) || string.IsNullOrEmpty(machine.WorkSheetName))
                {
                    Debug.LogWarning("No spreadsheet or worksheet is specified.");
                    return;
                }

                if (Generate(this.machine) != null)
                    Debug.Log("Successfully generated!");
                else
                    Debug.LogError("Failed to create a script from Google Spreadsheet.");
            }

            // force save changed type.
            if (GUI.changed)
            {
                EditorUtility.SetDirty(GoogleDataSettings.Instance);
                EditorUtility.SetDirty(machine);
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
            var client = new DatabaseClient("", "");

            if (string.IsNullOrEmpty(machine.SpreadSheetName))
                return;
            if (string.IsNullOrEmpty(machine.WorkSheetName))
                return;

            string error = string.Empty;
            var db = client.GetDatabase(machine.SpreadSheetName, ref error);
            if (db == null)
            {
                string message = string.Empty;
                if (string.IsNullOrEmpty(error))
                    message = @"Unknown error.";
                else
                    message = string.Format(@"{0}", error);

                message += "\n\nOn the other hand, see 'GoogleDataSettings.asset' file and check the oAuth2 setting is correctly done.";
                EditorUtility.DisplayDialog("Error", message, "OK");
                return;
            }

            // retrieves all cells
            var worksheet = ((Database)db).GetWorksheetEntry(machine.WorkSheetName);

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
        protected override void Import(bool reimport = false)
        {
            Regex re = new Regex(@"\d+");

            Dictionary<string, ColumnHeader> headerDic = null;
            if (reimport)
                machine.ColumnHeaderList.Clear();
            else
                headerDic = machine.ColumnHeaderList.ToDictionary(k => k.name);

            List<ColumnHeader> tmpColumnList = new List<ColumnHeader>();

            int order = 0;
            // query the first columns only.
            DoCellQuery((cell) =>
            {

                // get numerical value from a cell's address in A1 notation
                // only retrieves first column of the worksheet 
                // which is used for member fields of the created data class.
                Match m = re.Match(cell.Title.Text);
                if (int.Parse(m.Value) > 1)
                    return;

                // check the column header is valid
                if (!IsValidHeader(cell.Value))
                {
                    string error = string.Format(@"Invalid column header name {0}. Any c# keyword should not be used for column header. Note it is not case sensitive.", cell.Value);
                    EditorUtility.DisplayDialog("Error", error, "OK");
                    return;
                }

                ColumnHeader column = ParseColumnHeader(cell.Value, order++);
                if (headerDic != null && headerDic.ContainsKey(cell.Value))
                {
                    // if the column is already exist, copy its name and type from the exist one.
                    ColumnHeader h = machine.ColumnHeaderList.Find(x => x.name == column.name);
                    if (h != null)
                    {
                        column.type = h.type;
                        column.isArray = h.isArray;
                    }
                }

                tmpColumnList.Add(column);
            });

            // update (all of settings are reset when it reimports)
            machine.ColumnHeaderList = tmpColumnList;

            EditorUtility.SetDirty(machine);
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
            DoCellQuery((cell) =>
            {
                // get numerical value from a cell's address in A1 notation
                // only retrieves first column of the worksheet 
                // which is used for member fields of the created data class.
                Match m = re.Match(cell.Title.Text);
                if (int.Parse(m.Value) > 1)
                    return;

                // add cell's displayed value to the list.
                fieldList.Add(new MemberFieldData(cell.Value.Replace(" ", "")));
            });

            sp.className = machine.WorkSheetName + "Data";
            sp.template = GetTemplate("DataClass");

            sp.memberFields = fieldList.ToArray();

            // write a script to the given folder.		
            using (var writer = new StreamWriter(TargetPathForData(machine.WorkSheetName)))
            {
                writer.Write(new ScriptGenerator(sp).ToString());
                writer.Close();
            }
        }

        /// 
        /// Create utility class which has menu item function to create an asset file.
        /// 
        protected override void CreateAssetCreationScript(BaseMachine m, ScriptPrescription sp)
        {
            sp.className = machine.WorkSheetName;
            sp.spreadsheetName = machine.SpreadSheetName;
            sp.worksheetClassName = machine.WorkSheetName;
            sp.assetFileCreateFuncName = "Create" + machine.WorkSheetName + "AssetFile";
            sp.template = GetTemplate("AssetFileClass");

            // write a script to the given folder.		
            using (var writer = new StreamWriter(TargetPathForAssetFileCreateFunc(machine.WorkSheetName)))
            {
                writer.Write(new ScriptGenerator(sp).ToString());
                writer.Close();
            }
        }

    }
}