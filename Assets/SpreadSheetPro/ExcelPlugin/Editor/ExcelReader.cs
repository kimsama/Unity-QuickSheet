using UnityEngine;
using System.Collections;

namespace Data.Excel
{
	public class ExcelReader 
	{
		public static IExcelReader Open(string path)
		{
			return GetReader (path);;
		}

		static IExcelReader GetReader(string path)
		{
			string _path = path;
			
			string suffix = GetSuffix(_path);

			IExcelReader reader = null;

			if (suffix == "xls")
				reader = new XlsReader (path);
			else if (suffix == "xlsx")
				reader = new XlsxReader(path);
			else
				Debug.LogError("");

			return reader;
		}

		static string GetSuffix(string path)
		{
			string[] arg1 = path.Split(new char[]{'\\'});
			string str1 = arg1[arg1.Length - 1];
			string[] arg2 = str1.Split(new char[] {'.'});

			return arg2[1];
		}

        //public virtual string[] GetTitle(string sheetName)
        //{
        //    return null;
        //}
	}
}
