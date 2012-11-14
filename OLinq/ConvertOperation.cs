using System;
using System.Linq.Expressions;

namespace OLinq
{

    class ConvertOperation<TIn, TOut> : UnaryOperation<TIn, TOut>
    {

        public ConvertOperation(OperationContext context, UnaryExpression expression)
            : base(context, expression)
        {

        }

        protected override TOut CoerceValue(TIn value)
        {
            return Coerce(value);
        }

        TOut Coerce(dynamic arg)
        {
            return (TOut)arg;
        }

    }

}
