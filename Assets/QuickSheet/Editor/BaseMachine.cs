///////////////////////////////////////////////////////////////////////////////
///
/// BaseMachine.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace UnityQuickSheet
{
    /// <summary>
    /// A class which represents column header on the worksheet.
    /// </summary>
    [System.Serializable]
    public class ColumnHeader
    {
        public CellType type;
        public string name;
        public bool isEnable;
        public bool isArray;
        public ColumnHeader nextArrayItem;

        // used to order columns by ascending. (only need on excel-plugin)
        public int OrderNO { get; set; }
    }

    /// <summary>
    /// A class which stores various settings for a worksheet which is imported.
    /// </summary>
    public class BaseMachine : ScriptableObject
    {
        protected readonly static string ImportSettingFilename = "New Import Setting.asset";

        [SerializeField]
        private string templatePath = "QuickSheet/Templates";
        public string TemplatePath
        {
            get { return templatePath; }
            set { templatePath = value; }
        }

        /// <summary>
        /// path the created ScriptableObject class file will be located.
        /// </summary>
        [SerializeField]
        private string scriptFilePath;
        public string RuntimeClassPath
        {
            get { return scriptFilePath; }
            set { scriptFilePath = value; }
        }

        /// <summary>
        /// path the created editor script files will be located.
        /// </summary>
        [SerializeField]
        private string editorScriptFilePath;
        public string EditorClassPath
        {
            get { return editorScriptFilePath; }
            set { editorScriptFilePath = value; }
        }

        [SerializeField]
        private string sheetName;
        public string SpreadSheetName
        {
            get { return sheetName; }
            set { sheetName = value; }
        }

        [SerializeField]
        private string workSheetName;
        public string WorkSheetName
        {
            get { return workSheetName; }
            set { workSheetName = value; }
        }

        [System.NonSerialized]
        public bool onlyCreateDataClass = false;

        public List<ColumnHeader> ColumnHeaderList
        {
            get { return columnHeaderList; }
            set { columnHeaderList = value; }
        }

        [SerializeField]
        protected List<ColumnHeader> columnHeaderList;

        /// <summary>
        /// Return true, if the list is instantiated and has any its item more than one.
        /// </summary>
        /// <returns></returns>
        public bool HasColumnHeader()
        {
            if (columnHeaderList != null && columnHeaderList.Count > 0)
                return true;

            return false;
        }

        protected readonly string DEFAULT_CLASS_PATH = "Scripts/Runtime";
        protected readonly string DEFAULT_EDITOR_PATH = "Scripts/Editor";

        protected void OnEnable()
        {
            if (columnHeaderList == null)
                columnHeaderList = new List<ColumnHeader>();
        }

        /// <summary>
        /// Initialize with default value whenever the asset file is enabled.
        /// </summary>
        public void ReInitialize()
        {
            if (string.IsNullOrEmpty(RuntimeClassPath))
                RuntimeClassPath = DEFAULT_CLASS_PATH;
            if (string.IsNullOrEmpty(EditorClassPath))
                EditorClassPath = DEFAULT_EDITOR_PATH;

            // reinitialize. it does not need to be serialized.
            onlyCreateDataClass = false;
        }
    }
}