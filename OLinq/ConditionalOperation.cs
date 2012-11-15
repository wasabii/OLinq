using System.Linq.Expressions;

namespace OLinq
{

    class ConditionalOperation<TResult> : Operation<TResult>
    {

        IOperation<bool> testOp;
        IOperation<TResult> trueOp;
        IOperation<TResult> falseOp;

        public ConditionalOperation(OperationContext context, ConditionalExpression expression)
            : base(context, expression)
        {
            testOp = OperationFactory.FromExpression<bool>(context, expression.Test);
            testOp.ValueChanged += testOp_ValueChanged;

            trueOp = OperationFactory.FromExpression<TResult>(context, expression.IfTrue);
            trueOp.ValueChanged += trueOp_ValueChanged;

            falseOp = OperationFactory.FromExpression<TResult>(context, expression.IfFalse);
            falseOp.ValueChanged += falseOp_ValueChanged;

            // set initial value
            ResetValue();
        }

        void testOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            ResetValue();
        }

        void trueOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            ResetValue();
        }

        void falseOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            ResetValue();
        }

        void ResetValue()
        {
            SetValue(testOp.Value ? trueOp.Value : falseOp.Value);
        }

    }

}
