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

        static readonly MethodInfo[] QueryableAverageMethods = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Average")
            .ToArray();

        static readonly MethodInfo[] EnumerableAverageMethods = typeof(Enumerable).GetMethods()
            .Where(i => i.Name == "Average")
            .ToArray();

        static readonly MethodInfo QueryableAverageInt32Method = QueryableAverageMethods
            .Where(i => !i.IsGenericMethodDefinition)
            .Where(i => i.ReturnType == typeof(double))
            .Where(i => i.GetParameters().Length == 1)
            .Where(i => i.GetParameters()[0].ParameterType == typeof(IQueryable<int>))
            .Single();

        static readonly MethodInfo QueryableAverageInt32WithProjectionMethod = QueryableAverageMethods
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.ReturnType == typeof(double))
            .Where(i => i.GetParameters().Length == 2)
            .Where(i => i.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(int))
            .Single();

        static readonly MethodInfo QueryableAverageSingleMethod = QueryableAverageMethods
            .Where(i => !i.IsGenericMethodDefinition)
            .Where(i => i.ReturnType == typeof(float))
            .Where(i => i.GetParameters().Length == 1)
            .Where(i => i.GetParameters()[0].ParameterType == typeof(IQueryable<float>))
            .Single();

        static readonly MethodInfo QueryableAverageSingleWithProjectionMethod = QueryableAverageMethods
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.ReturnType == typeof(float))
            .Where(i => i.GetParameters().Length == 2)
            .Where(i => i.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(float))
            .Single();

        static readonly MethodInfo EnumerableAverageInt32Method = EnumerableAverageMethods
            .Where(i => !i.IsGenericMethodDefinition)
            .Where(i => i.ReturnType == typeof(double))
            .Where(i => i.GetParameters().Length == 1)
            .Where(i => i.GetParameters()[0].ParameterType == typeof(IEnumerable<int>))
            .Single();

        static readonly MethodInfo EnumerableAverageInt32WithProjectionMethod = EnumerableAverageMethods
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.ReturnType == typeof(double))
            .Where(i => i.GetParameters().Length == 2)
            .Where(i => i.GetParameters()[0].ParameterType == typeof(IEnumerable<int>))
            .Single();

        static readonly MethodInfo EnumerableAverageSingleWithProjectionMethod = EnumerableAverageMethods
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.ReturnType == typeof(double))
            .Where(i => i.GetParameters().Length == 2)
            .Where(i => i.GetParameters()[1].ParameterType.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters()[1].ParameterType.GetGenericArguments()[0] == typeof(float))
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(Enumerable) &&
                expression.Method.DeclaringType != typeof(Queryable))
                throw new InvalidOperationException("Requires Enumerable or Queryable method.");

            var method = expression.Method.GetGenericMethodDefinition();
            if (method == QueryableAverageInt32Method)
                return Operation.CreateMethodCallOperation(typeof(AverageInt32Operation), context, expression);
            if (method == QueryableAverageInt32WithProjectionMethod)
                return Operation.CreateMethodCallOperation(typeof(AverageInt32WithProjectionOperation<>), context, expression, 0);
            if (method == QueryableAverageSingleMethod)
                return Operation.CreateMethodCallOperation(typeof(AverageSingleOperation), context, expression);
            if (method == QueryableAverageSingleWithProjectionMethod)
                return Operation.CreateMethodCallOperation(typeof(AverageSingleWithProjectionOperation<>), context, expression);

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
            if (method.GetGenericArguments().Length == 0 &&
                method.GetParameters().Length == 1 &&
                method.ReturnType == typeof(double))
                return Operation.CreateMethodCallOperation(typeof(AverageDoubleOperation), context, expression);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2 &&
                method.ReturnType == typeof(double))
                return Operation.CreateMethodCallOperation(typeof(AverageDoubleWithProjectionOperation<>), context, expression, 0);

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

    class AverageDoubleOperation : GroupOperation<double, double>
    {

        int count = 0;
        double sum = 0;
        double average = 0;

        public AverageDoubleOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<double> newItems, int startingIndex)
        {
            SetValue(average = (sum += newItems.Sum()) / (count += newItems.Count()));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<double> oldItems, int startingIndex)
        {
            SetValue(average = (sum -= oldItems.Sum()) / (count -= oldItems.Count()));
        }

        protected override double RecalculateValue()
        {
            return average = (sum = Source.Sum()) / (count = Source.Count());
        }

    }

    class AverageDoubleWithProjectionOperation<TSource> : GroupOperationWithProjection<TSource, double, double>
    {

        int count = 0;
        double sum = 0;
        double average = 0;

        public AverageDoubleWithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, double>(1))
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
                    SetValue(average = (sum += args.NewItems.Cast<LambdaOperation<double>>().Sum(i => i.Value)) / (count += args.NewItems.Cast<LambdaOperation<double>>().Count()));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(average = (sum -= args.OldItems.Cast<LambdaOperation<double>>().Sum(i => i.Value)) / (count -= args.OldItems.Cast<LambdaOperation<double>>().Count()));
                    break;
            }
        }

        protected override void OnProjectionValueChanged(LambdaValueChangedEventArgs<TSource, double> args)
        {
            SetValue(average = (sum = sum - args.OldValue + args.NewValue) / count);
        }

        protected override double RecalculateValue()
        {
            return average = (sum = Projections.Sum(i => i.Value)) / (count = Projections.Count());
        }

    }

}
