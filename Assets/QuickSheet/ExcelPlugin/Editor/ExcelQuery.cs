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

namespace UnityQuickSheet
{
    /// <summary>
    /// Query each of cell data from the given excel sheet and deserialize it to the ScriptableObject's data array.
    /// </summary>
    public class ExcelQuery
    {
        private readonly IWorkbook workbook = null;
        private readonly ISheet sheet = null;
        private string filepath = string.Empty;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExcelQuery(string path, string sheetName = "")
        {
            try
            {
                this.filepath = path;

                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    string extension = GetSuffix(path);

                    if (extension == "xls")
                        workbook = new HSSFWorkbook(fileStream);
                    else if (extension == "xlsx")
                    {
                    #if UNITY_EDITOR_OSX
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
            catch (Exception e)
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
            foreach (IRow row in sheet)
            {
                if (current < start)
                {
                    current++; // skip header column.
                    continue;
                }

                var item = (T)Activator.CreateInstance(t);
                for (var i = 0; i < p.Length; i++)
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
                                //NOTE: enum array type is not supported. (Does it really needed?)

                                const char DELIMETER = ',';
                                string str = value as string;

                                // remove whitespace between each of element
                                str = new string(str.ToCharArray()
                                                    .Where(ch => !Char.IsWhiteSpace(ch))
                                                    .ToArray());

                                // remove ',', if it is found at the end.
                                char[] charToTrim = { ',', ' ' };
                                str = str.TrimEnd(charToTrim);

                                // split by ','
                                object[] temp = str.Split(DELIMETER);

                                Array array = (Array)Activator.CreateInstance(property.PropertyType, temp.Length);

                                for (int j = 0; j < array.Length; j++)
                                {
                                    array.SetValue(Convert.ChangeType(temp[j], property.PropertyType.GetElementType()), j);
                                }

                                property.SetValue(item, array, null);
                            }
                            else
                                property.SetValue(item, value, null);

                        }
                        catch (Exception e)
                        {
                            string pos = string.Format("Row[{0}], Cell[{1}]", (current + 1).ToString(), GetHeaderColumnName(i));
                            Debug.LogError(string.Format("Excel File {0} Deserialize Exception: {1} at {2}", this.filepath, e.Message, pos));
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
                for (int i = 0; i < numSheets; i++)
                {
                    sheetList.Add(this.workbook.GetSheetName(i));
                }
            }
            else
                Debug.LogError("Workbook is null. Did you forget to import excel file first?");

            return (sheetList.Count > 0) ? sheetList.ToArray() : null;
        }

        /// <summary>
        /// Retrieves all first columns(aka. header column) which are needed to determine each type of a cell.
        /// </summary>
        public string[] GetTitle(int start, ref string error)
        {
            List<string> result = new List<string>();

            IRow title = sheet.GetRow(start);
            if (title != null)
            {
                for (int i = 0; i < title.LastCellNum; i++)
                {
                    string value = title.GetCell(i).StringCellValue;
                    if (string.IsNullOrEmpty(value))
                    {
                        error = string.Format(@"Empty column is found at {0}.", i);
                        return null;
                    }
                    else
                    {
                        // column header is not an empty string, we check its validation later.
                        result.Add(value);
                    }
                }

                return result.ToArray();
            }
            
            error = string.Format(@"Empty row at {0}", start);
            return null;
        }

        /// <summary>
        /// Convert type of cell value to its predefined type in the sheet's ScriptMachine setting file.
        /// </summary>
        protected object ConvertFrom(ICell cell, Type t)
        {
            object value = null;

            if (t == typeof(float) || t == typeof(double) || t == typeof(int) || t == typeof(long))
            {
                if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
                    value = cell.NumericCellValue;
                else if (cell.CellType == NPOI.SS.UserModel.CellType.String)
                {
                    //Get correct numeric value even the cell is string type but defined with a numeric type in a data class.
                    if (t == typeof(float))
                        value = Convert.ToSingle(cell.StringCellValue);
                    if (t == typeof(double))
                        value = Convert.ToDouble(cell.StringCellValue);
                    if (t == typeof(int))
                        value = Convert.ToInt32(cell.StringCellValue);
                    if (t == typeof(long))
                        value = Convert.ToInt64(cell.StringCellValue);
                }
            }
            else if (t == typeof(string) || t.IsArray)
            {
                // HACK: handles the case that a cell contains numeric value 
                //       but a member field in a data class is defined as string type.
                if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
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
}