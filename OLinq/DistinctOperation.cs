using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class DistinctOperation<TElement> : EnumerableSourceOperation<TElement, IEnumerable<TElement>>, IEnumerable<TElement>, INotifyCollectionChanged
    {

        Dictionary<TElement, int> counts = new Dictionary<TElement, int>();

        public DistinctOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {
            SetValue(this);
        }

        protected override void OnSourceCollectionReset()
        {
            var oldItems = counts.Keys.ToList();
            var newItems = Source;

            var newTrack = new List<TElement>();
            var oldTrack = new List<TElement>();

            foreach (var item in newItems)
                if (counts.GetOrDefault(item) == 0)
                {
                    // item did not exist before, we've just added it
                    counts[item] = 1;
                    newTrack.Add(item);
                }
                else
                    // item already existed, increase it's count
                    counts[item]++;

            foreach (var item in oldItems)
                if (counts.GetOrDefault(item) == 0)
                    // value never existed in the first place
                    continue;
                else if (counts.GetOrDefault(item) == 1)
                {
                    // item had one count, removed for good
                    counts.Remove(item);
                    oldTrack.Add(item);
                }
                else
                    // decrease count
                    counts[item]--;

            if (newTrack.Count > 0 && oldTrack.Count > 0)
                // both new and old items exist
                NotifyCollectionChangedUtil.RaiseReplaceEvent<TElement>(OnCollectionChanged, oldTrack, newTrack);
            else if (newTrack.Count > 0 && oldTrack.Count == 0)
                // only new items were added
                NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(OnCollectionChanged, newTrack);
            else if (newTrack.Count == 0 && oldTrack.Count > 0)
                // only items were removed
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(OnCollectionChanged, oldTrack);
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TElement> newItems, int startingIndex)
        {
            var newTrack = new List<TElement>();

            foreach (var item in newItems)
                if (counts.GetOrDefault(item) == 0)
                {
                    // item did not exist, added
                    counts.GetOrCreate(item, i => 1);
                    newTrack.Add(item);
                }
                else
                    // item already existed, increase count
                    counts[item]++;

            if (newTrack.Count > 0)
                NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(OnCollectionChanged, newTrack);
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TElement> oldItems, int startingIndex)
        {
            var oldTrack = new List<TElement>();

            foreach (var item in oldItems)
                if (counts.GetOrDefault(item) == 0)
                    // item did not exist, ignore
                    continue;
                else if (counts.GetOrDefault(item) == 1)
                {
                    // item has been removed
                    counts.Remove(item);
                    oldTrack.Add(item);
                }
                else
                    // decrease count
                    counts[item]--;

            if (oldTrack.Count > 0)
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(OnCollectionChanged, oldTrack);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return counts.Keys.GetEnumerator();
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
