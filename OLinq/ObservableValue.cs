using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Represents a single output value that monitors its underlying dependencies.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ObservableValue<TResult> : INotifyPropertyChanged, IObservable<TResult>, IDisposable
    {

        IOperation<TResult> operation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected internal ObservableValue()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="func"></param>
        public ObservableValue(Expression<Func<TResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            // initialize result
            Initialize(OperationFactory.FromExpression<TResult>(new OperationContext(), func));
        }

        /// <summary>
        /// Initializes the operation.
        /// </summary>
        /// <param name="op"></param>
        internal void Initialize(IOperation<TResult> op)
        {
            operation = op;
            operation.ValueChanged += operation_ValueChanged;
        }

        /// <summary>
        /// Invoked when the operation's output value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnValueChanged(args);
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }

        /// <summary>
        /// Gets the current observed value.
        /// </summary>
        public TResult Value
        {
            get { return operation.Value; }
        }

        /// <summary>
        /// Raised when the value is changed.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args"></param>
        protected void OnValueChanged(ValueChangedEventArgs args)
        {
            if (ValueChanged != null)
                ValueChanged(this, args);
        }

        /// <summary>
        /// Raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="args"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        /// <summary>
        /// Disposes of the current instance.
        /// </summary>
        public void Dispose()
        {
            if (operation != null)
            {
                // dispose of the operation
                operation.ValueChanged -= operation_ValueChanged;
                operation.Dispose();

                // dispose of variables
                if (operation.Context != null)
                    foreach (var var in operation.Context.Variables.Values)
                        var.Dispose();

                operation = null;
            }
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        IDisposable IObservable<TResult>.Subscribe(IObserver<TResult> observer)
        {
            return new ObservableSubscription<TResult>(this, observer);
        }

    }

    /// <summary>
    /// Represents a single output value that monitors it's underlying dependencies.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public sealed class ObservableValue<TSource, TResult> : ObservableValue<TResult>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="func"></param>
        internal ObservableValue(Expression source, LambdaExpression func)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (func == null)
                throw new ArgumentNullException("func");
            if (func.Parameters.Count != 1)
                throw new ArgumentException("func");

            // provide source
            var src = OperationFactory.FromExpression<TSource>(new OperationContext(), source);

            // provide source to func operation as named variable
            var ctx = new OperationContext();
            ctx.Variables[func.Parameters[0].Name] = src;

            // initialize result
            Initialize(OperationFactory.FromExpression<TResult>(ctx, func));
        }

    }

}
