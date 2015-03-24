///////////////////////////////////////////////////////////////////////////////
///
/// BaseDatabase.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BaseDatabase : ScriptableObject 
{
    [HideInInspector] [SerializeField] 
    public string sheetName = "";
    
    [HideInInspector] [SerializeField] 
    public string worksheetName = "";
    
    [ExposeProperty]
    public string SheetName 
    {
        get { return sheetName; }
        set { sheetName = value;}
    }
    
    [ExposeProperty]
    public string WorksheetName
    {
        get { return worksheetName; }
        set { worksheetName = value;}
    }
    
    void OnEnable()
    {
        
    }
}
