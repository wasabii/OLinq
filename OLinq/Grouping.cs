using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace OLinq
{

    class Grouping<TKey, TElement> : IGrouping<TKey, TElement>, INotifyCollectionChanged
    {

        ObservableCollection<TElement> items = new ObservableCollection<TElement>();

        public Grouping(TKey key)
        {
            Key = key;
            items.CollectionChanged += items_CollectionChanged;
        }

        void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RaiseCollectionChanged(args);
        }

        public TKey Key { get; private set; }

        internal void Add(TElement item)
        {
            items.Add(item);
        }

        internal void Remove(TElement item)
        {
            items.Remove(item);
        }

        internal int Count
        {
            get { return items.Count; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
