using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        protected override void OnSourceCollectionItemsAdded(System.Collections.Generic.IEnumerable<TSource> newItems, int startingIndex)
        {
            SetValue(count += newItems.Count());
        }

        protected override void OnSourceCollectionItemsRemoved(System.Collections.Generic.IEnumerable<TSource> oldItems, int startingIndex)
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

        protected override void OnPredicateCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Add:
                    SetValue(count += args.NewItems.Cast<LambdaOperation<bool>>().Count(i => i.Value));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(count -= args.OldItems.Cast<LambdaOperation<bool>>().Count(i => i.Value));
                    break;
            }
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
