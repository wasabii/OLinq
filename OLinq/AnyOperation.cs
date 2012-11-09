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

        static readonly MethodInfo QueryableAnyMethod = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Any")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters().Length == 1)
            .Single();

        static readonly MethodInfo QueryableAnyMethodWithPredicate = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Any")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        static readonly MethodInfo EnumerableAnyMethod = typeof(Enumerable).GetMethods()
            .Where(i => i.Name == "Any")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters().Length == 1)
            .Single();

        static readonly MethodInfo EnumerableAnyMethodWithPredicate = typeof(Enumerable).GetMethods()
            .Where(i => i.Name == "Any")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method == QueryableAnyMethod ||
                method == EnumerableAnyMethod)
                return Operation.CreateMethodCallOperation(typeof(AnyOperation<>), context, expression, 0);
            if (method == QueryableAnyMethodWithPredicate ||
                method == EnumerableAnyMethodWithPredicate)
                return Operation.CreateMethodCallOperation(typeof(AnyOperationWithPredicate<>), context, expression, 0);

            throw new NotImplementedException("Any operation not found.");
        }

    }

    class AnyOperation<TSource> : GroupOperation<TSource, bool>
    {

        public AnyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
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
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, bool>(1))
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
                base.OnPredicateValueChanged(args);
        }

        protected override bool RecalculateValue()
        {
            return Predicates.Any(i => i.Value);
        }

    }

}
