using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    public class ObservableView<TElement> : IEnumerable<TElement>, INotifyCollectionChanged, IDisposable
    {

        private Expression expression;
        private Operation<IEnumerable<TElement>> operation;
        private ObservableBuffer<TElement> buffer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="query"></param>
        internal ObservableView(ObservableQuery<TElement> query)
        {
            expression = query.Expression;

            // establish root operation, hook up events, and load
            operation = (Operation<IEnumerable<TElement>>)OperationFactory.FromExpression<IEnumerable<TElement>>(new OperationContext(), expression);
            operation.ValueChanged += operation_ValueChanged;
            operation.Init();
        }

        /// <summary>
        /// Invoked when the handler changes its output value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= operation_CollectionChanged;

            var newValue = args.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += operation_CollectionChanged;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

        public IEnumerator<TElement> GetEnumerator()
        {
            return operation.Value.GetEnumerator();
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
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Disposes of the view instance, unsubscribing from all listened to events.
        /// </summary>
        public void Dispose()
        {
            if (operation != null)
            {
                var operationValue = operation.Value as INotifyCollectionChanged;
                operation.ValueChanged -= operation_ValueChanged;
                operation.Dispose();
                if (operationValue != null)
                    operationValue.CollectionChanged -= operation_CollectionChanged;
                operation = null;
            }
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
        /// library that requires a populated ObservableCollection.
        /// </summary>
        /// <returns></returns>
        public ObservableBuffer<TElement> ToBuffer()
        {
            return buffer ?? (buffer = new ObservableBuffer<TElement>(this));
        }

    }

}
