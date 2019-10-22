///////////////////////////////////////////////////////////////////////////////
///
/// Util.cs
/// 
/// (c)2016 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Linq;
using UnityEditor;

namespace UnityQuickSheet
{
    public static class Util 
    {
        //all c# keywords.
        public static string[] Keywords = new string[] {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", 
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
            "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
            "if", "implicit", "in", "in", "int", "interface", "internal", "is", "lock", "long",
            "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", 
            "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
            "ushort", "using", "virtual", "void", "volatile", "while",
        };

        public static T[] FindAssetsByType<T>() where T : UnityEngine.Object
        {
            string filter = "t:" + typeof(T);
            var guids = AssetDatabase.FindAssets(filter);
            var results = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                results[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }

            return results;
        }
    }
}