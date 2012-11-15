using System;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class NewOperation<T> : Operation<T>
    {

        IOperation[] argumentOps;

        public NewOperation(OperationContext context, NewExpression expression)
            : base(context, expression)
        {
            argumentOps = new IOperation[expression.Arguments.Count];
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                argumentOps[i] = OperationFactory.FromExpression(context, expression.Arguments[i]);
                argumentOps[i].ValueChanged += argument_ValueChanged;
            }

            ResetValue();
        }

        /// <summary>
        /// Invoked when the value of one of the arguments changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void argument_ValueChanged(object sender, ValueChangedEventArgs args)
        {
                ResetValue();
        }

        void ResetValue()
        {
            var args = new object[argumentOps.Length];
            for (int i = 0; i < argumentOps.Length; i++)
                args[i] = argumentOps[i].Value;

            SetValue((T)((NewExpression)Expression).Constructor.Invoke(args));
        }

        public override void Dispose()
        {
            foreach (var op in argumentOps)
            {
                op.ValueChanged -= argument_ValueChanged;
                op.Dispose();
            }

            base.Dispose();
        }

    }

}
