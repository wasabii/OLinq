using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SingleOperation<TSource> : SingleOrDefaultOperation<TSource>
    {

        public SingleOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override TSource RecalculateValue()
        {
            var l = Lambdas.Single(i => i.Value);
            return l != null ? Lambdas[l] : default(TSource);
        }

    }

}
