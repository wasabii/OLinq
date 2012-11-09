using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class AnyOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(AnyOperation<>), context, expression, 0);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(AnyOperationWithPredicate<>), context, expression, 0);

            throw new NotImplementedException("Any operation not found.");
        }

    }

    class AnyOperation<TSource> : GroupOperation<TSource, bool>
    {

        public AnyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            if (newItems.Any())
                SetValue(true);
            else
                base.OnSourceCollectionItemsAdded(newItems, startingIndex);
        }

        protected override bool RecalculateValue()
        {
            return Source.Any();
        }

    }

    class AnyOperationWithPredicate<TSource> : GroupOperationWithPredicate<TSource, bool>
    {

        public AnyOperationWithPredicate(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.GetLambdaArgument<TSource, bool>(1))
        {

        }

        protected override void OnPredicateCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Add:
                    // we are currently false, any new true items make us true
                    if (!Value)
                        SetValue(args.NewItems.Cast<LambdaOperation<bool>>().Any(i => i.Value));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ResetValue();
                    break;
            }
        }

        protected override void OnPredicateValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {
            // any value being set to true means we're true
            if (args.NewValue)
                SetValue(true);
            else
                base.OnProjectionValueChanged(args);
        }

        protected override bool RecalculateValue()
        {
            return Predicates.Any(i => i.Value);
        }

    }

}
