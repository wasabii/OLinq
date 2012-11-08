using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class SingleOrDefaultOperation
    {

        static readonly MethodInfo Method = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "SingleOrDefault")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetParameters().Length == 1)
            .Single();

        static readonly MethodInfo WithPredicateMethod = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "SingleOrDefault")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method == Method)
                return Operation.CreateMethodCallOperation(typeof(SingleOrDefaultOperation<>), context, expression, 0);
            else if (expression.Method == WithPredicateMethod)
                return Operation.CreateMethodCallOperation(typeof(SingleOrDefaultWithPredicateOperation<>), context, expression, 0);
            else
                throw new NotImplementedException("SingleOrDefault operation not found.");
        }

    }

    class SingleOrDefaultOperation<TSource> : GroupOperation<TSource, TSource>
    {

        public SingleOrDefaultOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
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
            : base(context, expression, expression.GetLambdaArgument<TSource, bool>(1))
        {

        }

        protected override TSource RecalculateValue()
        {
            var l = Predicates.SingleOrDefault(i => i.Value);
            return l != null ? Predicates[l] : default(TSource);
        }

    }

}
