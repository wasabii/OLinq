using System;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class MaxOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(MaxOperation<>), context, expression, 0);
            if (method.GetGenericArguments().Length == 2 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(MaxWithProjectionOperation<,>), context, expression, 0, 1);

            throw new NotSupportedException("Max operation not found.");
        }

    }

    class MaxOperation<TSource> : GroupOperation<TSource, TSource>
    {

        TSource max = default(TSource);

        public MaxOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override TSource RecalculateValue()
        {
            return max = Source.Max();
        }

    }

    class MaxWithProjectionOperation<TSource, TResult> : GroupOperationWithProjection<TSource, TResult, TResult>
    {

        TResult max = default(TResult);

        public MaxWithProjectionOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, TResult>(1))
        {

        }

        protected override TResult RecalculateValue()
        {
            return max = Projections.Select(i => i.Value).DefaultIfEmpty().Max();
        }

    }

}
