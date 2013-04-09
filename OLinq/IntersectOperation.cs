using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{
    class IntersectOperation<TSource> : EnumerableSource2Operation<TSource, TSource, IEnumerable<TSource>>, IEnumerable<TSource>, INotifyCollectionChanged
    {

        public IntersectOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.Arguments[1])
        {
            SetValue(this);
        }

        protected override void OnSourceCollectionReset()
        {
            cache = new HashSet<TSource>();
            sourceLookup = new HashSet<TSource>(Source);
            OnSourceCollectionItemsAdded(Source, -1);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            foreach (var newItem in newItems)
            {
                sourceLookup.Add(newItem);
            }

            var matched = newItems.Where(i => source2Lookup.Contains(i)).ToList();
            if (matched.Any())
            {
                matched.ForEach(i => cache.Add(i));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, matched));
            }
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            oldItems = oldItems.ToList();
            foreach (var oldItem in oldItems)
            {
                sourceLookup.Remove(oldItem);
            }

            var matched = oldItems.Where(i => source2Lookup.Contains(i)).ToList();
            if (matched.Any())
            {
                matched.ForEach(i => cache.Remove(i));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, matched));
            }
        }

        protected override void OnSource2CollectionReset()
        {
            cache = new HashSet<TSource>();
            source2Lookup = new HashSet<TSource>(Source2);
            OnSource2CollectionItemsAdded(Source2, -1);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSource2CollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {
            foreach (var newItem in newItems)
            {
                source2Lookup.Add(newItem);
            }

            var matched = newItems.Where(i => sourceLookup.Contains(i)).ToList();
            if (matched.Any())
            {
                matched.ForEach(i => cache.Add(i));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, matched));
            }
        }

        protected override void OnSource2CollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {
            oldItems = oldItems.ToList();
            foreach (var oldItem in oldItems)
            {
                source2Lookup.Remove(oldItem);
            }

            var matched = oldItems.Where(i => sourceLookup.Contains(i)).ToList();
            if (matched.Any())
            {
                matched.ForEach(i => cache.Remove(i));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, matched));
            }
        }


        public IEnumerator<TSource> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        private HashSet<TSource> cache = new HashSet<TSource>();
        HashSet<TSource> sourceLookup = new HashSet<TSource>();
        HashSet<TSource> source2Lookup = new HashSet<TSource>();
        
        IEnumerable<TSource> Enumerate()
        {
            foreach (var item in cache)
            {
                yield return item;
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