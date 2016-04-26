using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Final product of an <see cref="ObservableQuery{TElement}"/>. Allows iteration and raises events when the
    /// underlying collections change.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public class ObservableView<TElement> : IEnumerable<TElement>, INotifyCollectionChanged, IDisposable
    {

        Expression expression;
        Operation<IEnumerable<TElement>> operation;
        ObservableBuffer<TElement> buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="query"></param>
        internal ObservableView(ObservableQuery<TElement> query)
        {
            expression = query.Expression;

            // establish root operation, hook up events, and load
            operation = (Operation<IEnumerable<TElement>>)OperationFactory.FromExpression<IEnumerable<TElement>>(new OperationContext(), expression);
            SubscribeOperation(operation);
            OperationChanged(null, operation.Value);
        }

        /// <summary>
        /// Invoked when the operation's value changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OperationChanged((IEnumerable<TElement>)args.OldValue, (IEnumerable<TElement>)args.NewValue);
        }

        /// <summary>
        /// Invoked when the operation's collection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(args);
        }

        /// <summary>
        /// Invoked when the value of the operation is changed.
        /// </summary>
        void OperationChanged(IEnumerable<TElement> oldValue, IEnumerable<TElement> newValue)
        {
            UnsubscribeOperationCollection(oldValue);
            SubscribeOperationCollection(newValue);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Subscribes to the specified source operation.
        /// </summary>
        /// <param name="operation"></param>
        void SubscribeOperation(IOperation<IEnumerable> operation)
        {
            operation.ValueChanged += operation_ValueChanged;
        }

        /// <summary>
        /// Unsubscribes from the specified source operation.
        /// </summary>
        /// <param name="operation"></param>
        void UnsubscribeOperation(IOperation<IEnumerable> operation)
        {
            operation.ValueChanged -= operation_ValueChanged;
        }

        /// <summary>
        /// Subscribes to the given source collection.
        /// </summary>
        /// <param name="sourceCollection"></param>
        void SubscribeOperationCollection(IEnumerable value)
        {
            var collection = value as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += operation_CollectionChanged;
        }

        /// <summary>
        /// Unsubscribes from the given source collection.
        /// </summary>
        /// <param name="sourceCollection"></param>
        void UnsubscribeOperationCollection(IEnumerable value)
        {
            var collection = value as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += operation_CollectionChanged;
        }

        /// <summary>
        /// Disposes of the view instance, unsubscribing from all listened to events.
        /// </summary>
        public void Dispose()
        {
            if (operation != null)
            {
                UnsubscribeOperationCollection(operation.Value);
                UnsubscribeOperation(operation);
                operation.Dispose();
                operation = null;
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return operation.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Raised when the items in the query change.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Initiates a new query using this view as the base.
        /// </summary>
        /// <returns></returns>
        public ObservableQuery<TElement> Query()
        {
            return new ObservableQuery<TElement>(this);
        }

        /// <summary>
        /// Gets a buffer that is kept in sync with the view. This method might be useful if you are working with a
        /// library that requires a populated <see cref="ObservableCollection{TElement}"/>.
        /// </summary>
        /// <returns></returns>
        public ObservableBuffer<TElement> ToBuffer()
        {
            return buffer ?? (buffer = new ObservableBuffer<TElement>(this));
        }

    }

}
