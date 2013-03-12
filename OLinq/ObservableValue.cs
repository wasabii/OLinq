using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Represents a single output value that monitors its underlying dependencies.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class ObservableValue<TResult> : INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// Gets the value.
        /// </summary>
        public abstract TResult Value { get; }

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
        /// Disposes of the instance.
        /// </summary>
        public abstract void Dispose();

    }

    /// <summary>
    /// Implementation of ObservableValue{TResult}.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    sealed class ObservableValue<TSource, TResult> : ObservableValue<TResult>
    {

        Expression sourceExpr;
        LambdaExpression resultExpr;
        IOperation<TResult> operation;

        internal ObservableValue(Expression sourceExpr, LambdaExpression resultExpr)
        {
            if (sourceExpr == null)
                throw new ArgumentNullException("sourceExpr");
            if (resultExpr == null)
                throw new ArgumentNullException("resultExpr");
            if (resultExpr.Parameters.Count != 1)
                throw new ArgumentException("resultExpr");

            this.sourceExpr = sourceExpr;
            this.resultExpr = resultExpr;

            // provide source
            var ctx = new OperationContext();
            var src = OperationFactory.FromExpression<TSource>(new OperationContext(), sourceExpr);
            ctx.Variables[resultExpr.Parameters[0].Name] = src;

            // operation to emit value
            operation = OperationFactory.FromExpression<TResult>(ctx, resultExpr);
            operation.ValueChanged += operation_ValueChanged;
        }

        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnValueChanged(new ValueChangedEventArgs(args.OldValue, args.NewValue));
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }

        public override TResult Value
        {
            get { return operation.Value; }
        }

        public override void Dispose()
        {
            // dispose of the operation
            operation.ValueChanged -= operation_ValueChanged;
            operation.Dispose();

            // dispose of variables
            foreach (var var in operation.Context.Variables.Values)
                var.Dispose();

            operation = null;
        }

    }

}
