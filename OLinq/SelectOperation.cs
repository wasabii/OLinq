using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectOperation<TSource, TResult> : Operation<IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        Expression sourceExpr;
        IOperation<IEnumerable<TSource>> source;
        Expression<Func<TSource, TResult>> selectorExpr;
        Dictionary<TSource, LambdaOperation<TResult>> selectors = new Dictionary<TSource, LambdaOperation<TResult>>();

        public SelectOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            // extract expressions
            sourceExpr = expression.Arguments[0];
            selectorExpr = Utils.UnpackLambda<TSource, TResult>(expression.Arguments[1]);

            // source operation
            source = OperationFactory.FromExpression<IEnumerable<TSource>>(context, sourceExpr);
            source.Init();
            source.ValueChanged += source_ValueChanged;

            // initial collection load
            SetValue(this);
            SourceValueChanged(null, source.Value);
        }

        void source_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SourceValueChanged((IEnumerable<TSource>)args.OldValue, (IEnumerable<TSource>)args.NewValue);
        }

        /// <summary>
        /// Invoke when the source collection changes.
        /// </summary>
        /// <param name="oldSource"></param>
        /// <param name="newSource"></param>
        void SourceValueChanged(IEnumerable<TSource> oldSource, IEnumerable<TSource> newSource)
        {
            var oldValue = oldSource as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= source_CollectionChanged;

            var newValue = newSource as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += source_CollectionChanged;

            ResetCollection(Utils.AsEnumerable<TSource>(oldSource), Utils.AsEnumerable<TSource>(newSource));
        }

        /// <summary>
        /// Processes a large collection change.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        void ResetCollection(IEnumerable<TSource> oldItems, IEnumerable<TSource> newItems)
        {
            // items that have been removed
            var oldSelectors = oldItems.Except(newItems).ToList();
            foreach (var selector in oldSelectors.Select(i => GetOrCreateSelector(i)))
                ReleaseSelector(selector);

            // items that have been added
            foreach (var item in newItems)
                GetOrCreateSelector(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Processes an add operation.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        void AddCollection(IEnumerable<TSource> newItems, int startingIndex)
        {
            var addValues = newItems.Select(i => GetOrCreateSelector(i).Value).ToList();
            if (addValues.Count > 0)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addValues, startingIndex));
        }

        /// <summary>
        /// Processes a remove operation.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        void RemoveCollection(IEnumerable<TSource> oldItems, int startingIndex)
        {
            var oldSelectors = oldItems.Select(i => GetOrCreateSelector(i)).ToList();
            foreach (var selector in oldSelectors)
                ReleaseSelector(selector);

            var oldValues = oldSelectors.Select(i => i.Value).ToList();
            if (oldValues.Count > 0)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValues, startingIndex));
        }

        /// <summary>
        /// Invoked when the source collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    ResetCollection(Utils.AsEnumerable<TSource>(args.OldItems), Utils.AsEnumerable<TSource>(args.NewItems));
                    break;
                case NotifyCollectionChangedAction.Add:
                    AddCollection(Utils.AsEnumerable<TSource>(args.NewItems), args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveCollection(Utils.AsEnumerable<TSource>(args.OldItems), args.OldStartingIndex);
                    break;
            }
        }

        /// <summary>
        /// Invoked when any of the current selectors change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void selector_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = (TResult)args.OldValue;
            var newValue = (TResult)args.NewValue;

            // single value has been replaced
            if (!object.Equals(oldValue, newValue))
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new TResult[] { newValue }, new TResult[] { oldValue }));
        }

        /// <summary>
        /// Gets or creates a selector for the given source item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        LambdaOperation<TResult> GetOrCreateSelector(TSource item)
        {
            return selectors.GetOrCreate(item, i =>
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = new ValueOperation<TSource>(item);
                ctx.Variables[selectorExpr.Parameters[0].Name] = var;

                // create new test and subscribe to test modifications
                var selector = new LambdaOperation<TResult>(ctx, selectorExpr);
                selector.Tag = item;
                selector.Init(); // load before value changed to prevent double notification
                selector.ValueChanged += selector_ValueChanged;
                return selector;
            });
        }

        /// <summary>
        /// Releases the given selector.
        /// </summary>
        /// <param name="selector"></param>
        void ReleaseSelector(LambdaOperation<TResult> selector)
        {
            // remove from selectors
            selectors.Remove((TSource)selector.Tag);

            // dispose of selector and variables
            selector.ValueChanged -= selector_ValueChanged;
            selector.Dispose();
            foreach (var var in selector.Context.Variables)
                var.Value.Dispose();
        }

        IEnumerator<TResult> GetEnumerator()
        {
            return source.Value.Select(i => GetOrCreateSelector(i).Value).GetEnumerator();
        }

        public override void Init()
        {
            base.Init();

            OnValueChanged(null, this);
        }

        public override void Dispose()
        {
            if (source != null)
            {
                source.ValueChanged -= source_ValueChanged;
                source.Dispose();
                var sourceValue = source.Value as INotifyCollectionChanged;
                if (sourceValue != null)
                    sourceValue.CollectionChanged -= source_CollectionChanged;
            }

            foreach (var selector in selectors.Values)
                ReleaseSelector(selector);
            selectors = null;

            base.Dispose();
        }

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
