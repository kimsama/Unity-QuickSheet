///////////////////////////////////////////////////////////////////////////////
///
/// ScriptGenerator.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

using Object = UnityEngine.Object;

namespace UnityQuickSheet
{
    internal class ScriptGenerator
    {
        private const int CommentWrapLength = 35;
        
        private TextWriter m_Writer;
        private string m_Text;
        private ScriptPrescription m_ScriptPrescription;
        private string m_Indentation;
        private int m_IndentLevel = 0;
        
        private int IndentLevel
        {
            get
            {
                return m_IndentLevel;
            }
            set
            {
                m_IndentLevel = value;
                m_Indentation = String.Empty;
                for (int i=0; i<m_IndentLevel; i++)
                    m_Indentation += "  ";
            }
        }
        
        private string ClassName
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.className))
                    return m_ScriptPrescription.className;
                return "Error_Empty_ClassName";
            }
        }

        private string SpreadSheetName
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.spreadsheetName))
                    return m_ScriptPrescription.spreadsheetName;
                return "Error_Empty_SpreadSheetName";
            }
        }

        private string WorkSheetClassName
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.worksheetClassName))
                    return m_ScriptPrescription.worksheetClassName;
                return "Error_Empty_WorkSheet_ClassName";
            }
        }

        private string DataClassName
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.dataClassName))
                    return m_ScriptPrescription.dataClassName;
                return "Error_Empty_DataClassName";
            }
        }

        private string AssetFileCreateFuncName
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.assetFileCreateFuncName))
                    return m_ScriptPrescription.assetFileCreateFuncName;
                return "Error_Empty_AssetFileCreateFunc_Name";
            }
        }

        private string ImportedFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.importedFilePath))
                    return m_ScriptPrescription.importedFilePath;
                return "Error_Empty_FilePath";
            }
        }

        private string AssetFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.assetFilepath))
                    return m_ScriptPrescription.assetFilepath;
                return "Error_Empty_AssetFilePath";
            }
        }

        private string AssetPostprocessorClass
        { 
            get
            {
                if (!string.IsNullOrEmpty(m_ScriptPrescription.assetPostprocessorClass))
                    return m_ScriptPrescription.assetPostprocessorClass;
                return "Error_Empty_AssetPostprocessorClass";
            }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptGenerator (ScriptPrescription scriptPrescription)
        {
            m_ScriptPrescription = scriptPrescription;
        }
        
        /// <summary>
        /// Replace markdown keywords in the template text file which is currently read in.
        /// </summary>
        public override string ToString ()
        {
            m_Text = m_ScriptPrescription.template;
            m_Writer = new StringWriter ();
            m_Writer.NewLine = "\n";
            
            // Make sure all line endings to be Unix (Mac OSX) format.
            m_Text = Regex.Replace (m_Text, @"\r\n?", delegate(Match m) { return "\n"; });
            
            // Class Name
            m_Text = m_Text.Replace ("$ClassName", ClassName);
            m_Text = m_Text.Replace ("$SpreadSheetName", SpreadSheetName);
            m_Text = m_Text.Replace ("$WorkSheetClassName", WorkSheetClassName);
            m_Text = m_Text.Replace ("$DataClassName", DataClassName);
            m_Text = m_Text.Replace ("$AssetFileCreateFuncName", AssetFileCreateFuncName);

            m_Text = m_Text.Replace ("$AssetPostprocessorClass", AssetPostprocessorClass);
            m_Text = m_Text.Replace ("$IMPORT_PATH", ImportedFilePath);
            m_Text = m_Text.Replace ("$ASSET_PATH", AssetFilePath);
            
            // Other replacements
            foreach (KeyValuePair<string, string> kvp in m_ScriptPrescription.mStringReplacements)
                m_Text = m_Text.Replace (kvp.Key, kvp.Value);

            // Do not change tabs to spcaes of the .txt template files.
            Match match = Regex.Match (m_Text, @"(\t*)\$MemberFields");
            if (match.Success)
            {
                // Set indent level to number of tabs before $Functions keyword
                IndentLevel = match.Groups[1].Value.Length;
                if (m_ScriptPrescription.memberFields != null)
                {
                    foreach(var field in m_ScriptPrescription.memberFields)
                    {
                        WriteMemberField(field);
                        WriteProperty(field);
                        WriteBlankLine();
                    }
                    m_Text = m_Text.Replace (match.Value + "\n", m_Writer.ToString ());
                }
            }
            
            // Return the text of the script
            return m_Text;
        }
        
        private void PutCurveBracesOnNewLine ()
        {
            m_Text = Regex.Replace (m_Text, @"(\t*)(.*) {\n((\t*)\n(\t*))?", delegate(Match match)
            {
                return match.Groups[1].Value + match.Groups[2].Value + "\n" + match.Groups[1].Value + "{\n" +
                    (match.Groups[4].Value == match.Groups[5].Value ? match.Groups[4].Value : match.Groups[3].Value);
            });
        }

        ///
        /// Write a member field of a data class.
        ///
        private void WriteMemberField(MemberFieldData field)
        {
            m_Writer.WriteLine (m_Indentation + "[SerializeField]");

            var fieldName = GetFieldNameForField(field);
            string tmp;
            if (field.type == CellType.Enum)
                tmp = field.Name + " " + fieldName + ";";
            else
            {
                if (field.IsArrayType)
                    tmp = field.Type + "[]" + " " + fieldName + " = new " + field.Type + "[0]" +";";
                else
                    tmp = field.Type + " " + fieldName + ";";
            }

            m_Writer.WriteLine (m_Indentation + tmp);
        }
    
        ///
        /// Write a property of a data class.
        ///
        private void WriteProperty(MemberFieldData field)
        {
            string tmp = string.Empty;
            var propertyName = GetPropertyNameForField(field);
            var fieldName = GetFieldNameForField(field);

            if (field.type == CellType.Enum)
                tmp += "public " + field.Name + " " + propertyName + " ";
            else
            {
                if (field.IsArrayType)
                    tmp += "public " + field.Type + "[]" + " " + propertyName + " ";
                else
                    tmp += "public " + field.Type + " " + propertyName + " ";
            }

            tmp += "{ get {return " + fieldName + "; } set { this." + fieldName + " = value;} }";

            m_Writer.WriteLine (m_Indentation + tmp);
        }

        /// <summary>
        /// Override to implement your own field name format.
        /// </summary>
        protected virtual string GetFieldNameForField(MemberFieldData field)
        {
            return field.Name.ToLower();
        }

        /// <summary>
        /// Override to implement your own property name format.
        /// </summary>
        protected virtual string GetPropertyNameForField(MemberFieldData field)
        {
            if (field.type == CellType.Enum)
                return field.Name.ToUpper();

            // To prevent an error can happen when the name of the column header has all lower case characters.
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            return ti.ToTitleCase(field.Name);
        }

        /// <summary>
        /// Write a blank line.
        /// </summary>
        private void WriteBlankLine ()
        {
            m_Writer.WriteLine (m_Indentation);
        }
        
        /// <summary>
        /// Write comment.
        /// </summary>
        /// <param name="comment"></param>
        private void WriteComment (string comment)
        {
            int index = 0;
            while (true)
            {
                if (comment.Length <= index + CommentWrapLength)
                {
                    m_Writer.WriteLine (m_Indentation + "// " + comment.Substring (index));
                    break;
                }
                else
                {
                    int wrapIndex = comment.IndexOf (' ', index + CommentWrapLength);
                    if (wrapIndex < 0)
                    {
                        m_Writer.WriteLine (m_Indentation + "// " + comment.Substring (index));
                        break;
                    }   
                    else
                    {
                        m_Writer.WriteLine (m_Indentation + "// " + comment.Substring (index, wrapIndex-index));
                        index = wrapIndex + 1;
                    }
                }
            }
        }
    }
}

