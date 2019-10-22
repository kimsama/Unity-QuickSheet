///////////////////////////////////////////////////////////////////////////////
///
/// ExcelSettings.cs
/// 
/// (c)2015 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityQuickSheet
{
    /// <summary>
    /// A class manages excel setting.
    /// </summary>
    [CreateAssetMenu(menuName = "QuickSheet/Setting/Excel Setting")]
    public class ExcelSettings : SingletonScriptableObject<ExcelSettings>
    {
        /// <summary>
        /// A default path where .txt template files are.
        /// </summary>
        public string TemplatePath = "QuickSheet/ExcelPlugin/Templates";

        /// <summary>
        /// A path where generated ScriptableObject derived class and its data class script files are to be put.
        /// </summary>
        public string RuntimePath = string.Empty;

        /// <summary>
        /// A path where generated editor script files are to be put.
        /// </summary>
        public string EditorPath = string.Empty;
        
        /// <summary>
        /// record import path for QuickSheetMenu
        /// </summary>
        internal List<string> _waitImportPath = new List<string>();
    }
}
