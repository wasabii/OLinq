using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class EnumerableSource2Operation<TSource1, TSource2, TResult> : EnumerableSourceOperation<TSource1, TResult>
    {

        IOperation<IEnumerable<TSource2>> source2Op;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <param name="source1Expression"></param>
        /// <param name="source2Expression"></param>
        public EnumerableSource2Operation(OperationContext context, MethodCallExpression expression, Expression source1Expression, Expression source2Expression)
            : base(context, expression, source1Expression)
        {
            source2Op = OperationFactory.FromExpression<IEnumerable<TSource2>>(Context, source2Expression);
            SubscribeSource2Operation(source2Op);
            Source2Changed(null, source2Op.Value);
        }

        /// <summary>
        /// Invoked when the value of source changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void source2Op_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Source2Changed((IEnumerable<TSource2>)args.OldValue, (IEnumerable<TSource2>)args.NewValue);
        }

        /// <summary>
        /// Invoked when the source collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void source2_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    OnSource2CollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnSource2CollectionItemsAdded(Utils.AsEnumerable<TSource2>(args.NewItems), args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnSource2CollectionItemsRemoved(Utils.AsEnumerable<TSource2>(args.OldItems), args.OldStartingIndex);
                    break;
            }
        }

        /// <summary>
        /// Invoked when the value of an argument is changed.
        /// </summary>
        protected virtual void Source2Changed(IEnumerable<TSource2> oldSource, IEnumerable<TSource2> newSource)
        {
            UnsubscribeSource2Collection(oldSource);
            SubscribeSource2Collection(newSource);
            OnSource2CollectionReset();
        }

        /// <summary>
        /// Gets the current source collection.
        /// </summary>
        protected IEnumerable<TSource2> Source2
        {
            get { return source2Op.Value; }
        }

        /// <summary>
        /// Processes a large collection change.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        protected virtual void OnSource2CollectionReset()
        {

        }

        /// <summary>
        /// Processes an add operation.
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnSource2CollectionItemsAdded(IEnumerable<TSource2> newItems, int startingIndex)
        {
            foreach (var item in newItems)
                OnSource2CollectionItemAdded(item, startingIndex >= 0 ? startingIndex++ : -1);
        }

        /// <summary>
        /// Override to implement the logic required to acknowledge an item being added from the underlying operation.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnSource2CollectionItemAdded(TSource2 item, int index)
        {

        }

        /// <summary>
        /// Processes a remove operation.
        /// </summary>
        /// <param name="oldItems"></param>
        /// <param name="startingIndex"></param>
        protected virtual void OnSource2CollectionItemsRemoved(IEnumerable<TSource2> oldItems, int startingIndex)
        {
            foreach (var item in oldItems.ToList())
                OnSource2CollectionItemRemoved(item, startingIndex >= 0 ? startingIndex++ : -1);
        }

        /// <summary>
        /// Override to implement the logic required to remove an underlying item from the operation.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnSource2CollectionItemRemoved(TSource2 item, int index)
        {

        }

        /// <summary>
        /// Subscribes to the specified source operation.
        /// </summary>
        /// <param name="sourceOp"></param>
        void SubscribeSource2Operation(IOperation<IEnumerable> source2Op)
        {
            source2Op.ValueChanged += source2Op_ValueChanged;
        }

        /// <summary>
        /// Unsubscribes from the specified source operation.
        /// </summary>
        /// <param name="sourceOp"></param>
        void UnsubscribeSource2Operation(IOperation<IEnumerable> source2Op)
        {
            source2Op.ValueChanged -= source2Op_ValueChanged;
        }

        /// <summary>
        /// Subscribes to the given source collection.
        /// </summary>
        /// <param name="sourceCollection"></param>
        void SubscribeSource2Collection(IEnumerable source2Collection)
        {
            var collection = source2Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += source2_CollectionChanged;
        }

        /// <summary>
        /// Unsubscribes from the given source collection.
        /// </summary>
        /// <param name="sourceCollection"></param>
        void UnsubscribeSource2Collection(IEnumerable source2Collection)
        {
            var collection = source2Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += source2_CollectionChanged;
        }

        public override void Dispose()
        {
            if (source2Op != null)
            {
                UnsubscribeSource2Collection(source2Op.Value);
                UnsubscribeSource2Operation(source2Op);
                source2Op.Dispose();
                source2Op = null;
            }

            base.Dispose();
        }

    }

}
