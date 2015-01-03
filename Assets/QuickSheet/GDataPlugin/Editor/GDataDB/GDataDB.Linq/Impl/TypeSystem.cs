using System;
using System.Collections.Generic;

namespace GDataDB.Linq.Impl {
    /// <summary>
    /// From http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// </summary>
    internal static class TypeSystem {
        internal static Type GetElementType(Type seqType) {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type seqType) {
            if (seqType == null || seqType == typeof (string))
                return null;
            if (seqType.IsArray)
                return typeof (IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType) {
                foreach (var arg in seqType.GetGenericArguments()) {
                    Type ienum = typeof (IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType)) {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0) {
                foreach (var iface in ifaces) {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof (object)) {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }
    }
}