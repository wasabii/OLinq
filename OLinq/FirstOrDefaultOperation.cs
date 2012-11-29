using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class FirstOrDefaultOperation<TSource> : GroupOperation<TSource, TSource>
    {

        public FirstOrDefaultOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {
            
        }

        protected override TSource RecalculateValue()
        {
            return Source.FirstOrDefault();
        }

    }

}
