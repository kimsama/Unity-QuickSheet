/*
   Tuner Data -  Read Static Data in Game Development.
   e-mail : dongliang17@126.com
*/
namespace Data.Excel
{
    public class Field
    {
        public Field(object value)
        {
            m_Value = value;
        }
        public object m_Value;

        public int GetInt()
        {
            return (int)m_Value;
        }

        public float GetFloat()
        {
            return (float)m_Value;
        }
        public string GetString()
        {
            return m_Value.ToString();
        }
    }
}