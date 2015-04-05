///////////////////////////////////////////////////////////////////////////////
///
/// NewScriptGenerator.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

using Object = UnityEngine.Object;

namespace UnityEditor
{
    internal class NewScriptGenerator
    {
        private const int kCommentWrapLength = 35;
        
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
                    m_Indentation += "	";
            }
        }
        
        private string ClassName
        {
            get
            {
                if (m_ScriptPrescription.className != string.Empty)
                    return m_ScriptPrescription.className;
                return "Example";
            }
        }

        private string WorkSheetClassName
        {
            get
            {
                if (m_ScriptPrescription.worksheetClassName != string.Empty)
                    return m_ScriptPrescription.worksheetClassName;
                return "Empty_WorkSheetClass_Name";
            }
        }

        private string DataClassName
        {
            get
            {
                if (m_ScriptPrescription.dataClassName != string.Empty)
                    return m_ScriptPrescription.dataClassName;
                return "Empty_DataClass_Name";
            }
        }

        private string AssetFileCreateFuncName
        {
            get
            {
                if (m_ScriptPrescription.assetFileCreateFuncName != string.Empty)
                    return m_ScriptPrescription.assetFileCreateFuncName;
                return "Empty_AssetFileCreateFunc_Name";
            }
        }

        private string ImportedFilePath
        {
            get
            {
                if (m_ScriptPrescription.importedFilePath != string.Empty)
                    return m_ScriptPrescription.importedFilePath;
                return "Empty_FilePath";
            }
        }

        private string AssetFilePath
        {
            get
            {
                if (m_ScriptPrescription.assetFilepath != string.Empty)
                    return m_ScriptPrescription.assetFilepath;
                return "Empty_AssetFilePath";
            }
        }

        private string AssetPostprocessorClass
        { 
            get
            {
                if (m_ScriptPrescription.assetPostprocessorClass != String.Empty)
                    return m_ScriptPrescription.assetPostprocessorClass;
                return "Empty_AssetPostprocessorClass";
            }
        }
        
        public NewScriptGenerator (ScriptPrescription scriptPrescription)
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
            
            // Make sure all line endings are Unix (Mac OS X) format
            m_Text = Regex.Replace (m_Text, @"\r\n?", delegate(Match m) { return "\n"; });
            
            // Class Name
            m_Text = m_Text.Replace ("$ClassName", ClassName);

            m_Text = m_Text.Replace ("$WorkSheetClassName", WorkSheetClassName);
            m_Text = m_Text.Replace ("$DataClassName", DataClassName);
            m_Text = m_Text.Replace ("$AssetFileCreateFuncName", AssetFileCreateFuncName);

            m_Text = m_Text.Replace ("$AssetPostprocessorClass", AssetPostprocessorClass);
            m_Text = m_Text.Replace ("$IMPORT_PATH", ImportedFilePath);
            m_Text = m_Text.Replace ("$ASSET_PATH", AssetFilePath);
            
            // Other replacements
            foreach (KeyValuePair<string, string> kvp in m_ScriptPrescription.m_StringReplacements)
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
                        WriteBlankLine();
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

            string tmp;
            if (field.type == CellType.Enum)
                tmp = field.Name + " " + field.Name.ToLower() + ";";
            else
            {
                if (field.IsArrayType)
                    tmp = field.Type + "[]" + " " + field.Name.ToLower() + " = new " + field.Type + "[0]" +";";
                else
                    tmp = field.Type + " " + field.Name.ToLower() + ";";
            }

            m_Writer.WriteLine (m_Indentation + tmp);
        }
    
        ///
        /// Write a property of a data class.
        ///
        private void WriteProperty(MemberFieldData field)
        {
            TextInfo ti = new CultureInfo("en-US",false).TextInfo;

            m_Writer.WriteLine (m_Indentation + "[ExposeProperty]");

            string tmp = string.Empty;

            if (field.type == CellType.Enum)
                tmp += "public " + field.Name + " " + field.Name.ToUpper() + " ";
            else
            {
                if (field.IsArrayType)
                    tmp += "public " + field.Type + "[]" + " " + ti.ToTitleCase(field.Name) + " ";
                else
                    tmp += "public " + field.Type + " " + ti.ToTitleCase(field.Name) + " ";
            }

            tmp += "{ get {return " + field.Name.ToLower() + "; } set { " + field.Name.ToLower() + " = value;} }";

            m_Writer.WriteLine (m_Indentation + tmp);
        }

        private void WriteBlankLine ()
        {
            m_Writer.WriteLine (m_Indentation);
        }
        
        private void WriteComment (string comment)
        {
            int index = 0;
            while (true)
            {
                if (comment.Length <= index + kCommentWrapLength)
                {
                    m_Writer.WriteLine (m_Indentation + "// " + comment.Substring (index));
                    break;
                }
                else
                {
                    int wrapIndex = comment.IndexOf (' ', index + kCommentWrapLength);
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

