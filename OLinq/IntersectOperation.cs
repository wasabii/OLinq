using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class IntersectOperation<TSource> : EnumerableSource2Operation<TSource, TSource, IEnumerable<TSource>>, IEnumerable<TSource>, INotifyCollectionChanged
    {

        HashSet<TSource> sourceLookup = new HashSet<TSource>();
        HashSet<TSource> source2Lookup = new HashSet<TSource>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public IntersectOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.Arguments[1])
        {
            SetValue(this);
        }

        protected override void OnSourceCollectionReset()
        {
            sourceLookup = new HashSet<TSource>(Source);
            OnSourceCollectionItemsAdded(Source, -1);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            foreach (var newItem in newItems)
                sourceLookup.Add(newItem);

            var matched = newItems.Where(i => source2Lookup.Contains(i)).ToList();
            if (matched.Any())
                NotifyCollectionChangedUtil.RaiseAddEvent<TSource>(OnCollectionChanged, matched);
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            foreach (var oldItem in oldItems)
                sourceLookup.Remove(oldItem);

            var matched = oldItems.Where(i => source2Lookup.Contains(i)).ToList();
            if (matched.Any())
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TSource>(OnCollectionChanged, matched);
        }

        protected override void OnSource2CollectionReset()
        {
            source2Lookup = new HashSet<TSource>(Source2);
            OnSource2CollectionItemsAdded(Source2, -1);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSource2CollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            foreach (var newItem in newItems)
                source2Lookup.Add(newItem);

            var matched = newItems.Where(i => sourceLookup.Contains(i)).ToList();
            if (matched.Any())
                NotifyCollectionChangedUtil.RaiseAddEvent<TSource>(OnCollectionChanged, matched);
        }

        protected override void OnSource2CollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            foreach (var oldItem in oldItems)
                source2Lookup.Remove(oldItem);

            var matched = oldItems.Where(i => sourceLookup.Contains(i)).ToList();
            if (matched.Any())
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TSource>(OnCollectionChanged, matched);
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return sourceLookup.Where(i => source2Lookup.Contains(i)).GetEnumerator();
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