using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace OLinq
{

    public sealed class ObservableBuffer<TElement> : IEnumerable<TElement>, INotifyCollectionChanged
    {

        ObservableView<TElement> view;
        ObservableCollection<TElement> buffer = new ObservableCollection<TElement>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="view"></param>
        internal ObservableBuffer(ObservableView<TElement> view)
        {
            this.view = view;

            // subscribe to notifications from both sides
            view.CollectionChanged += view_CollectionChanged;
            buffer.CollectionChanged += buffer_CollectionChanged;

            // reset buffer items
            Reset();
        }

        /// <summary>
        /// Invoked when the view collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void view_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    // add new items
                    foreach (TElement item in args.NewItems)
                        buffer.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // remove old items
                    foreach (TElement item in args.OldItems)
                        buffer.Remove(item);
                    break;
            }
        }

        /// <summary>
        /// Invoked when the buffer collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void buffer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, args.NewItems, args.NewStartingIndex, args.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.NewItems, args.OldItems, args.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems, args.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems, args.OldStartingIndex));
                    break;
            }
        }

        /// <summary>
        /// Resets the buffered collection based on the underlying list.
        /// </summary>
        void Reset()
        {
            // remove obsolete items
            var oldItems = buffer.Except(view).ToList();
            foreach (var item in oldItems)
                buffer.Remove(item);

            // add missing items
            var newItems = view.Except(buffer).ToList();
            foreach (var item in newItems)
                buffer.Add(item);
        }

        /// <summary>
        /// Gets the associated view.
        /// </summary>
        public ObservableView<TElement> View
        {
            get { return view; }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
