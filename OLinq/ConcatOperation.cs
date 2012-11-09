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
            SetValue(this);
        }

        protected override void OnSourceCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.ToList(), -1));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.ToList(), -1));
        }

        protected override void OnSource2CollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSource2CollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.ToList(), -1));
        }

        protected override void OnSource2CollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.ToList(), -1));
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return Source.Concat(Source2).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
