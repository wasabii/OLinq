using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    public class ObservableView<T> : IEnumerable<T>, INotifyCollectionChanged, IDisposable
    {

        private Expression expression;
        private IOperation<IEnumerable<T>> operation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="query"></param>
        internal ObservableView(ObservableQuery<T> query)
        {
            expression = query.Expression;

            // establish root operation, hook up events, and load
            operation = OperationFactory.FromExpression<IEnumerable<T>>(new OperationContext(), expression);
            operation.ValueChanged += operation_ValueChanged;
            operation.Load();
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

        public IEnumerator<T> GetEnumerator()
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
        public ObservableQuery<T> Query()
        {
            return new ObservableQuery<T>(this);
        }

    }

}
