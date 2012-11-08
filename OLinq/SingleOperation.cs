using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    static class SingleOperation
    {

        static readonly MethodInfo Method = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Single")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetParameters().Length == 1)
            .Single();

        static readonly MethodInfo WithPredicateMethod = typeof(Queryable).GetMethods()
            .Where(i => i.Name == "Single")
            .Where(i => i.IsGenericMethodDefinition)
            .Where(i => i.GetParameters().Length == 2)
            .Single();

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            if (expression.Method == Method)
                return Operation.CreateMethodCallOperation(typeof(SingleOperation<>), context, expression, 0);
            else if (expression.Method == WithPredicateMethod)
                return Operation.CreateMethodCallOperation(typeof(SingleWithPredicateOperation<>), context, expression, 0);
            else
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
