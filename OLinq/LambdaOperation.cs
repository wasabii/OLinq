using System;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Represents a lambda expression.
    /// </summary>
    class LambdaOperation<T> : Operation<T>
    {

        IOperation<T> body;

        public LambdaOperation(OperationContext context, LambdaExpression expression)
            : base(context, expression)
        {
            if (expression.Body != null)
            {
                body = (IOperation<T>)OperationFactory.FromExpression(context, expression.Body);
                body.ValueChanged += body_ValueChanged;
                SetValue(body.Value);
            }
        }

        /// <summary>
        /// Invoked when the result of Body is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void body_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SetValue((T)body.Value);
        }

        public override void Dispose()
        {
            if (body != null)
            {
                body.ValueChanged -= body_ValueChanged;
                body.Dispose();
            }

            base.Dispose();
        }

    }

}
