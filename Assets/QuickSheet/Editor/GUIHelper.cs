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

        const int defaultVisibleArrayElements = 20;
        static int maxVisibleArrayElements = defaultVisibleArrayElements;

        /// <summary>
        /// Recursively draw properties of the given SerializedProperty data.
        /// </summary>
        public static void DrawSerializedProperty(SerializedProperty prop, GUIStyle guiStyle = null)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    // make Array and Object to be fold
                    prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName, true);
                    if (!prop.isExpanded)
                        break;

                    // increase indentation
                    EditorGUI.indentLevel++;
                    if (!prop.isArray)
                    {
                        // get Serializable Object
                        var child = prop.Copy();
                        var end = prop.GetEndProperty(true);
                        if (child.Next(true))
                        {
                            while (!SerializedProperty.EqualContents(child, end))
                            {
                                DrawSerializedProperty(child);
                                if (!child.Next(false))
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Handles array type with separate way due to SerializedProperty provides
                        // its own method for array type.
                        prop.arraySize = EditorGUILayout.IntField("Length", prop.arraySize);
                        var showCount = Mathf.Min(prop.arraySize, maxVisibleArrayElements);
                        for (int i = 0; i < showCount; i++)
                        {
                            if (guiStyle != null)
                            {
                                using (new GUILayout.VerticalScope(guiStyle))
                                {
                                    DrawSerializedProperty(prop.GetArrayElementAtIndex(i));
                                }
                            }
                            else
                                DrawSerializedProperty(prop.GetArrayElementAtIndex(i));
                        }
                        // Hide elements if it exceeds defined show count.
                        if (prop.arraySize > showCount)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                // Do indentation
                                for (int i = 0; i < EditorGUI.indentLevel; i++)
                                {
                                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                                }
                                if (GUILayout.Button("Show more ..."))
                                {
                                    maxVisibleArrayElements += defaultVisibleArrayElements;
                                }
                            }
                        }
                    }
                    // decrease indentation
                    EditorGUI.indentLevel--;
                    break;
                case SerializedPropertyType.Integer:
                    prop.intValue = EditorGUILayout.IntField(prop.displayName, prop.intValue);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = EditorGUILayout.Toggle(prop.displayName, prop.boolValue);
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = EditorGUILayout.FloatField(prop.displayName, prop.floatValue);
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = EditorGUILayout.TextField(prop.displayName, prop.stringValue);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = EditorGUILayout.ColorField(prop.displayName, prop.colorValue);
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = EditorGUILayout.ObjectField(
                        prop.displayName, prop.objectReferenceValue, typeof(Object), true);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Type", prop.type);
                    EditorGUI.indentLevel--;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = EditorGUILayout.IntField(prop.displayName, prop.intValue);
                    break;
                case SerializedPropertyType.Enum:
                    EditorGUILayout.PropertyField(prop);
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    prop.enumValueIndex = EditorGUILayout.IntField(prop.name, prop.enumValueIndex);
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = EditorGUILayout.Vector2Field(prop.displayName, prop.vector2Value);
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = EditorGUILayout.Vector3Field(prop.displayName, prop.vector3Value);
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = EditorGUILayout.RectField(prop.displayName, prop.rectValue);
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.intValue = EditorGUILayout.IntField(prop.displayName, prop.intValue);
                    break;
                case SerializedPropertyType.Character:
                    EditorGUILayout.PropertyField(prop);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = EditorGUILayout.CurveField(prop.displayName, prop.animationCurveValue);
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = EditorGUILayout.BoundsField(prop.displayName, prop.boundsValue);
                    break;
                case SerializedPropertyType.Gradient:
                    EditorGUILayout.PropertyField(prop);
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = Quaternion.Euler(
                        EditorGUILayout.Vector3Field(prop.displayName, prop.quaternionValue.eulerAngles));
                    break;
            }
        }

    }
}
