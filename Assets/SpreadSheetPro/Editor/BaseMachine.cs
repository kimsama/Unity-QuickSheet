using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor
{
    [System.Serializable]
    public class HeaderColumn
    {
        public CellType type;
        public string name;
        public bool isEnable;
        public bool isArray;
        public HeaderColumn nextArrayItem;
    }

    public class BaseMachine : ScriptableObject
    {

        [ExposeProperty]
        public string TemplatePath
        {
            get { return templatePath; }
            set { templatePath = value; }
        }

        [SerializeField]
        private string templatePath = "SpreadSheetPro/Templates";

        [ExposeProperty]
        public string RuntimeClassPath
        {
            get { return scriptFilePath; }
            set { scriptFilePath = value; }
        }

        [SerializeField]
        private string scriptFilePath;

        [ExposeProperty]
        public string EditorClassPath
        {
            get { return editorScriptFilePath; }
            set { editorScriptFilePath = value; }
        }

        [SerializeField]
        private string editorScriptFilePath;

        [ExposeProperty]
        public string SpreadSheetName
        {
            get { return sheetName; }
            set { sheetName = value; }
        }

        [SerializeField]
        private string sheetName;

        [ExposeProperty]
        public string WorkSheetName
        {
            get { return workSheetName; }
            set { workSheetName = value; }
        }

        [SerializeField]
        private string workSheetName;

        [System.NonSerialized]
        public bool onlyCreateDataClass = false;

        public List<HeaderColumn> HeaderColumnList
        {
            get { return headerColumnList; }
        }

        [SerializeField]
        protected List<HeaderColumn> headerColumnList;

        /// <summary>
        /// Return true, if the list is instantiated and has any its item more than one.
        /// </summary>
        /// <returns></returns>
        public bool HasHeadColumn()
        {
            if (headerColumnList != null && headerColumnList.Count > 0)
                return true;

            return false;
        }

        protected readonly string DEFAULT_CLASS_PATH = "Scripts/Runtime";
        protected readonly string DEFAULT_EDITOR_PATH = "Scripts/Editor";

        protected void OnEnable()
        {
            if (headerColumnList == null)
                headerColumnList = new List<HeaderColumn>();
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