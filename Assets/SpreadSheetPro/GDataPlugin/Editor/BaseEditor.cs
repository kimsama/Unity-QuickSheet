///////////////////////////////////////////////////////////////////////////////
///
/// BaseEditor.cs
/// 
/// (c)2013 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

// to resolve TlsException error.
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Google.GData.Client;
using Google.GData.Spreadsheets;

/// 
/// A BaseEditor class.
/// 
public class BaseEditor<T> : Editor //where T : BaseDatabase
{	
    protected string username;
    protected string password;
    
    // custom data 
    //protected BaseDatabase database; 
    
    // property draw
    protected PropertyField[] databaseFields;
    protected PropertyField[] dataFields;
    
    protected List<PropertyField[]> pInfoList = new List<PropertyField[]>();
        
    /// 
    /// Actively ignore security concerns to resolve TlsException error.
    /// 
    /// See: http://www.mono-project.com/UsingTrustedRootsRespectfully
    ///
    public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, 
                                  SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public virtual void OnEnable()
    {
        // resolves TlsException error
        ServicePointManager.ServerCertificateValidationCallback = Validator;

        GoogleDataSettings settings = GoogleDataSettings.Instance;		
        if (settings != null)
        {
            username = settings.Account;
            password = settings.Password;
        }
        else
        {
            Debug.LogError("Failed to get google data settings. See the google data setting if it has correct path.");
            return;
        }
                
        //database = target as BaseDatabase;
        //Debug.Log ("Target type: " + database.GetType().ToString());
    }

    public override void OnInspectorGUI()
    { 		
        ShowAuthenticastion();
        
        if (target == null)
            return;
        
        //this.DrawDefaultInspector();
        ExposeProperties.Expose(databaseFields);
 
        foreach(PropertyField[] p in pInfoList)
        {
            ExposeProperties.Expose( p );	
        }

    }
    
    /// 
    /// Should be reimplemented in derived class.
    /// 
    public virtual bool Load()
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("User account or password is empty.");
            return false;
        }
        
        return true;
    }
    
    protected List<int> SetArrayValue(string from)
    {
        List<int> tmp = new List<int>();

        CsvParser parser = new CsvParser(from);

        foreach(string s in parser)
        {
            Debug.Log("parsed value: " + s);
            tmp.Add(int.Parse(s));
        }

        return tmp;
    }	

    void ShowAuthenticastion()
    {		
        username = EditorGUILayout.TextField("Username", username);
        password = EditorGUILayout.PasswordField("Password", password);
        
        if (GUILayout.Button("Download"))
        {
            if (!Load ())
                Debug.LogError("Failed to Load data from Google.");
        }				
    }
        
    /*
    static string[] SplitCamelCase(string stringToSplit)
    {
        if (!string.IsNullOrEmpty(stringToSplit))
        {
            List<string> words = new List<string>();

            string temp = string.Empty;
                
            foreach (char ch in stringToSplit)
            {
                if (ch >= 'a' && ch <= 'z')
                    temp = temp + ch;
                else
                {
                    words.Add(temp);
                    temp = string.Empty + ch;
                }
            }
            words.Add(temp);
            return words.ToArray();
        }
        else
            return null;
    }
    */
    
    public static string SplitCamelCase(string inputCamelCaseString)
    {
        string sTemp = Regex.Replace(inputCamelCaseString, "([A-Z][a-z])", " $1", RegexOptions.Compiled).Trim();
        return Regex.Replace(sTemp, "([A-Z][A-Z])", " $1", RegexOptions.Compiled).Trim();
    }	
}
