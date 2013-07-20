using System.Linq.Expressions;

namespace OLinq
{

    class QuoteOperation<T> : UnaryOperation<T,T>
    {

        internal QuoteOperation(OperationContext context, UnaryExpression expression)
            : base(context, expression)
        {
            
        }

        protected override T CoerceValue(T value)
        {
            return value;
        }

    }

}
