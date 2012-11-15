using System.Linq.Expressions;

namespace OLinq
{

    class CallOperation<T> : Operation<T>
    {

        IOperation targetOp;
        IOperation[] parameterOps;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public CallOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            if (expression.Object != null)
            {
                targetOp = OperationFactory.FromExpression(context, expression.Object);
                targetOp.ValueChanged += targetOp_ValueChanged;
            }

            parameterOps = new IOperation[expression.Arguments.Count];
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                parameterOps[i] = OperationFactory.FromExpression(context, expression.Arguments[i]);
                parameterOps[i].ValueChanged += parameterOp_ValueChanged;
            }

            Invoke();
        }

        /// <summary>
        /// Invoked when the value of Object is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void targetOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Invoke();
        }

        /// <summary>
        /// Invoked when the value of an argument is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void parameterOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Invoke();
        }

        protected virtual T Invoke(object target, params object[] parameters)
        {
            return (T)((MethodCallExpression)Expression).Method.Invoke(target, parameters);
        }

        /// <summary>
        /// Invokes the method and saves the result.
        /// </summary>
        void Invoke()
        {
            var args = new object[parameterOps.Length];
            for (int i = 0; i < parameterOps.Length; i++)
                args[i] = parameterOps[i].Value;

            SetValue(Invoke(targetOp != null ? targetOp.Value : null, args));
        }

        public override void Dispose()
        {
            if (targetOp != null)
            {
                targetOp.ValueChanged -= targetOp_ValueChanged;
                targetOp.Dispose();
            }

            foreach (var op in parameterOps)
            {
                op.ValueChanged -= parameterOp_ValueChanged;
                op.Dispose();
            }

            base.Dispose();
        }

    }

}
