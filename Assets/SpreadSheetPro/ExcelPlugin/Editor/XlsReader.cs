using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using NPOI.HSSF.UserModel;

namespace Data.Excel
{
	/// <summary>
	/// Read .xls excel file
	/// </summary>
	public class XlsReader : IExcelReader
	{
		Schema schema = null;

		HSSFWorkbook hssWorkbook = null;
		List<Row> rowList = new List<Row>();

		string _path = "";
		string fileName = "";

		public string Path
		{
			get { return _path; }
			set { _path = value;}
		}

		public XlsReader(string path)
		{
			Init (path);
		}

		public void Init(string path)
		{
			Path = path;

			this.fileName = Util.GetFileName (path);

			if (InitHssf())
			{
				//TODO: change to retrieve sheet index by argument
				int sheetIndex = 0;
				LoadSchema(sheetIndex);
				LoadData(sheetIndex);
			}
			else
				Debug.Log ("Failed to creaate HSSFWorkbook instance.");

		}

		/*
		string GetFileName(string path)
		{
			string[] filePath = path.Split(new char[] { '/' });
			string filename = filePath[filePath.Length - 1];
			string[] onlyFilename = filename.Split(new char[] { '.' });

			return onlyFilename[0];
		}
		*/

		bool InitHssf()
		{
			if (hssWorkbook == null)
			{
				using (FileStream fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read))
				{
					hssWorkbook = new HSSFWorkbook(fileStream);
				}
			}

			if (hssWorkbook != null)
				return true;

			return false;
		}

		bool LoadSchema(int sheetIndex)
		{
			this.schema = new Schema ();
			if (this.schema == null)
				return false;

			this.schema.ClassName = this.fileName;

			HSSFSheet sheet = (HSSFSheet)this.hssWorkbook.GetSheetAt (sheetIndex);

			HSSFRow row0 = (HSSFRow)sheet.GetRow (0);
			HSSFRow row1 = (HSSFRow)sheet.GetRow (1);

			for(int i=0; i<row0.LastCellNum; ++i)
			{
				FieldDefine fieldDefine = new FieldDefine();
				fieldDefine.FieldName = row0.GetCell(i).ToString();
				fieldDefine.Index = i;

				string strType = row1.GetCell(i).ToString().ToLower();

				if (strType == "int")
					fieldDefine.FieldType = FIELD_TYPE.T_INT;
				else if (strType == "float")
					fieldDefine.FieldType = FIELD_TYPE.T_FLOAT;
				else if (strType == "string")
					fieldDefine.FieldType = FIELD_TYPE.T_STRING;
				else
					fieldDefine.FieldType = FIELD_TYPE.T_INVALID;

				this.schema.AddDefine(fieldDefine);

				Debug.Log("FieldName: " +fieldDefine.FieldName);
			}

			return true;
		}

		int startRow = 2;
		bool LoadData(int sheetIndex)
		{
			if (this.schema == null)
				return false;

			HSSFSheet sheet = (HSSFSheet)this.hssWorkbook.GetSheetAt (sheetIndex);
			if (sheet != null)
			{
				int currentRowIndex = 0;

				System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
				while(rows.MoveNext())
				{
					// skip a row if it is field description and its type
					if (currentRowIndex < startRow)
					{
						currentRowIndex++;
						continue;
					}

					Row tmpRow = new Row(this.schema);
					HSSFRow currentRow = (HSSFRow)rows.Current;

					for (int i=0; i<currentRow.LastCellNum; i++)
					{
						HSSFCell cell = (HSSFCell)currentRow.GetCell(i);
						FieldDefine fieldDefine = this.schema.GetDefine(i);
						Field field = null;
						if (cell != null)
						{
							switch(fieldDefine.FieldType)
							{
							case FIELD_TYPE.T_INT:
								int intValue = (int)cell.NumericCellValue;
								field = new Field(intValue);
								break;

							case FIELD_TYPE.T_FLOAT:
								float floatValue = (float)cell.NumericCellValue;
								field = new Field(floatValue);
								break;

							case FIELD_TYPE.T_STRING:
								string stringValue = cell.StringCellValue;
								field = new Field(stringValue);
								break;
							}
						}
						else
						{
							switch(fieldDefine.FieldType)
							{
							case FIELD_TYPE.T_INT:
								field = new Field(0);
								break;
							case FIELD_TYPE.T_FLOAT:
								field = new Field(0f);
								break;
							case FIELD_TYPE.T_STRING:
								field = new Field("");
								break;
							}
						}

						tmpRow.m_Fields.Add(field);

						Debug.Log ("FieldValue at [" + currentRowIndex.ToString() + "]" + " : " + field.m_Value.ToString());
					}

					this.rowList.Add (tmpRow);
					currentRowIndex++;
				}
			}
			else
				return false;

			return true;
		}

		public Schema ReadSchema()
		{
			return schema;
		}
		
		public Row[] ReadData()
		{			
			if (rowList.Count > 0)
			{
				return rowList.ToArray();
			}
			else
			{
				return null;
			}
		}		

	}
}