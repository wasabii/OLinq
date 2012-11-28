using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectManyOperation<TSource, TResult> : EnumerableSourceWithLambdaOperation<TSource, IEnumerable<TResult>, IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        List<IEnumerable> items = new List<IEnumerable>();

        public SelectManyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, IEnumerable<TResult>>(1))
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionReset()
        {
            // unsubscribe from all items
            foreach (var item in items)
                UnsubscribeItem(item);

            // reset all items
            items.Clear();
            items.AddRange(Lambdas.Select(i => i.Value));

            // subscribe to all items
            foreach (var item in items)
                SubscribeItem(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnLambdaCollectionItemsAdded(IEnumerable<LambdaOperation<IEnumerable<TResult>>> newItems, int startingIndex)
        {
            // subscribe to new items
            foreach (var newItem in newItems.Select(i => i.Value))
            {
                items.Add(newItem);
                SubscribeItem(newItem);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems.SelectMany(i => i.Value).ToList()));
        }

        protected override void OnLambdaCollectionItemsRemoved(IEnumerable<LambdaOperation<IEnumerable<TResult>>> oldItems, int startingIndex)
        {
            // unsubscribe from oldl items
            foreach (var oldItem in oldItems.Select(i => i.Value))
            {
                UnsubscribeItem(oldItem);
                items.Remove(oldItem);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems.SelectMany(i => i.Value).ToList()));
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, IEnumerable<TResult>> args)
        {
            // unsubscribe from old value
            UnsubscribeItem(args.OldValue);
            items.Remove(args.OldValue);

            // subscribe to new value
            items.Add(args.NewValue);
            SubscribeItem(args.NewValue);

            var oldValues = args.OldValue.Except(args.NewValue).ToList();
            var newValues = args.NewValue.Except(args.OldValue).ToList();
            if (oldValues.Count == 0 && newValues.Count >= 1)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newValues));
            else if (oldValues.Count >= 1 && newValues.Count == 0)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValues));
            else if (oldValues.Count >= 1 && newValues.Count >= 1)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldValues, newValues));
        }

        void SubscribeItem(IEnumerable item)
        {
            var c = item as INotifyCollectionChanged;
            if (c != null)
                c.CollectionChanged += item_CollectionChanged;
        }

        void UnsubscribeItem(IEnumerable item)
        {
            var c = item as INotifyCollectionChanged;
            if (c != null)
                c.CollectionChanged -= item_CollectionChanged;
        }

        void item_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems.Cast<TResult>().ToList()));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems.Cast<TResult>().ToList()));
                    break;
            }
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Lambdas.SelectMany(i => i.Value).GetEnumerator();
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

        public override void Dispose()
        {
            base.Dispose();

            // unsubscribe from items
            foreach (var item in items)
                UnsubscribeItem(item);
            items.Clear();
        }

    }

}
