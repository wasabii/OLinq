﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public static class OperationFactory
    {

        private static Type Fix(Type type)
        {
            var oldArgs = type.GetGenericArguments();
            var newArgs = new Type[oldArgs.Length];
            for (int i = 0; i < oldArgs.Length; i++)
                newArgs[i] = Fix(oldArgs[i]);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>))
                return typeof(IEnumerable<>).MakeGenericType(newArgs);
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition().MakeGenericType(newArgs);
            else
                return type;
        }

        /// <summary>
        /// Creates a typed <see cref="Operation"/> that properly wraps <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static IOperation<T> FromExpression<T>(OperationContext context, Expression expression)
        {
            var op = FromExpression(context, expression);
            if (op is IOperation<T>)
                return (IOperation<T>)op;

            throw new InvalidOperationException("Generated operation does not return expected type.");
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
            // replace IQueryable references with IEnumerable
            var type = Fix(expression.Type);

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return FromConstantExpression(context, (ConstantExpression)expression);
                case ExpressionType.Quote:
                    return (IOperation)Activator.CreateInstance(typeof(QuoteOperation<>).MakeGenericType(type), context, expression);
                case ExpressionType.Call:
                    return FromCallExpression(context, (MethodCallExpression)expression);
                case ExpressionType.MemberAccess:
                    return (IOperation)Activator.CreateInstance(typeof(MemberAccessOperation<>).MakeGenericType(type), context, expression);
                case ExpressionType.Lambda:
                    return FromLambdaExpression(context, (LambdaExpression)expression);
                case ExpressionType.Parameter:
                    return (IOperation)Activator.CreateInstance(typeof(ParameterOperation<>).MakeGenericType(type), context, expression);
                case ExpressionType.New:
                    return (IOperation)Activator.CreateInstance(typeof(NewOperation<>).MakeGenericType(type), context, expression);
                case ExpressionType.MemberInit:
                    return (IOperation)Activator.CreateInstance(typeof(MemberInitOperation<>).MakeGenericType(type), context, expression);
                case ExpressionType.Equal:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return new BinaryOperation<bool>(context, (BinaryExpression)expression);
                case ExpressionType.Convert:
                    var inType = Fix(((UnaryExpression)expression).Operand.Type);
                    var outType = Fix(((UnaryExpression)expression).Type);
                    return (IOperation)Activator.CreateInstance(typeof(ConvertOperation<,>).MakeGenericType(inType, outType), context, expression);
                case ExpressionType.Conditional:
                    return (IOperation)Activator.CreateInstance(typeof(ConditionalOperation<>).MakeGenericType(type), context, (ConditionalExpression)expression);
                case ExpressionType.Subtract:
                case ExpressionType.Divide:
                    return (IOperation)Activator.CreateInstance(typeof(BinaryOperation<>).MakeGenericType(type), context, (BinaryExpression)expression);
            }

            throw new NotSupportedException(string.Format("{0} expression not supported.", expression.NodeType));
        }

        private static IOperation FromConstantExpression(OperationContext context, ConstantExpression expression)
        {
            var query = expression.Value as ObservableQuery;
            if (query != null)
                return (IOperation)Activator.CreateInstance(typeof(ObservableQueryConstantOperation<>).MakeGenericType(query.ElementType), context, expression);
            else
                return (IOperation)Activator.CreateInstance(typeof(ConstantOperation<>).MakeGenericType(Fix(expression.Type)), context, expression);
        }

        /// <summary>
        /// Creates an <see cref="Operation"/> that properly wraps <paramref name="expression"/>, if expression is a Call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static IOperation FromCallExpression(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(Queryable) ||
                expression.Method.DeclaringType == typeof(Enumerable))
                return FromQueryableExpression(context, expression);
            else
                return (IOperation)Activator.CreateInstance(typeof(CallOperation<>).MakeGenericType(Fix(expression.Type)), context, expression);
        }

        /// <summary>
        /// Creates an <see cref="Operation"/> that implements the appropriate <see cref="Queryable"/> method.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static IOperation FromQueryableExpression(OperationContext context, MethodCallExpression expression)
        {
            Type resultItemType, sourceItemType, keyItemType;

            switch (expression.Method.Name)
            {
                case "Concat":
                    resultItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(ConcatOperation<>).MakeGenericType(resultItemType), context, expression);
                case "Intersect":
                    resultItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(IntersectOperation<>).MakeGenericType(resultItemType), context, expression);
                case "Select":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    resultItemType = expression.Method.GetGenericArguments()[1];
                    return (IOperation)Activator.CreateInstance(typeof(SelectOperation<,>).MakeGenericType(sourceItemType, resultItemType), context, expression);
                case "SelectMany":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    resultItemType = expression.Method.GetGenericArguments()[1];
                    return (IOperation)Activator.CreateInstance(typeof(SelectManyOperation<,>).MakeGenericType(sourceItemType, resultItemType), context, expression);
                case "Where":
                    resultItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(WhereOperation<>).MakeGenericType(resultItemType), context, expression);
#if DEBUG
                case "Take":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(TakeOperation<>).MakeGenericType(sourceItemType), context, expression);
                case "Skip":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(SkipOperation<>).MakeGenericType(sourceItemType), context, expression);
#endif
                case "All":
                    return AllOperation.CreateOperation(context, expression);
                case "Any":
                    return AnyOperation.CreateOperation(context, expression);
                case "Count":
                    return CountOperation.CreateOperation(context, expression);
                case "Single":
                    return SingleOperation.CreateOperation(context, expression);
                case "SingleOrDefault":
                    return SingleOrDefaultOperation.CreateOperation(context, expression);
                case "GroupBy":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    keyItemType = expression.Method.GetGenericArguments()[1];
                    return (IOperation)Activator.CreateInstance(typeof(GroupByOperation<,>).MakeGenericType(sourceItemType, keyItemType), context, expression);
                case "Distinct":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(DistinctOperation<>).MakeGenericType(sourceItemType), context, expression);
                case "Sum":
                    return SumOperation.CreateOperation(context, expression);
                case "Average":
                    return AverageOperationFactory.CreateOperation(context, expression);
                case "Cast":
                    return CastOperation.CreateOperation(context, expression);
                case "Min":
                    return MinOperation.CreateOperation(context, expression);
                case "Max":
                    return MaxOperation.CreateOperation(context, expression);
                case "OrderBy":
                    return OrderByOperation.CreateOperation(context, expression);
                case "FirstOrDefault":
                    sourceItemType = expression.Method.GetGenericArguments()[0];
                    return (IOperation)Activator.CreateInstance(typeof(FirstOrDefaultOperation<>).MakeGenericType(sourceItemType), context, expression);
                default:
                    throw new NotSupportedException(expression.Method.Name);
            }
        }

        private static IOperation FromLambdaExpression(OperationContext context, LambdaExpression expression)
        {
            return (IOperation)Activator.CreateInstance(typeof(LambdaOperation<>).MakeGenericType(expression.ReturnType), context, expression);
        }

        internal static IOperation FromValue(object value)
        {
            return (IOperation)Activator.CreateInstance(typeof(ValueOperation<>).MakeGenericType(value.GetType()), value);
        }

    }

}

