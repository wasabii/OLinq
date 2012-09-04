using System.Linq.Expressions;

namespace OLinq
{

    class QuoteOperation<T> : UnaryOperation<T>
    {

        internal QuoteOperation(OperationContext context, UnaryExpression expression)
            : base(context, expression)
        {
            
        }

    }

}
