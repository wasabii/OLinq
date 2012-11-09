using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class MinOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(MinOperation), context, expression);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(MinWithProjectionOperation<,>), context, expression, 0, 1);

            throw new NotSupportedException("Min operation not found.");
        }

    }

    class MinOperation<TSource> : GroupOperation<TSource, TSource>
    {

        TSource min = default(TSource);

        public MinOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override TSource RecalculateValue()
        {
            return min = Source.Min();
        }

    }

    class MinWithProjectionOperation<TSource, TResult> : GroupOperationWithProjection<TSource, TResult, TResult>
    {

        TResult min = default(TResult);

        public MinWithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, TResult>(1))
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
                    ResetValue();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ResetValue();
                    break;
            }
        }

        protected override TResult RecalculateValue()
        {
            return min = Projections.Min(i => i.Value);
        }

    }

}
