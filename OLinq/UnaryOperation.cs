using System.Linq.Expressions;

namespace OLinq
{

    abstract class UnaryOperation<TOperand, TResult> : Operation<TResult>
    {

        IOperation<TOperand> operand;

        protected UnaryOperation(OperationContext context, UnaryExpression expression)
            : base(context, expression)
        {
            if (expression.Operand != null)
            {
                operand = OperationFactory.FromExpression<TOperand>(context, expression.Operand);
                operand.ValueChanged += operand_ValueChanged;
                SetValue(CoerceValue((TOperand)operand.Value));
            }
        }

        /// <summary>
        /// Invoked when the result value of the operand is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operand_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SetValue(CoerceValue((TOperand)args.NewValue));
        }

        /// <summary>
        /// Used to coerce the value from the input type to the output type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract TResult CoerceValue(TOperand value);

        public override void Dispose()
        {
            if (operand != null)
            {
                operand.ValueChanged -= operand_ValueChanged;
                operand.Dispose();
            }

            base.Dispose();
        }

    }

}
