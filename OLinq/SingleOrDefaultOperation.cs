using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class SingleOrDefaultOperation
    {
public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            var method = expression.Method.GetGenericMethodDefinition();
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 1)
                return Operation.CreateMethodCallOperation(typeof(SingleOrDefaultOperation<>), context, expression, 0);
            if (method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2)
                return Operation.CreateMethodCallOperation(typeof(SingleOrDefaultWithPredicateOperation<>), context, expression, 0);

            throw new NotImplementedException("SingleOrDefault operation not found.");
        }

    }

    class SingleOrDefaultOperation<TSource> : GroupOperation<TSource, TSource>
    {

        public SingleOrDefaultOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {

        }

        protected override TSource RecalculateValue()
        {
            return Source.SingleOrDefault();
        }

    }

    class SingleOrDefaultWithPredicateOperation<TSource> : GroupOperationWithPredicate<TSource, TSource>
    {

        public SingleOrDefaultWithPredicateOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, bool>(1))
        {

        }

        protected override TSource RecalculateValue()
        {
            var l = Predicates.SingleOrDefault(i => i.Value);
            return l != null ? Predicates[l] : default(TSource);
        }

    }

}
