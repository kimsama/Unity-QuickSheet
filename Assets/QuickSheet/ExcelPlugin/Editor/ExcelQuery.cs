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
using System.Linq;
using System.ComponentModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

/// <summary>
/// Query each of cell data from the given excel sheet and deserialize it to the ScriptableObject's data array.
/// </summary>
public class ExcelQuery 
{
    private readonly IWorkbook workbook = null;
    private readonly ISheet sheet = null;

    /// <summary>
    /// Constructor.
    /// </summary>
    public ExcelQuery(string path, string sheetName = "")
    {
        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                string extension = GetSuffix(path);

                if (extension == "xls")
                    workbook = new HSSFWorkbook(fileStream);
                else if (extension == "xlsx")
                {
#if UNITY_MAC
                    throw new Exception("xlsx is not supported on OSX.");
#else
                    workbook = new XSSFWorkbook(fileStream);
#endif
                }
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
    /// Determine whether the excel file is successfully read in or not.
    /// </summary>
    public bool IsValid()
    {
        if (this.workbook != null && this.sheet != null)
            return true;

        return false;
    }

    /// <summary>
    /// Retrieves file extension only from the given file path.
    /// </summary>
    static string GetSuffix(string path)
    {
        string ext = Path.GetExtension(path);
        string[] arg = ext.Split(new char[] { '.' });
        return arg[1];
    }

    string GetHeaderColumnName(int cellnum)
    {
        ICell headerCell = sheet.GetRow(0).GetCell(cellnum);
        if (headerCell != null)
            return headerCell.StringCellValue;
        return string.Empty;
    }

    /// <summary>
    /// Deserialize all the cell of the given sheet.
    /// 
    /// NOTE:
    ///     The first row of a sheet is header column which is not the actual value 
    ///     so it skips when it deserializes.
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

                        if (property.PropertyType.IsArray)
                        {
                            // collect string values.
                            CsvParser parser = new CsvParser(value as string);
                            List<string> tokenizedValues = new List<string>();
                            foreach (string s in parser)
                                tokenizedValues.Add(s);

                            // convert string to corresponded type of the array element.
                            object[] convertedValues = null;
                            if (property.PropertyType.GetElementType() == typeof(string))
                                convertedValues = tokenizedValues.ToArray();
                            else
                                convertedValues = tokenizedValues.ToArray().Select(s => Convert.ChangeType(s, property.PropertyType.GetElementType())).ToArray();
                            
                            // set converted string value to the array.
                            Array array = (Array)property.GetValue(item, null);
                            if (array != null)
                            {
                                // initialize the array of the data class
                                array = Array.CreateInstance(property.PropertyType.GetElementType(), convertedValues.Length);
                                for (int j = 0; j < convertedValues.Length; j++)
                                    array.SetValue(convertedValues[j], j);

                                // should do deep copy
                                property.SetValue(item, array, null);
                            }
                        }
                        else
                            property.SetValue(item, value, null);

                        //Debug.Log("cell value: " + value.ToString());
                    }
                    catch(Exception e)
                    {
                        string pos = string.Format("Row[{0}], Cell[{1}]", current.ToString(), GetHeaderColumnName(i + 1));
                        Debug.LogError("Excel Deserialize Exception: " + e.Message + " " + pos);
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
    /// Convert type of cell value to its predefined type in the sheet's ScriptMachine setting file.
    /// </summary>
    protected object ConvertFrom(ICell cell, Type t)
    {
        object value = null;

        if (t == typeof(float) || t == typeof(double) || t == typeof(int))
            value = cell.NumericCellValue;
        else if (t == typeof(string) || t.IsArray)
        {
            // HACK: handles the case that a cell contains numeric value 
            //       but a member field in a data class is defined as string type.
            if (cell.CellType == CellType.Numeric) 
                value = cell.NumericCellValue;
            else
                value = cell.StringCellValue;
        }
        else if (t == typeof(bool))
            value = cell.BooleanCellValue;

        if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            var nc = new NullableConverter(t);
            return nc.ConvertFrom(value);
        }

        if (t.IsEnum)
        {
            // for enum type, first get value by string then convert it to enum.
            value = cell.StringCellValue;
            return Enum.Parse(t, value.ToString(), true);
        }
        else if (t.IsArray)
        {
            // for array type, return comma separated string 
            // then parse and covert its corresponding type.
            return value as string;
        }
        else
        {
            // for all other types, convert its corresponding type.
            return Convert.ChangeType(value, t);
        }
    }
}
