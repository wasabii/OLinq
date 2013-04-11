using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OLinq.Tests
{
    internal class TestObservableCollection<T> : INotifyCollectionChanged, IEnumerable<T>
    {
        private readonly ObservableCollection<T> _observableCollection;
      
        public TestObservableCollection(IEnumerable<T> items = null)
        {
            _observableCollection = new ObservableCollection<T>(items ?? new T[]{});
            _observableCollection.CollectionChanged += (o, e) => this.CollectionChanged(this, e);
        }


        public event NotifyCollectionChangedEventHandler CollectionChanged = (sender, args) => {};
        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            return _observableCollection.GetEnumerator();
        }

        public int EnumerationCount { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(T t)
        {
            _observableCollection.Add(t);
        }

        public T this[int i]
        {
            get { return _observableCollection[i]; }
        }
        public void Insert(int index, object value)
        {
            ((IList)_observableCollection).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)_observableCollection).Remove(value);
        }


        public bool Remove(T item)
        {
            return _observableCollection.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _observableCollection.RemoveAt(index);
        }

    }
}