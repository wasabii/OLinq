using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace OLinq
{

    static class NotifyCollectionChangedUtil
    {

        public static void RaiseAddEvent<T>(Action<NotifyCollectionChangedEventArgs> raise, IEnumerable<T> newItems)
        {
#if !SILVERLIGHT
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.ToList()));
#else
            foreach (var item in newItems)
                raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, -1));
#endif
        }

        public static void RaiseAddEvent<T>(Action<NotifyCollectionChangedEventArgs> raise, T newItem)
        {
#if !SILVERLIGHT
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, -1));
#else
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, -1));
#endif
        }

        public static void RaiseRemoveEvent<T>(Action<NotifyCollectionChangedEventArgs> raise, IEnumerable<T> oldItems)
        {
#if !SILVERLIGHT
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.ToList()));
#else
            foreach (var item in oldItems)
                raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, -1));
#endif
        }

        public static void RaiseRemoveEvent<T>(Action<NotifyCollectionChangedEventArgs> raise, T oldItem)
        {
#if !SILVERLIGHT
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, -1));
#else
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, -1));
#endif
        }

        public static void RaiseReplaceEvent<T>(Action<NotifyCollectionChangedEventArgs> raise, IEnumerable<T> oldItems, IEnumerable<T> newItems)
        {
#if !SILVERLIGHT
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems.ToList(), oldItems.ToList()));
#else
            var items = newItems
                .Zip(oldItems, (i, j) => new { Old = i, New = j });
            foreach (var i in items)
                raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, i.New, i.Old, -1));
#endif
        }

        public static void RaiseReplaceEvent<T>(Action<NotifyCollectionChangedEventArgs> raise, T oldItem, T newItem)
        {
#if !SILVERLIGHT
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new [] { newItem }, new [] { oldItem }));
#else
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, -1));
#endif
        }

        public static void RaiseResetEvent<T>(Action<NotifyCollectionChangedEventArgs> raise)
        {
            raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

    }

}
