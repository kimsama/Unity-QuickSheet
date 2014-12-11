using UnityEngine;
using UnityEditor;

using System;
using System.ComponentModel;
using System.Reflection;
using Google.GData.Spreadsheets;

namespace GDataDB.Impl {
    /// <summary>
    /// (de)serializes an object into a spreadsheet row
    /// Uses only the object properties.
    /// Property names are used as column names in the spreadsheet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Serializer<T> {
        public ListEntry Serialize(T e) {
            return Serialize(e, new ListEntry());
        }

        public ListEntry Serialize(T e, ListEntry record) {
            foreach (var p in typeof (T).GetProperties()) {
				if (p.CanRead) {
					record.Elements.Add(new ListEntry.Custom {
						LocalName = p.Name.ToLowerInvariant(), // for some reason this HAS to be lowercase or it throws
						Value = ToNullOrString(p.GetValue(e, null)),
					});
				}
            }
            return record;
        }

        public string ToNullOrString(object o) {
            if (o == null)
                return null;
            return o.ToString();
        }

        public object ConvertFrom(object value, Type t) {
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof (Nullable<>))) {
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

        public T Deserialize(ListEntry e) {
            var t = typeof (T);
            var r = (T) Activator.CreateInstance(t);
            foreach (ListEntry.Custom c in e.Elements) {
                var property = t.GetProperty(c.LocalName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                    continue;
				if (property.CanWrite) {
					
					try
					{
						var value = ConvertFrom(c.Value, property.PropertyType);
						property.SetValue(r, value, null);
					}
					catch(Exception exc)
					{
						Debug.LogError ("GDataDB Serialization Exception: " + exc.Message + " ListEntry LocalName: " + c.LocalName);
					}
				}
            }
            return r;
        }
    }
}