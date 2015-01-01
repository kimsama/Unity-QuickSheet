///////////////////////////////////////////////////////////////////////////////
///
/// ExcelQuery.cs
/// 
/// (c)2014 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using System.ComponentModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

/// <summary>
/// 
/// </summary>
public class ExcelQuery 
{
    private readonly IWorkbook workbook = null;
    private readonly ISheet sheet = null;

    /// <summary>
    /// 
    /// </summary>
    public ExcelQuery(string path, string sheetName = "")
    {
        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                string extension = GetSuffix(path);

                if (extension == "xls")
                    workbook = new HSSFWorkbook(fileStream);
                else if (extension == "xlsx")
                    workbook = new XSSFWorkbook(fileStream);
                else
                {
                    throw new Exception("Wrong file.");
                }

                if (!string.IsNullOrEmpty(sheetName))
                    sheet = workbook.GetSheet(sheetName);
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// Retrieves file extension only from the given file path.
    /// </summary>
    static string GetSuffix(string path)
    {
        string[] arg1 = path.Split(new char[] { '\\' });
        string str1 = arg1[arg1.Length - 1];
        string[] arg2 = str1.Split(new char[] { '.' });

        return arg2[1];
    }

    /// <summary>
    /// 
    /// </summary>
    public List<T> Deserialize<T>(int start = 1)
    {
        var t = typeof(T);
        PropertyInfo[] p = t.GetProperties();

        var result = new List<T>();

        int current = 0;
        foreach(IRow row in sheet)
        {
            if (current < start)
            {
                current++; // skip header column.
                continue;
            }

            //var item = new T();
            var item = (T)Activator.CreateInstance(t);
            for(var i=0; i<p.Length; i++)
            {
                ICell cell = row.GetCell(i);

                var property = p[i];
                if (property.CanWrite)
                {
                    try
                    {
                        var value = ConvertFrom(cell, property.PropertyType);
                        property.SetValue(item, value, null);
                        //Debug.Log("cell value: " + value.ToString());
                    }
                    catch(Exception e)
                    {
                        string pos = string.Format("Row[{0}], Cell[{1}]", current.ToString(), i.ToString());
                        Debug.LogError("Excel Deserialize Exception: " + e.Message + pos);
                    }
                }
            }

            result.Add(item);

            current++;
        }

        return result;
    }

    /// <summary>
    /// Retrieves all sheet names.
    /// </summary>
    public string[] GetSheetNames()
    {
        List<string> sheetList = new List<string>();
        if (this.workbook != null)
        {
            int numSheets = this.workbook.NumberOfSheets;
            for (int i=0; i<numSheets; i++)
            {
                sheetList.Add(this.workbook.GetSheetName(i));
            }
        }
        else
            Debug.LogError("Workbook is null. Did you forget to import excel file first?");

        return (sheetList.Count > 0) ? sheetList.ToArray() : null;
    }

    /// <summary>
    /// Retrieves all first columns(aka. header) which are needed to determine type of each cell.
    /// </summary>
    public string[] GetTitle(int start = 0)
    {
        List<string> result = new List<string>();

        IRow title = sheet.GetRow(start);
        if (title != null)
        {
            for (int i = 0; i < title.LastCellNum; i++)
                result.Add(title.GetCell(i).StringCellValue);

            return result.ToArray();
        }

        return null;
    }

    /// <summary>
    /// TODO: 
    ///     may need to move abstract class or static class.
    /// </summary>
    protected object ConvertFrom(ICell cell, Type t)
    {
        object value = null;

        if (t == typeof(float) || t == typeof(double) || t == typeof(int))
            value = cell.NumericCellValue;
        else if (t == typeof(string))
            value = cell.StringCellValue;
        else if (t == typeof(bool))
            value = cell.BooleanCellValue;

        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            var nc = new NullableConverter(t);
            return nc.ConvertFrom(value);
        }

        //HACK: modified to return enum.
        if (t.IsEnum)
        {
            return Enum.Parse(t, value.ToString(), true);
        }
        else
            return Convert.ChangeType(value, t);
        
    }
}
