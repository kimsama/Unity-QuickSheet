using System;
using System.Linq.Expressions;

namespace GDataDB.Linq.Impl {
    public class OrderTranslator : ExpressionVisitor {
        private string columnName;
        private bool descending;

        public Order Translate(Expression e) {
            Visit(e);
            if (columnName == null)
                return null;
            return new Order {ColumnName = columnName, Descending = descending};
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (m.Arguments.Count > 2)
                throw new NotSupportedException("OrderBy with comparer is not supported");
            if (m.Method.Name == "OrderBy") {
                Visit(m.Arguments[1]);
            } else if (m.Method.Name == "OrderByDescending") {
                descending = true;
                Visit(m.Arguments[1]);				
            }
            return m;
        }

        protected override Expression VisitMemberAccess(MemberExpression m) {
            columnName = m.Member.Name.ToLowerInvariant();
            return m;
        }
    }
}