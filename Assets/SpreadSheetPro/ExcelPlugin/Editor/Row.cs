/*
   Tuner Data -  Read Static Data in Game Development.
   e-mail : dongliang17@126.com
*/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Excel
{
    public class Row
    {
        public Schema m_Schema = null;
        public List<Field> m_Fields = new List<Field>();

        public Row(Schema a_Schema)
        {
            m_Schema = a_Schema;
        }

        public Field GetField(int field)
        {
            if (field >= 0 && field < m_Fields.Count)
            {
                return m_Fields[field];
            }
            return null;
        }

        public Field GetField(string fieldName)
        {
            FieldDefine define = m_Schema.GetDefine(fieldName);
            return GetField(define.Index);
        }


    }
}