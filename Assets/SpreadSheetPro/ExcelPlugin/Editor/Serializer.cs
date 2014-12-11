using UnityEngine;
using System;
using System.ComponentModel;
using System.Collections;
using System.Reflection;

namespace Data.Excel
{
	public class Serializer<T> 
	{
		public Row Serialize(T e) 
		{
			return Serialize(e, new Row(CreateSchema(e)));
		}

		public Row Serialize(T e, Row row) 
		{
			foreach (var p in typeof (T).GetProperties()) 
			{
				if (p.CanRead) 
				{
					Field field = new Field( p.GetValue(typeof(T), null));
					row.m_Fields.Add(field);
				}
			}
			return row;
		}

		Schema CreateSchema(T e)
		{
			Schema schema = new Schema ();

			int i = 0; 
			foreach (var p in typeof (T).GetProperties()) 
			{
				if (p.CanRead) 
				{

					FieldDefine fieldDefine = new FieldDefine();
					fieldDefine.FieldName = p.Name;
					fieldDefine.Index = i;

					if (p.PropertyType == typeof(int))
						fieldDefine.FieldType = FIELD_TYPE.T_INT;
					else if (p.PropertyType == typeof(float))
						fieldDefine.FieldType = FIELD_TYPE.T_FLOAT;
					else if (p.PropertyType == typeof(string))
						fieldDefine.FieldType = FIELD_TYPE.T_STRING;
					else
						fieldDefine.FieldType = FIELD_TYPE.T_INVALID;

					schema.AddDefine(fieldDefine);
				}
				i++;
			}

			return schema;
		}

		public string ToNullOrString(object o) 
		{
			if (o == null)
				return null;
			return o.ToString();
		}

		public object ConvertFrom(object value, Type t) 
		{
			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof (Nullable<>))) 
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
		
		public T Deserialize(Row e) 
		{
			var t = typeof (T);
			var r = (T) Activator.CreateInstance(t);

			int index = 0;

			foreach(Field c in e.m_Fields) 
			{
				FieldDefine fieldDefine = e.m_Schema.GetDefine(index++);
				string fieldName = fieldDefine.FieldName;
				var property = t.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
				if (property == null)
					continue;
				if (property.CanWrite) 
				{					
					try
					{
						var value = ConvertFrom(c.m_Value, property.PropertyType);
						property.SetValue(r, value, null);
					}
					catch(Exception exc)
					{
						Debug.LogError ("ExcelReader Serialization Exception: " + exc.Message + " Cell LocalName: " + fieldName);
					}
				}
			}
			return r;
		}
	}
}