using System.Linq.Expressions;

namespace OLinq
{

    class ArrayIndexOperation<TResult> : BinaryOperation<TResult>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public ArrayIndexOperation(OperationContext context, BinaryExpression expression)
            : base(context, expression)
        {

        }

        protected override TResult GetValue()
        {
            // index on null array
            var left = Left.Value;
            if (left == null)
                return default(TResult);

            return base.GetValue();
        }

    }

}
