using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    abstract class EnumerableSourceOperation<TSource, TResult> : Operation<TResult>
    {

        IOperation<IEnumerable<TSource>> sourceOp;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <param name="sourceExpression"></param>
        public EnumerableSourceOperation(OperationContext context, MethodCallExpression expression, Expression sourceExpression)
            : base(context, expression)
        {
            // source operation
            sourceOp = OperationFactory.FromExpression<IEnumerable<TSource>>(context, sourceExpression);
            SubscribeSourceOperation(sourceOp);
            SourceChanged(null, sourceOp.Value);
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
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                    OnSourceCollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnSourceCollectionItemsAdded(Utils.AsEnumerable<TSource>(args.NewItems), args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnSourceCollectionItemsRemoved(Utils.AsEnumerable<TSource>(args.OldItems), args.OldStartingIndex);
                    break;
            }
        }

        /// <summary>
        /// Invoked when the value of an argument is changed.
        /// </summary>
        void SourceChanged(IEnumerable<TSource> oldSource, IEnumerable<TSource> newSource)
        {
            UnsubscribeSourceCollection(oldSource);
            SubscribeSourceCollection(newSource);
            OnSourceCollectionReset();
        }

        /// <summary>
        /// Gets the current source collection.
        /// </summary>
        protected IEnumerable<TSource> Source
        {
            get { return sourceOp.Value; }
        }

        /// <summary>
        /// Processes a large collection change.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        protected virtual void OnSourceCollectionReset()
        {

        }

        /// <summary>
        /// Processes an add operation.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnSourceCollectionItemsAdded(IEnumerable<TSource> newItems, int startingIndex)
        {

        }

        /// <summary>
        /// Processes a remove operation.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnSourceCollectionItemsRemoved(IEnumerable<TSource> oldItems, int startingIndex)
        {

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

    }

}
