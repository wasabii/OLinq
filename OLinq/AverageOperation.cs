using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class AverageOperationFactory
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method.Name != "Average")
                throw new InvalidOperationException("Average operation requires Average method call.");

            if (expression.Method.DeclaringType != typeof(Enumerable) &&
                expression.Method.DeclaringType != typeof(Queryable))
                throw new InvalidOperationException("Requires Enumerable or Queryable method.");

            if (expression.Method.GetGenericArguments().Length == 0 &&
                expression.Method.GetParameters().Length == 1)
            {
                var sourceType = expression.Method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
                var resultType = expression.Method.ReturnType;
                return (IOperation)Activator.CreateInstance(typeof(AverageOperation<,>).MakeGenericType(sourceType, resultType), context, expression);
            }

            if (expression.Method.GetGenericArguments().Length == 1 &&
                expression.Method.GetParameters().Length == 2)
            {
                var sourceType = expression.Method.GetGenericArguments()[0];
                var resultType = expression.Method.ReturnType;
                return (IOperation)Activator.CreateInstance(typeof(AverageWithProjectionOperation<,>).MakeGenericType(sourceType, resultType), context, expression);
            }

            throw new NotSupportedException("Average operation not found.");
        }

    }

    class AverageOperation<TSource, TResult> : GroupOperation<TSource, TResult>
        where TSource : struct
        where TResult : struct
    {

        int count;
        TSource sum;
        TResult average;

        public AverageOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        TSource Sum(IEnumerable<TSource> source)
        {
            return source.Aggregate((i, j) => Add(i, j));
        }

        dynamic Add(dynamic l, dynamic r)
        {
            return l + r;
        }

        dynamic Subtract(dynamic l, dynamic r)
        {
            return l - r;
        }

        dynamic Divide(dynamic l, dynamic r)
        {
            return l / r;
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            SetValue(average = Divide(sum = Add(sum, Sum(newItems)), count += newItems.Count()));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            SetValue(average = Divide(sum = Subtract(sum, Sum(oldItems)), count -= oldItems.Count()));
        }

        protected override TResult RecalculateValue()
        {
            return average = Divide(sum = Sum(Source), count = Source.Count());
        }

    }

    class AverageWithProjectionOperation<TSource, TResult> : GroupOperationWithProjection<TSource, TResult, TResult>
        where TSource : struct
        where TResult : struct
    {

        int count;
        TResult sum;
        TResult average;

        public AverageWithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, TResult>(1))
        {

        }

        TResult Sum(IEnumerable<TResult> source)
        {
            return source.Aggregate((i, j) => Add(i, j));
        }

        dynamic Add(dynamic l, dynamic r)
        {
            return l + r;
        }

        dynamic Subtract(dynamic l, dynamic r)
        {
            return l - r;
        }

        dynamic Divide(dynamic l, dynamic r)
        {
            return l / r;
        }

        protected override void OnProjectionCollectionItemsAdded(IEnumerable<FuncOperation<TResult>> newItems, int startingIndex)
        {
            var newValues = newItems.Select(i => i.Value);
            SetValue(average = Divide(sum = Add(sum, Sum(newValues)), count += newValues.Count()));
        }

        protected override void OnProjectionCollectionItemsRemoved(IEnumerable<FuncOperation<TResult>> oldItems, int startingIndex)
        {
            var oldValues = oldItems.Select(i => i.Value);
            SetValue(average = Divide(sum = Subtract(sum, Sum(oldValues)), count -= oldValues.Count()));
        }

        protected override void OnProjectionValueChanged(FuncValueChangedEventArgs<TSource, TResult> args)
        {
            SetValue(average = Divide(sum = Add(Subtract(sum, args.OldValue), args.NewValue), count));
        }

        protected override TResult RecalculateValue()
        {
            return average = Divide(sum = Sum(Projections.Select(i => i.Value)), count = Source.Count());
        }

    }

}
