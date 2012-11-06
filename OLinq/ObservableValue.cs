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

            // provide source
            var ctx = new OperationContext();
            var src = OperationFactory.FromExpression<TSource>(new OperationContext(), sourceExpr);
            ctx.Variables[resultExpr.Parameters[0].Name] = src;

            // operation to emit value
            operation = OperationFactory.FromExpression<TResult>(ctx, resultExpr);
            operation.ValueChanged += operation_ValueChanged;
            operation.Init();
        }

        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            OnValueChanged(new ValueChangedEventArgs(args.OldValue, args.NewValue));
            OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }

        public TResult Value
        {
            get { return operation.Value; }
        }

        public event ValueChangedEventHandler ValueChanged;

        internal void OnValueChanged(ValueChangedEventArgs args)
        {
            if (ValueChanged != null)
                ValueChanged(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        public void Dispose()
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
