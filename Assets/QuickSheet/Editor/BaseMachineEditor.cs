///////////////////////////////////////////////////////////////////////////////
///
/// BaseMachineEditor.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace UnityQuickSheet
{
    /// <summary>
    /// A base class for a spreadsheet import setting.
    /// </summary>
    [CustomEditor(typeof(BaseMachine))]
    public class BaseMachineEditor : Editor
    {
        protected BaseMachine machine;

        protected readonly string NoTemplateString = "No Template file is found";

        protected GUIStyle headerStyle = null;

        protected virtual void OnEnable()
        {
            // Nothing to do here.
        }

        public override void OnInspectorGUI()
        {
            if (headerStyle == null)
            {
                headerStyle = GUIHelper.MakeHeader();
            }
        }

        /// <summary>
        /// Do not call this in the derived class.
        /// </summary>
        protected virtual void Import(bool reimport = false)
        {
            Debug.LogWarning("!!! It should be implemented in the derived class !!!");
        }

        /// <summary>
        /// Check the given header column has valid name which should not be any c# keywords.
        /// </summary>
        protected bool IsValidHeader(string s)
        {
            // no case sensitive!
            string comp = s.ToLower();

            string found = Array.Find(Util.Keywords, x => x == comp);
            if (string.IsNullOrEmpty(found))
                return true;

            return false;
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
                writer.Write(new ScriptGenerator(sp).ToString());
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
                writer.Write(new ScriptGenerator(sp).ToString());
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
            foreach (ColumnHeader header in machine.ColumnHeaderList)
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
                writer.Write(new ScriptGenerator(sp).ToString());
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

        /// <summary>
        /// Draw column headers on the Inspector view.
        /// </summary>
        protected void DrawHeaderSetting(BaseMachine m)
        {
            if (m.HasColumnHeader())
            {
                GUILayout.Label("Type Settings:", headerStyle);

                // Title
                const int MEMBER_WIDTH = 100;
                using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUILayout.Label("Member", GUILayout.MinWidth(MEMBER_WIDTH));
                    GUILayout.FlexibleSpace();
                    string[] names = { "Type", "Array" };
                    int[] widths = { 55, 40 };
                    for (int i = 0; i < names.Length; i++)
                    {
                        GUILayout.Label(new GUIContent(names[i]), GUILayout.Width(widths[i]));
                    }
                }

                // Each cells
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    foreach (ColumnHeader header in m.ColumnHeaderList)
                    {
                        GUILayout.BeginHorizontal();

                        // show member field with label, read-only
                        EditorGUILayout.LabelField(header.name, GUILayout.MinWidth(MEMBER_WIDTH));
                        GUILayout.FlexibleSpace();

                        // specify type with enum-popup
                        header.type = (CellType)EditorGUILayout.EnumPopup(header.type, GUILayout.Width(60));
                        GUILayout.Space(20);

                        // array toggle
                        header.isArray = EditorGUILayout.Toggle(header.isArray, GUILayout.Width(20));
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
}