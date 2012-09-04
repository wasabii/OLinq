using System;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Represents a lambda expression.
    /// </summary>
    class LambdaOperation<T> : Operation<T>
    {

        IOperation body;

        public LambdaOperation(OperationContext context, LambdaExpression expression)
            : base(context, expression)
        {
            if (expression.Body != null)
            {
                body = OperationFactory.FromExpression(context, expression.Body);
                body.ValueChanged += body_ValueChanged;
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

        public override void Load()
        {
            if (body != null)
                body.Load();
            base.Load();
        }

    }

}
