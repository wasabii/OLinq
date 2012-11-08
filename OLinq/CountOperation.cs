using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class CountOperation
    {

        static readonly MethodInfo Method = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Count")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetParameters().Length == 1)
            .Single();

        static readonly MethodInfo CountWithPredicateMethod = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Count")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method == Method)
                return Operation.CreateMethodCallOperation(typeof(CountOperation<>), context, expression, 0);
            else if (expression.Method == CountWithPredicateMethod)
                return Operation.CreateMethodCallOperation(typeof(CountOperationWithPredicate<>), context, expression, 0);
            else
                throw new NotImplementedException("Count operation not found.");
        }

    }

    class CountOperation<TSource> : GroupOperation<TSource, int>
    {

        int count = 0;

        public CountOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
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
            : base(context, expression, expression.GetLambdaArgument<TSource, bool>(1))
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
