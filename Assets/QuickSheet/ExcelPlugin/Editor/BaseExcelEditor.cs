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
using System.IO;

/// <summary>
/// Base class of the created .asset ScriptableObject class.
/// </summary>
public class BaseExcelEditor<T> : Editor
{

    // to reflect properties on the Inspector view.
    protected PropertyField[] databaseFields;
    protected PropertyField[] dataFields;

    protected List<PropertyField[]> pInfoList = new List<PropertyField[]>();

    public virtual void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Update"))
        {
            if (!Load())
            {
                const string error1 = "\n- Check the path of the 'Sheet Name' and the file is exist at the path.";
                const string error2 = "\n- Also check the excel file has the sheet which matches with 'Worksheet Name'.";
                EditorUtility.DisplayDialog(
                    "Error",
                    "Failed to import and update the excel file." + error1 + error2,
                    "OK"
                );
            }
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
    /// Called when 'Update' button is pressed. It should be reimplemented in the derived class.
    /// 
    public virtual bool Load()
    {
        return false;
    }
}
