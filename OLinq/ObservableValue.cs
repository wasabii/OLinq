using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OLinq
{

    public sealed class ObservableValue<TSource, TResult> : INotifyPropertyChanged, IDisposable
    {

        private Expression sourceExpr;
        private LambdaExpression resultExpr;
        private IOperation<TResult> operation;

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
        }

        /// <summary>
        /// Ensures an operation has been constructed.
        /// </summary>
        void Ensure()
        {
            if (operation == null)
            {
                // provide source
                var ctx = new OperationContext();
                var src = OperationFactory.FromExpression<TSource>(new OperationContext(), sourceExpr);
                ctx.Variables[resultExpr.Parameters[0].Name] = src;

                // operation to emit value
                operation = OperationFactory.FromExpression<TResult>(ctx, resultExpr);
                operation.ValueChanged += operation_ValueChanged;
                operation.Load();
            }
        }

        /// <summary>
        /// Checks whether the operation should be disposed.
        /// </summary>
        void Check()
        {
            if (propertyChanged == null && valueChanged == null)
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

        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnValueChanged(new ValueChangedEventArgs(args.OldValue, args.NewValue));
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }

        public TResult Value
        {
            get { Ensure(); return operation.Value; }
        }

        event ValueChangedEventHandler valueChanged;

        public event ValueChangedEventHandler ValueChanged
        {
            add { Ensure(); valueChanged += value; }
            remove { valueChanged -= value; Check(); }
        }

        internal void OnValueChanged(ValueChangedEventArgs args)
        {
            if (valueChanged != null)
                valueChanged(this, args);
        }

        event PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { Ensure(); propertyChanged += value; }
            remove { propertyChanged -= value; Check(); }
        }

        internal void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (propertyChanged != null)
                propertyChanged(this, args);
        }

        public void Dispose()
        {
            valueChanged = null;
            propertyChanged = null;
            Check();
        }

    }

}
