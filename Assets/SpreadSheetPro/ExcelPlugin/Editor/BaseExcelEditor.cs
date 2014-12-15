///////////////////////////////////////////////////////////////////////////////
///
/// BaseExelEditor.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class BaseExcelEditor<T> : Editor
{

    // property draw
    protected PropertyField[] databaseFields;
    protected PropertyField[] dataFields;

    protected List<PropertyField[]> pInfoList = new List<PropertyField[]>();

    public virtual void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        //TODO: show excel file path and worksheet name.

        if (GUILayout.Button("Import"))
        {
            if (!Load())
                Debug.LogError("Failed to import excel file.");
        }

        if (target == null)
            return;

        //this.DrawDefaultInspector();
        ExposeProperties.Expose(databaseFields);

        foreach (PropertyField[] p in pInfoList)
        {
            ExposeProperties.Expose(p);
        }
    }

    /// 
    /// Should be reimplemented in derived class.
    /// 
    public virtual bool Load()
    {
        //TODO: check file path and worksheet name is valid.

        return true;
    }
}
