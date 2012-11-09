using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Constant operation for referencing a base ObservableQuery instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ObservableQueryConstantOperation<T> : Operation<IEnumerable<T>>
    {

        public ObservableQueryConstantOperation(OperationContext context, ConstantExpression expression)
            : base(context, expression)
        {
            var query = ((ConstantExpression)Expression).Value as ObservableQuery;
            if (query == null)
                throw new Exception("Requires ObservableQuery.");

            SetValue((IEnumerable<T>)query.Enumerable);
        }

    }

}
