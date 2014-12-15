///////////////////////////////////////////////////////////////////////////////
///
/// ScriptMachine.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor
{
    [System.Serializable]
    internal class HeaderColumn
    {
        public CellType type;
        public string name;
        public bool isEnable;
        public bool isArray;
        public HeaderColumn nextArrayItem;
    }

    /// <summary>
    /// 
    /// </summary>
    internal class ScriptMachine : ScriptableObject
    {
        [SerializeField]
        public static string generatorAssetPath = "Assets/SpreadSheetPro/GDataPlugin/Tool/";
        [SerializeField]
        public static string assetFileName = "ScriptMachine.asset";

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
        private List<HeaderColumn> headerColumnList;

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

        private readonly string DEFAULT_CLASS_PATH = "Scripts/Runtime";
        private readonly string DEFAULT_EDITOR_PATH = "Scripts/Editor";

        /// <summary>
        /// Called when the asset file is selected.
        /// </summary>
        void OnEnable()
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

        /// <summary>
        /// A menu item which create a 'ScriptMachine' asset file.
        /// </summary>
        [MenuItem("Assets/Create/ScriptMachine")]
        public static void CreateScriptMachineAsset()
        {
            ScriptMachine inst = ScriptableObject.CreateInstance<ScriptMachine>();
            string path = CustomAssetUtility.GetUniqueAssetPathNameOrFallback("New ScriptMachine.asset");
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = inst;
        }
    }
}