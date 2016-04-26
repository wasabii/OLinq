using System.Linq.Expressions;

namespace OLinq
{

    class ConditionalOperation<TResult> : Operation<TResult>
    {

        ConditionalExpression expression;
        IOperation<bool> testOp;
        IOperation<TResult> valueOp;
        IOperation<TResult> trueOp;
        IOperation<TResult> falseOp;

        public ConditionalOperation(OperationContext context, ConditionalExpression expression)
            : base(context, expression)
        {
            this.expression = expression;

            testOp = OperationFactory.FromExpression<bool>(context, expression.Test);
            testOp.ValueChanged += testOp_ValueChanged;

            //trueOp = OperationFactory.FromExpression<TResult>(context, expression.IfTrue);
            //trueOp.ValueChanged += trueOp_ValueChanged;

            //falseOp = OperationFactory.FromExpression<TResult>(context, expression.IfFalse);
            //falseOp.ValueChanged += falseOp_ValueChanged;

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
            if (testOp.Value)
            {
                if (falseOp != null)
                {
                    falseOp.ValueChanged -= falseOp_ValueChanged;
                    falseOp.Dispose();
                    falseOp = null;
                }

                if (trueOp == null)
                {
                    trueOp = OperationFactory.FromExpression<TResult>(Context, expression.IfTrue);
                    trueOp.ValueChanged += trueOp_ValueChanged;
                }

                SetValue(trueOp.Value);
            }
            else
            {
                if (trueOp != null)
                {
                    trueOp.ValueChanged -= trueOp_ValueChanged;
                    trueOp.Dispose();
                    trueOp = null;
                }

                if (falseOp == null)
                {
                    falseOp = OperationFactory.FromExpression<TResult>(Context, expression.IfFalse);
                    falseOp.ValueChanged += falseOp_ValueChanged;
                }

                SetValue(falseOp.Value);
            }
        }

    }

}
