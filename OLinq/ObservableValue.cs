using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OLinq
{

    public sealed class ObservableValue<TSource, TResult> : INotifyPropertyChanged
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

        void Load()
        {
            if (operation == null)
            {
                // provide source
                var ctx = new OperationContext();
                var src = OperationFactory.FromExpression<TSource>(new OperationContext(), sourceExpr);
                ctx.Variables[resultExpr.Parameters[0].Name] = src;

                // operation to emit value
                operation = OperationFactory.FromExpression<TResult>(ctx, resultExpr);
                operation.Load();
                operation.ValueChanged += operation_ValueChanged;
            }
        }

        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnValueChanged(new ValueChangedEventArgs(args.OldValue, args.NewValue));
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }

        public TResult Value
        {
            get { Load(); return operation.Value; }
        }

        event PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { Load(); propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        internal void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (propertyChanged != null)
                propertyChanged(this, args);
        }

        event ValueChangedEventHandler valueChanged;

        public event ValueChangedEventHandler ValueChanged
        {
            add { Load(); valueChanged += value; }
            remove { valueChanged -= value; }
        }

        internal void OnValueChanged(ValueChangedEventArgs args)
        {
            if (valueChanged != null)
                valueChanged(this, args);
        }

    }

}
