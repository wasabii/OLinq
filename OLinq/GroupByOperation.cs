using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class GroupByOperation<TElement, TKey> : EnumerableSourceWithLambdaOperation<TElement, TKey, IEnumerable<IGrouping<TKey, TElement>>>, IEnumerable<IGrouping<TKey, TElement>>, INotifyCollectionChanged
    {

        Dictionary<TKey, Grouping<TKey, TElement>> groups = new Dictionary<TKey, Grouping<TKey, TElement>>();

        public GroupByOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TElement, TKey>(1))
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    var newItems = args.NewItems.Cast<LambdaOperation<TKey>>().Select(i => Lambdas[i]);
                    foreach (var item in newItems)
                        AddItem(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItems = args.OldItems.Cast<LambdaOperation<TKey>>().Select(i => Lambdas[i]);
                    foreach (var item in oldItems)
                        RemoveItem(item);
                    break;
            }
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TElement, TKey> args)
        {
            RemoveItemWithKey(args.Item, args.OldValue);
            AddItem(args.Item);
        }

        void Reset()
        {
            // find new and old items
            var nowItems = groups.Values.SelectMany(i => i);
            var newItems = Lambdas.Select(i => Lambdas[i]).Except(nowItems).ToList();
            var oldItems = nowItems.Except(newItems).ToList();

            // remove old items
            foreach (var item in oldItems)
                RemoveItem(item);

            // add new items
            foreach (var item in newItems)
                AddItem(item);
        }

        void AddItem(TElement item)
        {
            // lambda associated with item
            var lambda = Lambdas[item];

            // group associated with key
            var group = GetOrCreateGroup(lambda.Value);

            // add item to group
            group.Add(item);
        }

        /// <summary>
        /// Removes the specified item with it's current key.
        /// </summary>
        /// <param name="item"></param>
        void RemoveItem(TElement item)
        {
            RemoveItemWithKey(item, Lambdas[item].Value);
        }

        /// <summary>
        /// Remoes the specified item with the given key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        void RemoveItemWithKey(TElement item, TKey key)
        {
            // group associated with key
            var group = GetOrCreateGroup(key);

            // remove item from group
            group.Remove(item);

            // if group is empty, remove group
            if (group.Count == 0)
            {
                groups.Remove(key);
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, group));
            }
        }

        Grouping<TKey, TElement> GetOrCreateGroup(TKey key)
        {
            // does group already exist?
            var group = groups.ValueOrDefault(key);
            if (group != null)
                return group;

            // create new group
            group = groups[key] = new Grouping<TKey, TElement>(key);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, group));
            return group;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return groups.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
