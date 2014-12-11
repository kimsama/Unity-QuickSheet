using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Data.Excel
{
	/// <summary>
	/// Read .xlsx excel file
	/// TODO: add load method
	/// </summary>
	public class XlsxReader : IExcelReader
	{
		Schema schema = null;

		List<Row> rowList = new List<Row>();

		public XlsxReader(string path)
		{
			//Init (path);
		}

		public Schema ReadSchema()
		{
			return schema;
		}
		
		public Row[] ReadData()
		{			
			if (rowList.Count > 0)
				return rowList.ToArray();
			else
				return null;
		}
	}
}
