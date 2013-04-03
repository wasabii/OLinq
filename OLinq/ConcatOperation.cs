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
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.ToList(), startingIndex));
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.ToList(), startingIndex));
        }

        protected override void OnSource2CollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSource2CollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.ToList(), startingIndex + SourceCount));
        }

        protected override void OnSource2CollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.ToList(), startingIndex + SourceCount));
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }
        public int SourceCount { get; set; }
        public int Source2Count { get; set; }
        IEnumerable<TSource> Enumerate()
        {
            SourceCount = 0;
            foreach (var item in Source)
            {
                yield return item;
                SourceCount++;
            }
            foreach (var item in Source2)
            {
                yield return item;
                Source2Count++;
            }
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
