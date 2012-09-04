using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OLinq
{

    public class ObservableQueryView<T> : IEnumerable<T>, INotifyCollectionChanged, IDisposable
    {

        internal ObservableQueryView(ObservableQuery<T> query)
        {
            Query = query;
            Query.CollectionChanged += Query_CollectionChanged;
        }

        public ObservableQuery<T> Query { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return Query.GetEnumerator();
        }

        void Query_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Query.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }

}
