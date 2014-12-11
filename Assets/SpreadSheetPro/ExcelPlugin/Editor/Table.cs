/*
   Tuner Data -  Read Static Data in Game Development.
   e-mail : dongliang17@126.com
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Data;

namespace Data.Excel
{
	/// <summary>
	/// Table class which represents a worksheet on a excel file.
	/// </summary>
    public class Table
    {
        public List<Row> m_DataRows = new List<Row>();
        public Schema m_Schema = null;//***********
        private Dictionary<int, int> m_IndexMap = new Dictionary<int, int>();
        public Table(IExcelReader reader)
        {
            m_Schema = reader.ReadSchema();
            Row[] tempRows = reader.ReadData();
            foreach (Row item in tempRows)
            {
                m_DataRows.Add(item);
                m_IndexMap.Add((int)item.m_Fields[0].m_Value, m_DataRows.Count - 1);
            }
        }
        public int RecordNum
        {
            get { return m_DataRows.Count; }
        }

        public int FieldNum
        {
            get { return m_Schema.Count; }
        }

        public Field GetField(int index, int field)
        {
            Row row = GetRow(index);
            if (row != null)
            {
                return row.GetField(field);
            }
            return null;
        }

        public Field GetField(int index, string fieldname)
        {
            Row row = GetRow(index);
            if (row != null)
            {
                return row.GetField(fieldname);
            }
            return null;
        }

        public Row GetRow(int index)
        {
            int row = 0;
            if (m_IndexMap.TryGetValue(index, out row))
            {
                return m_DataRows[row];
            }
            return null;
        }

        public void SetField(int index, int field, object value)
        {
            Field tempField = GetField(index, field);
            tempField.m_Value = value;
        }

        public Row Search_First_Column_Equ(int field, object value)
        {
            foreach (Row item_row in m_DataRows)
            {
                switch (m_Schema.GetDefine(field).FieldType)
                {
                    case FIELD_TYPE.T_INT:
                        if ((int)value == (int)item_row.m_Fields[field].m_Value)
                        {
                            return item_row;
                        }
                        break;
                    case FIELD_TYPE.T_FLOAT:
                        if ((float)value == (float)item_row.m_Fields[field].m_Value)
                        {
                            return item_row;
                        }
                        break;
                    case FIELD_TYPE.T_STRING:
                        if ((string)value == (string)item_row.m_Fields[field].m_Value)
                        {
                            return item_row;
                        }
                        break;
                }
            }
            return null;
        }

        public string GetFieldName(int field)
        {
            return m_Schema.GetDefine(field).FieldName;
        }

        public FIELD_TYPE GetFieldType(int field)
        {
            return m_Schema.GetDefine(field).FieldType;
        }
/*@kims
        public bool Write(IDataWriter writer)
        {
            return writer.Write(m_Schema, m_DataRows.ToArray());
        }

        public T GetStruct<T>(int index) where T : ITDStruct, new()
        {
            Row row = GetRow(index);
            T temp = new T();
            if (row != null)
            {              
                temp.Init(row);
               
            }
            return temp;
        }
*/            
    }

}
