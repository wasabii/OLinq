using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public static class OperationFactory
    {

        /// <summary>
        /// Creates a typed <see cref="Operation"/> that properly wraps <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static IOperation<T> FromExpression<T>(OperationContext context, Expression expression)
        {
            return (IOperation<T>)FromExpression(context, expression);
        }

        /// <summary>
        /// Creates an <see cref="Operation"/> that properly wraps <paramref name="expression"/>, with the given
        /// <see cref="OperationContext"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static IOperation FromExpression(OperationContext context, Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return (IOperation)Activator.CreateInstance(typeof(ConstantOperation<>).MakeGenericType(expression.Type), context, expression);
                case ExpressionType.Quote:
                    return (IOperation)Activator.CreateInstance(typeof(QuoteOperation<>).MakeGenericType(expression.Type), context, expression);
                case ExpressionType.Call:
                    return FromCallExpression(context, (MethodCallExpression)expression);
                case ExpressionType.MemberAccess:
                    return (IOperation)Activator.CreateInstance(typeof(MemberAccessOperation<>).MakeGenericType(expression.Type), context, expression);
                case ExpressionType.Lambda:
                    return FromLambdaExpression(context, (LambdaExpression)expression);
                case ExpressionType.Parameter:
                    return (IOperation)Activator.CreateInstance(typeof(ParameterOperation<>).MakeGenericType(expression.Type), context, expression);
                case ExpressionType.New:
                    return (IOperation)Activator.CreateInstance(typeof(NewOperation<>).MakeGenericType(expression.Type), context, expression);
                case ExpressionType.MemberInit:
                    return (IOperation)Activator.CreateInstance(typeof(MemberInitOperation<>).MakeGenericType(expression.Type), context, expression);
            }

            throw new NotSupportedException(string.Format("{0} expression not supported.", expression.NodeType));
        }

        /// <summary>
        /// Creates an <see cref="Operation"/> that properly wraps <paramref name="expression"/>, if expression is a Call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static IOperation FromCallExpression(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(Queryable))
                return FromQueryableExpression(context, expression);
            else
                return (IOperation)Activator.CreateInstance(typeof(CallOperation<>).MakeGenericType(expression.Type), context, expression);
        }

        /// <summary>
        /// Creates an <see cref="Operation"/> that implements the appropriate <see cref="Queryable"/> method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static IOperation FromQueryableExpression(OperationContext context, MethodCallExpression expression)
        {
            Type resultItemType, sourceItemType;

            switch (expression.Method.Name)
            {
                case "Concat":
                    resultItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(ConcatOperation<>).MakeGenericType(resultItemType), context, expression);
                case "Select":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    resultItemType = expression.Method.GetGenericArguments()[1];
                    return (IOperation)Activator.CreateInstance(typeof(SelectOperation<,>).MakeGenericType(sourceItemType, resultItemType), context, expression);
                case "Where":
                    resultItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(WhereOperation<>).MakeGenericType(resultItemType), context, expression);
                case "Any":
                    return new AnyOperation(context, expression);
                case "Count":
                    return new CountOperation(context, expression);
                default:
                    throw new NotSupportedException();
            }
        }

        private static IOperation FromLambdaExpression(OperationContext context, LambdaExpression expression)
        {
            return (IOperation)Activator.CreateInstance(typeof(LambdaOperation<>).MakeGenericType(expression.ReturnType), context, expression);
        }

    }

}
