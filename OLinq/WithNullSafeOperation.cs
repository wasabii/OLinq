using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Provides a context option to the graph which enables null-safe operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class WithNullSafeOperation<T> :
        OptionOperation<IQueryable<T>>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public WithNullSafeOperation(OperationContext context, MethodCallExpression expression)
            : base(new OperationContext(context, nullSafe: true), expression.Arguments[0])
        {

        }

    }

}
