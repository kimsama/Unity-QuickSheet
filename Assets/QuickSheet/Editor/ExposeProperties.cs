using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public static class ExposeProperties
{
    public static void Expose(PropertyField[] properties)
    {

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        EditorGUILayout.BeginVertical(emptyOptions);

        foreach (PropertyField field in properties)
        {

            EditorGUILayout.BeginHorizontal(emptyOptions);

            switch (field.Type)
            {
                case SerializedPropertyType.Integer:
                    {
                        if (field.Info.PropertyType == typeof(Int32))
                            field.SetValue(EditorGUILayout.IntField(field.Name, (int)field.GetValue(), emptyOptions));
                        else if (field.Info.PropertyType == typeof(Int64))
                            field.SetValue(EditorGUILayout.LongField(field.Name, (long)field.GetValue(), emptyOptions));
                    }
                    break;

                case SerializedPropertyType.Float:
                    {
                        if (field.Info.PropertyType == typeof(Single))
                            field.SetValue(EditorGUILayout.FloatField(field.Name, (float)field.GetValue(), emptyOptions));
                        else if (field.Info.PropertyType == typeof(Double))
                            field.SetValue(EditorGUILayout.DoubleField(field.Name, (double)field.GetValue(), emptyOptions));
                    }
                    break;

                case SerializedPropertyType.Boolean:
                    field.SetValue(EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.String:
                    field.SetValue(EditorGUILayout.TextField(field.Name, (String)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Vector2:
                    field.SetValue(EditorGUILayout.Vector2Field(field.Name, (Vector2)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Vector3:
                    field.SetValue(EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(), emptyOptions));
                    break;



                case SerializedPropertyType.Enum:
                    field.SetValue(EditorGUILayout.EnumPopup(field.Name, (Enum)field.GetValue(), emptyOptions));
                    break;

                default:

                    break;

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();

    }

    public static PropertyField[] GetProperties(System.Object obj)
    {

        List<PropertyField> fields = new List<PropertyField>();

        PropertyInfo[] infos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo info in infos)
        {

            if (!(info.CanRead && info.CanWrite))
                continue;

            object[] attributes = info.GetCustomAttributes(true);

            bool isExposed = false;

            foreach (object o in attributes)
            {
                if (o.GetType() == typeof(ExposePropertyAttribute))
                {
                    isExposed = true;
                    break;
                }
            }

            if (!isExposed)
                continue;

            SerializedPropertyType type = SerializedPropertyType.Integer;

            if (PropertyField.GetPropertyType(info, out type))
            {
                PropertyField field = new PropertyField(obj, info, type);
                fields.Add(field);
            }

        }

        return fields.ToArray();

    }

}


public class PropertyField
{
    System.Object m_Instance;
    PropertyInfo m_Info;
    SerializedPropertyType m_Type;

    MethodInfo m_Getter;
    MethodInfo m_Setter;

    public System.Object Instance
    {
        get
        {
            return m_Instance;
        }
    }

    public PropertyInfo Info
    {
        get
        {
            return m_Info;
        }
    }

    public SerializedPropertyType Type
    {
        get
        {
            return m_Type;
        }
    }

    public String Name
    {
        get
        {
            return ObjectNames.NicifyVariableName(m_Info.Name);
        }
    }

    public PropertyField(System.Object instance, PropertyInfo info, SerializedPropertyType type)
    {

        m_Instance = instance;
        m_Info = info;
        m_Type = type;

        m_Getter = m_Info.GetGetMethod();
        m_Setter = m_Info.GetSetMethod();
    }

    public System.Object GetValue()
    {
        return m_Getter.Invoke(m_Instance, null);
    }

    public void SetValue(System.Object value)
    {
        m_Setter.Invoke(m_Instance, new System.Object[] { value });
    }

    public static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType)
    {

        propertyType = SerializedPropertyType.Generic;

        Type type = info.PropertyType;

        if (type == typeof(int) || type == typeof(long))
        {
            propertyType = SerializedPropertyType.Integer;
            return true;
        }

        if (type == typeof(float) || type == typeof(double))
        {
            propertyType = SerializedPropertyType.Float;
            return true;
        }

        if (type == typeof(bool))
        {
            propertyType = SerializedPropertyType.Boolean;
            return true;
        }

        if (type == typeof(string))
        {
            propertyType = SerializedPropertyType.String;
            return true;
        }

        if (type == typeof(Vector2))
        {
            propertyType = SerializedPropertyType.Vector2;
            return true;
        }

        if (type == typeof(Vector3))
        {
            propertyType = SerializedPropertyType.Vector3;
            return true;
        }

        if (type.IsEnum)
        {
            propertyType = SerializedPropertyType.Enum;
            return true;
        }

        return false;

    }

}