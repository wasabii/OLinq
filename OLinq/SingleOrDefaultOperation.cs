using System;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SingleOrDefaultOperation<TSource> : GroupOperation<TSource, bool, TSource>
    {

        public SingleOrDefaultOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, true)
        {

        }

        protected override TSource RecalculateValue()
        {
            var l = Lambdas.SingleOrDefault(i => i.Value);
            return l != null ? Lambdas[l] : default(TSource);
        }

    }

}
