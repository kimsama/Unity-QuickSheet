using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GDataDB.Linq.Impl {
    /// <summary>
    /// From http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// </summary>
    public abstract class QueryProvider : IQueryProvider {
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression) {
            return new Query<S>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression) {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try {
                return (IQueryable) Activator.CreateInstance(typeof (Query<>).MakeGenericType(elementType), new object[] {this, expression});
            } catch (TargetInvocationException tie) {
                throw tie.InnerException;
            }
        }

        public abstract Query GetQuery(Expression expression);
        public abstract object Execute(Expression expression);

        public TResult Execute<TResult>(Expression expression) {
            return (TResult) Execute(expression);
        }
    }
}