using System.Linq;
using GDataDB.Linq.Impl;

namespace GDataDB.Linq {
	public static class ITableExtensions {
		public static IQueryable<T> AsQueryable<T>(this ITable<T> t) {
			return new Query<T>(new GDataDBQueryProvider<T>(t));
		}
	}
}