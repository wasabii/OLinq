using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class AllOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
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
                    if (Value)
                        SetValue(args.NewItems.Cast<LambdaOperation<bool>>().All(i => i.Value));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ResetValue();
                    break;
            }
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
