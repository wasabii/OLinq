using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class SumOperation
    {

        static readonly MethodInfo Int32Method = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Sum")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.ReturnType == typeof(int))
            .Where(i => i.GetParameters().Length == 1)
            .Single();

        static readonly MethodInfo Int32WithPredicateMethod = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Sum")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.ReturnType == typeof(int))
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method == Int32Method)
                return Operation.CreateMethodCallOperation(typeof(SumInt32Operation), context, expression);
            else if (expression.Method == Int32WithPredicateMethod)
                return Operation.CreateMethodCallOperation(typeof(SumInt32WithProjectionOperation<>), context, expression, 0);
            else
                throw new NotSupportedException("Sum operation not found.");
        }

    }

    class SumInt32Operation : GroupOperation<int, int>
    {

        int sum = 0;

        public SumInt32Operation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<int> newItems, int startingIndex)
        {
            SetValue(sum += newItems.Sum());
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<int> oldItems, int startingIndex)
        {
            SetValue(sum -= oldItems.Sum());
        }

        protected override int RecalculateValue()
        {
            return sum = Source.Sum();
        }

    }

    class SumInt32WithProjectionOperation<TSource> : GroupOperationWithProjection<TSource, int, int>
    {

        int sum = 0;

        public SumInt32WithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.GetLambdaArgument<TSource, int>(1))
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
                    SetValue(sum += args.NewItems.Cast<LambdaOperation<int>>().Sum(i => i.Value));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(sum -= args.OldItems.Cast<LambdaOperation<int>>().Sum(i => i.Value));
                    break;
            }
        }

        protected override void OnProjectionValueChanged(LambdaValueChangedEventArgs<TSource, int> args)
        {
            SetValue(sum = sum - args.OldValue + args.NewValue);
        }

        protected override int RecalculateValue()
        {
            return sum = Projections.Sum(i => i.Value);
        }

    }

}
