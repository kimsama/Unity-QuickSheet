using System;
using System.Linq.Expressions;
using System.Text;

namespace GDataDB.Linq.Impl {
    public class WhereTranslator : ExpressionVisitor {
        private readonly StringBuilder sb = new StringBuilder();

        public string Translate(Expression e) {
            Visit(e);
            return sb.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (m.Method.Name == "Where") {
                Visit(m.Arguments[1]);
            }
            return m;
        }

        protected override Expression VisitBinary(BinaryExpression b) {
            sb.Append("(");
            switch (b.NodeType) {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    Visit(b.Left);
                    sb.Append("&&");
                    Visit(b.Right);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    Visit(b.Left);
                    sb.Append("||");
                    Visit(b.Right);
                    break;
                case ExpressionType.Equal:
                    Visit(b.Left);
                    sb.Append("=");
                    Visit(b.Right);
                    break;
                case ExpressionType.NotEqual:
                    Visit(b.Left);
                    sb.Append("!=");
                    Visit(b.Right);
                    break;
                case ExpressionType.LessThan:
                    Visit(b.Left);
                    sb.Append("<");
                    Visit(b.Right);
                    break;
                case ExpressionType.GreaterThan:
                    Visit(b.Left);
                    sb.Append(">");
                    Visit(b.Right);
                    break;
                case ExpressionType.LessThanOrEqual:
                    Visit(Expression.Or(Expression.LessThan(b.Left, b.Right), Expression.Equal(b.Left, b.Right)));
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Visit(Expression.Or(Expression.GreaterThan(b.Left, b.Right), Expression.Equal(b.Left, b.Right)));
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            sb.Append(")");
            return b;
        }

        protected override Expression VisitMemberAccess(MemberExpression m) {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter) {
                sb.Append(m.Member.Name.ToLowerInvariant());
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected override Expression VisitConstant(ConstantExpression c) {
            if (c.Value is string)
                sb.AppendFormat("\"{0}\"", c.Value);
            else
                sb.Append(c.Value.ToString());
            return c;
        }
    }
}