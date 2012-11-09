using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class SingleOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(SingleOperation<>), context, expression, 0);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(SingleWithPredicateOperation<>), context, expression, 0);

            throw new NotImplementedException("Single operation not found.");
        }

    }

    class SingleOperation<TSource> : SingleOrDefaultOperation<TSource>
    {

        public SingleOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override TSource RecalculateValue()
        {
            return Source.Single();
        }

    }

    class SingleWithPredicateOperation<TSource> : SingleOrDefaultWithPredicateOperation<TSource>
    {

        public SingleWithPredicateOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override TSource RecalculateValue()
        {
            return Source.Single();
        }

    }

}
