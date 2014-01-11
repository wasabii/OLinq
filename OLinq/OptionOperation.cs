using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Sets a context option.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class OptionOperation<T> :
        Operation<T>
    {

        IOperation<T> operation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public OptionOperation(OperationContext context, Expression expression)
            : base(context, expression)
        {
            this.operation = OperationFactory.FromExpression<T>(new OperationContext(context), expression);
            this.operation.ValueChanged += operation_ValueChanged;
        }

        void operation_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SetValue((T)args.NewValue);
        }

    }

}
