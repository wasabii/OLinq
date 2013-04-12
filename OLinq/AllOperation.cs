using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class AllOperation
    {

        static readonly MethodInfo QueryableAllMethod = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "All")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        static readonly MethodInfo EnumerableAllMethod = typeof(Enumerable).GetMethods()
            .Where(i => i.Name == "All")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetGenericArguments().Length == 1)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method == QueryableAllMethod)
                return Operation.CreateMethodCallOperation(typeof(AllOperation<>), context, expression, 0);
            if (method == EnumerableAllMethod)
                return Operation.CreateMethodCallOperation(typeof(AllOperation<>), context, expression, 0);

            throw new NotImplementedException("All operation not found.");
        }

    }

    class AllOperation<TSource> : GroupOperationWithPredicate<TSource, bool>
    {

        public AllOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, bool>(1))
        {

        }

        protected override void OnPredicateCollectionItemsAdded(IEnumerable<LambdaOperation<bool>> newItems, int startingIndex)
        {
            // we are currently true, any new false items make us false
            if (Value)
                SetValue(newItems.All(i => i.Value));
        }

        protected override void OnPredicateValueChanged(LambdaValueChangedEventArgs<TSource, bool> args)
        {
            if (!args.NewValue)
                SetValue(false);
            else
                ResetValue();
        }

        protected override bool RecalculateValue()
        {
            return Predicates.All(i => i.Value);
        }

    }

}
