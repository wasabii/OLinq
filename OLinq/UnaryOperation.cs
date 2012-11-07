using System;
using System.Linq.Expressions;

namespace OLinq
{

    abstract class UnaryOperation<TIn, TOut> : Operation<TOut>
    {

        IOperation<TIn> operand;

        protected UnaryOperation(OperationContext context, UnaryExpression expression)
            : base(context, expression)
        {
            if (expression.Operand != null)
            {
                operand = OperationFactory.FromExpression<TIn>(context, expression.Operand);
                operand.ValueChanged += operand_ValueChanged;
                operand.Init();
            }
        }

        /// <summary>
        /// Invoked when the result value of the operand is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operand_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SetValue(CoerceValue((TIn)args.NewValue));
        }

        /// <summary>
        /// Used to coerce the value from the input type to the output type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract TOut CoerceValue(TIn value);

        public override void Init()
        {
            if (operand != null)
                OnValueChanged(null, Value);
            base.Init();
        }

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
