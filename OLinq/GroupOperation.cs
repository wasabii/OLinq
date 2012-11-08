using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    abstract class GroupOperation<TSource, TResult> : Operation<TResult>
    {

        IOperation<IEnumerable<TSource>> sourceOp;

        public GroupOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            if (expression.Arguments.Count >= 1)
            {
                // source operation
                sourceOp = OperationFactory.FromExpression<IEnumerable<TSource>>(context, expression.Arguments[0]);
                sourceOp.Init();
                SubscribeSourceOperation(sourceOp);

                // initial value
                SetValue(default(TResult));
                SourceChanged(null, sourceOp.Value);
            }
        }

        /// <summary>
        /// Invoked when the value of source changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void sourceOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SourceChanged((IEnumerable<TSource>)args.OldValue, (IEnumerable<TSource>)args.NewValue);
        }

        /// <summary>
        /// Invoked when the source collection changes.
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
                    SourceCollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    SourceCollectionAdd(Utils.AsEnumerable<TSource>(args.NewItems), args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SourceCollectionRemove(Utils.AsEnumerable<TSource>(args.OldItems), args.OldStartingIndex);
                    break;
            }
        }

        /// <summary>
        /// Invoked when the value of an argument is changed.
        /// </summary>
        protected virtual void SourceChanged(IEnumerable<TSource> oldSource, IEnumerable<TSource> newSource)
        {
            UnsubscribeSourceCollection(oldSource);
            SubscribeSourceCollection(newSource);
            SourceCollectionReset();
        }

        /// <summary>
        /// Processes a large collection change.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        protected virtual void SourceCollectionReset()
        {

        }

        /// <summary>
        /// Processes an add operation.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void SourceCollectionAdd(IEnumerable<TSource> newItems, int startingIndex)
        {
            foreach (var item in newItems)
                SourceCollectionAddItem(item, startingIndex >= 0 ? startingIndex++ : -1);
        }

        /// <summary>
        /// Override to implement the logic required to acknowledge an item being added from the underlying operation.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void SourceCollectionAddItem(TSource item, int index)
        {

        }

        /// <summary>
        /// Processes a remove operation.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void SourceCollectionRemove(IEnumerable<TSource> oldItems, int startingIndex)
        {
            foreach (var item in oldItems.ToList())
                SourceCollectionRemoveItem(item, startingIndex >= 0 ? startingIndex++ : -1);
        }

        /// <summary>
        /// Override to implement the logic required to remove an underlying item from the operation.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void SourceCollectionRemoveItem(TSource item, int index)
        {

        }

        /// <summary>
        /// Gets the current source collection.
        /// </summary>
        protected IEnumerable<TSource> SourceCollection
        {
            get { return sourceOp.Value; }
        }

        /// <summary>
        /// Subscribes to the specified source operation.
        /// </summary>
        /// <param name="sourceOp"></param>
        void SubscribeSourceOperation(IOperation<IEnumerable> sourceOp)
        {
            sourceOp.ValueChanged += sourceOp_ValueChanged;
        }

        /// <summary>
        /// Unsubscribes from the specified source operation.
        /// </summary>
        /// <param name="sourceOp"></param>
        void UnsubscribeSourceOperation(IOperation<IEnumerable> sourceOp)
        {
            sourceOp.ValueChanged -= sourceOp_ValueChanged;
        }

        /// <summary>
        /// Subscribes to the given source collection.
        /// </summary>
        /// <param name="sourceCollection"></param>
        void SubscribeSourceCollection(IEnumerable sourceCollection)
        {
            var collection = sourceCollection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += source_CollectionChanged;
        }

        /// <summary>
        /// Unsubscribes from the given source collection.
        /// </summary>
        /// <param name="sourceCollection"></param>
        void UnsubscribeSourceCollection(IEnumerable sourceCollection)
        {
            var collection = sourceCollection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += source_CollectionChanged;
        }

        public override void Dispose()
        {
            if (sourceOp != null)
            {
                UnsubscribeSourceCollection(sourceOp.Value);
                UnsubscribeSourceOperation(sourceOp);
                sourceOp.Dispose();
                sourceOp = null;
            }

            base.Dispose();
        }

        public override void Init()
        {
            if (sourceOp != null)
                sourceOp.Init();

            // for operations that require event during init
            OnValueChanged(default(TResult), Value);

            base.Init();
        }

    }

}
