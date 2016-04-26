using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class GroupByOperation<TElement, TKey> : EnumerableSourceWithFuncOperation<TElement, TKey, IEnumerable<IGrouping<TKey, TElement>>>, IEnumerable<IGrouping<TKey, TElement>>, INotifyCollectionChanged
    {

        Dictionary<TKey, Grouping<TKey, TElement>> groups = new Dictionary<TKey, Grouping<TKey, TElement>>();

        public GroupByOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TElement, TKey>(1))
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionReset()
        {
            Reset();
        }

        protected override void OnLambdaCollectionItemsAdded(IEnumerable<FuncOperation<TKey>> newItems, int startingIndex)
        {
            foreach (var item in newItems.Select(i => Funcs[i]))
                AddItem(item);
        }

        protected override void OnLambdaCollectionItemsRemoved(IEnumerable<FuncOperation<TKey>> oldItems, int startingIndex)
        {
            foreach (var item in oldItems.Select(i => Funcs[i]))
                RemoveItem(item);
        }

        protected override void OnLambdaValueChanged(FuncValueChangedEventArgs<TElement, TKey> args)
        {
            RemoveItemWithKey(args.Item, args.OldValue);
            AddItem(args.Item);
        }

        /// <summary>
        /// Resets the collection by reevaluating the groupings.
        /// </summary>
        void Reset()
        {
            // find new and old items
            var nowItems = groups.Values.SelectMany(i => i);
            var newItems = Funcs.Select(i => Funcs[i]).Except(nowItems).ToList();
            var oldItems = nowItems.Except(newItems).ToList();

            // remove old items
            foreach (var item in oldItems)
                RemoveItem(item);

            // add new items
            foreach (var item in newItems)
                AddItem(item);
        }

        /// <summary>
        /// Adds an item to the appropriate grouping
        /// </summary>
        /// <param name="item"></param>
        void AddItem(TElement item)
        {
            // lambda associated with item
            var lambda = Funcs[item];

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
            RemoveItemWithKey(item, Funcs[item].Value);
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
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(RaiseCollectionChanged, group);
            }
        }

        /// <summary>
        /// Gets or creates a grouping for the specified key value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Grouping<TKey, TElement> GetOrCreateGroup(TKey key)
        {
            // does group already exist?
            var group = groups.GetOrDefault(key);
            if (group != null)
                return group;

            // create new group
            group = groups[key] = new Grouping<TKey, TElement>(key);
            NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(RaiseCollectionChanged, group);
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

        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
