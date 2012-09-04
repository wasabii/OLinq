using System;
using System.Linq.Expressions;

namespace OLinq
{

    class UnaryOperation<T> : Operation<T>
    {

        IOperation<T> operand;

        protected UnaryOperation(OperationContext context, UnaryExpression expression)
            : base(context, expression)
        {
            if (expression.Operand != null)
            {
                operand = OperationFactory.FromExpression<T>(context, expression.Operand);
                operand.ValueChanged += operand_ValueChanged;
            }
        }

        /// <summary>
        /// Invoked when the result value of the operand is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void operand_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SetValue((T)args.NewValue);
        }

        public override void Load()
        {
            if (operand != null)
                operand.Load();
            base.Load();
        }

    }

}
