using System;
using System.Linq.Expressions;

namespace OLinq
{
    public static class ObservableValueExtension
    {
        public static ObservableValue<TValue> Raise<TValue>(this ObservableValue<TValue> value, Expression<Func<object>> propertyOnParentClass, Action<string> raisePropertyChanged)
        {
            return value.Raise(PropertyName.For(propertyOnParentClass), raisePropertyChanged);
        }

        public static ObservableValue<TValue> Raise<TValue>(this ObservableValue<TValue> value, string propertyName, Action<string> raisePropertyChanged)
        {
            Action raise = () => raisePropertyChanged(propertyName);
            value.ValueChanged += (o, e) => raise();
            return value;
        }

        internal class PropertyName
        {
            public static string For<t>(Expression<Func<t, object>> expression)
            {
                Expression body = expression.Body;
                return GetMemberName(body);
            }

            public static string For(Expression<Func<object>> expression)
            {
                Expression body = expression.Body;
                return GetMemberName(body);
            }

            public static string GetMemberName(Expression expression)
            {
                if (expression is MemberExpression)
                {
                    var memberExpression = (MemberExpression)expression;

                    if (memberExpression.Expression.NodeType ==
                        ExpressionType.MemberAccess)
                    {
                        return GetMemberName(memberExpression.Expression)
                               + "."
                               + memberExpression.Member.Name;
                    }
                    return memberExpression.Member.Name;
                }

                if (expression is UnaryExpression)
                {
                    var unaryExpression = (UnaryExpression)expression;

                    if (unaryExpression.NodeType != ExpressionType.Convert)
                        throw new Exception(string.Format(
                            "Cannot interpret member from {0}",
                            expression));

                    return GetMemberName(unaryExpression.Operand);
                }

                throw new Exception(string.Format(
                    "Could not determine member from {0}",
                    expression));
            }
        }
    }
}