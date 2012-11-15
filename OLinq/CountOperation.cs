using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class CountOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(CountOperation<>), context, expression, 0);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(CountOperationWithPredicate<>), context, expression, 0);

            throw new NotImplementedException("Count operation not found.");
        }

    }

    class CountOperation<TSource> : GroupOperation<TSource, int>
    {

        int count = 0;

        public CountOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            SetValue(count += newItems.Count());
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            SetValue(count -= oldItems.Count());
        }

        protected override int RecalculateValue()
        {
            return count = Source.Count();
        }

    }

    class CountOperationWithPredicate<TSource> : GroupOperationWithPredicate<TSource, int>
    {

        int count = 0;

        public CountOperationWithPredicate(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, bool>(1))
        {

        }

        protected override void OnPredicateCollectionItemsAdded(IEnumerable<LambdaOperation<bool>> newItems, int startingIndex)
        {
            SetValue(count += newItems.Count(i => i.Value));
        }

        protected override void OnPredicateCollectionItemsRemoved(IEnumerable<LambdaOperation<bool>> oldItems, int startingIndex)
        {
            SetValue(count -= oldItems.Count(i => i.Value));
        }

        protected override void OnPredicateValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {
            if (!args.OldValue && args.NewValue)
                SetValue(++count);
            else if (args.OldValue && !args.NewValue)
                SetValue(--count);
        }

        protected override int RecalculateValue()
        {
            return count = Source.Count();
        }

    }

}
