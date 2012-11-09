using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class AverageOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(Enumerable) &&
                expression.Method.DeclaringType != typeof(Queryable))
                throw new InvalidOperationException("Requires Enumerable or Queryable method.");

            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 0 &&
                method.GetParameters().Length == 1 &&
                method.ReturnType == typeof(int))
                return Operation.CreateMethodCallOperation(typeof(AverageInt32Operation), context, expression);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2 &&
                method.ReturnType == typeof(int))
                return Operation.CreateMethodCallOperation(typeof(AverageInt32WithProjectionOperation<>), context, expression, 0);
            if (method.GetGenericArguments().Length == 0 &&
                method.GetParameters().Length == 1 &&
                method.ReturnType == typeof(float))
                return Operation.CreateMethodCallOperation(typeof(AverageSingleOperation), context, expression);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2 &&
                method.ReturnType == typeof(float))
                return Operation.CreateMethodCallOperation(typeof(AverageSingleWithProjectionOperation<>), context, expression, 0);

            throw new NotSupportedException("Average operation not found.");
        }

    }

    class AverageInt32Operation : GroupOperation<int, double>
    {

        int count = 0;
        int sum = 0;
        double average = 0;

        public AverageInt32Operation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<int> newItems, int startingIndex)
        {
            SetValue(average = (sum += newItems.Sum()) / (count += newItems.Count()));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<int> oldItems, int startingIndex)
        {
            SetValue(average = (sum -= oldItems.Sum()) / (count -= oldItems.Count()));
        }

        protected override double RecalculateValue()
        {
            return average = (sum = Source.Sum()) / (count = Source.Count());
        }

    }

    class AverageInt32WithProjectionOperation<TSource> : GroupOperationWithProjection<TSource, int, double>
    {

        int count = 0;
        int sum = 0;
        double average = 0;

        public AverageInt32WithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, int>(1))
        {

        }

        protected override void OnProjectionCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Add:
                    SetValue(average = (sum += args.NewItems.Cast<LambdaOperation<int>>().Sum(i => i.Value)) / (count += args.NewItems.Cast<LambdaOperation<int>>().Count()));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(average = (sum -= args.OldItems.Cast<LambdaOperation<int>>().Sum(i => i.Value)) / (count -= args.OldItems.Cast<LambdaOperation<int>>().Count()));
                    break;
            }
        }

        protected override void OnProjectionValueChanged(LambdaValueChangedEventArgs<TSource, int> args)
        {
            SetValue(average = (sum = sum - args.OldValue + args.NewValue) / count);
        }

        protected override double RecalculateValue()
        {
            return average = (sum = Projections.Sum(i => i.Value)) / (count = Projections.Count());
        }

    }

    class AverageSingleOperation : GroupOperation<float, float>
    {

        int count = 0;
        float sum = 0;
        float average = 0;

        public AverageSingleOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<float> newItems, int startingIndex)
        {
            SetValue(average = (sum += newItems.Sum()) / (count += newItems.Count()));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<float> oldItems, int startingIndex)
        {
            SetValue(average = (sum -= oldItems.Sum()) / (count -= oldItems.Count()));
        }

        protected override float RecalculateValue()
        {
            return average = (sum = Source.Sum()) / (count = Source.Count());
        }

    }

    class AverageSingleWithProjectionOperation<TSource> : GroupOperationWithProjection<TSource, float, float>
    {

        int count = 0;
        float sum = 0;
        float average = 0;

        public AverageSingleWithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, float>(1))
        {

        }

        protected override void OnProjectionCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Add:
                    SetValue(average = (sum += args.NewItems.Cast<LambdaOperation<float>>().Sum(i => i.Value)) / (count += args.NewItems.Cast<LambdaOperation<float>>().Count()));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(average = (sum -= args.OldItems.Cast<LambdaOperation<float>>().Sum(i => i.Value)) / (count -= args.OldItems.Cast<LambdaOperation<float>>().Count()));
                    break;
            }
        }

        protected override void OnProjectionValueChanged(LambdaValueChangedEventArgs<TSource, float> args)
        {
            SetValue(average = (sum = sum - args.OldValue + args.NewValue) / count);
        }

        protected override float RecalculateValue()
        {
            return average = (sum = Projections.Sum(i => i.Value)) / (count = Projections.Count());
        }

    }

}
