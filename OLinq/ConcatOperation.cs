using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class ConcatOperation<TSource> : EnumerableSource2Operation<TSource, TSource, IEnumerable<TSource>>, IEnumerable<TSource>, INotifyCollectionChanged
    {

        public ConcatOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.Arguments[1])
        {

        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return Source.Concat(Source2).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
