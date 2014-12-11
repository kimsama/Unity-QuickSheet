/*
   Tuner Data -  Read Static Data in Game Development.
   e-mail : dongliang17@126.com
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Excel
{
    public class Schema
    {
        string m_ClassName;
        public string ClassName
        {
            get { return m_ClassName; }
            set { m_ClassName = value; }
        }
        Dictionary<int, FieldDefine> m_Define_IndexMap = new Dictionary<int, FieldDefine>();
        Dictionary<string, FieldDefine> m_Define_NameMap = new Dictionary<string, FieldDefine>();

        public FieldDefine AddDefine(FieldDefine a_FieldDefine)
        {
            m_Define_IndexMap.Add(a_FieldDefine.Index, a_FieldDefine);
            m_Define_NameMap.Add(a_FieldDefine.FieldName, a_FieldDefine);
            return a_FieldDefine;
        }

        public FieldDefine GetDefine(string name)
        {
            FieldDefine temp = null;
            m_Define_NameMap.TryGetValue(name, out temp);
            return temp;
        }

        public FieldDefine GetDefine(int index)
        {
            FieldDefine temp = null;
            m_Define_IndexMap.TryGetValue(index, out temp);
            return temp;
        }

        public int Count
        {
            get { return m_Define_IndexMap.Count; }

        }
    }
}
