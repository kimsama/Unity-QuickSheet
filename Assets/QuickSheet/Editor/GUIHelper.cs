///////////////////////////////////////////////////////////////////////////////
///
/// GUIHelper.cs
/// 
/// (c)2015 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

namespace UnityQuickSheet
{
    public static class GUIHelper
    {
        public static GUIStyle MakeHeader()
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 12;
            headerStyle.fontStyle = FontStyle.Bold;

            return headerStyle;
        }

        /// <summary>
        /// A wrapper put help message on the Inspector.
        /// </summary>
        public static void HelpBox(string message, MessageType msgType)
        {
            EditorGUILayout.HelpBox(message, msgType);                
        }

    }
}
