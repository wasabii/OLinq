using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Represents a single output value that monitors its underlying dependencies.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ObservableValue<TResult> : INotifyPropertyChanged, IDisposable
    {

        internal LambdaExpression resultExpr;
        internal IOperation<TResult> operation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected internal ObservableValue()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="func"></param>
        public ObservableValue(Expression<Func<TResult>> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            this.resultExpr = func;

            // operation to emit value
            operation = OperationFactory.FromExpression<TResult>(new OperationContext(), resultExpr);
            operation.ValueChanged += operation_ValueChanged;
        }

        /// <summary>
        /// Invoked when the operation's output value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnValueChanged(new ValueChangedEventArgs(args.OldValue, args.NewValue));
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

        public virtual void Dispose()
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

    /// <summary>
    /// Represents a single output value that monitors it's underlying dependencies.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public sealed class ObservableValue<TSource, TResult> : ObservableValue<TResult>
    {

        Expression sourceExpr;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sourceExpr"></param>
        /// <param name="resultExpr"></param>
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
