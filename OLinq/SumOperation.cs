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

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(SumInt32Operation), context, expression);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(SumInt32WithProjectionOperation<>), context, expression, 0);
            else
                throw new NotSupportedException("Sum operation not found.");
        }

    }

    class SumInt32Operation : GroupOperation<int, int>
    {

        int sum = 0;

        public SumInt32Operation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
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
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, int>(1))
        {

        }

        protected override void OnProjectionCollectionItemsAdded(IEnumerable<LambdaOperation<int>> newItems, int startingIndex)
        {
            SetValue(sum += newItems.Sum(i => i.Value));
        }

        protected override void OnProjectionCollectionItemsRemoved(IEnumerable<LambdaOperation<int>> oldItems, int startingIndex)
        {
            SetValue(sum -= oldItems.Sum(i => i.Value));
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
