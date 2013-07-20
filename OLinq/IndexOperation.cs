using System.Linq.Expressions;

namespace OLinq
{

    class IndexOperation<T> : Operation<T>
    {

        IOperation targetOp;
        IOperation[] parameterOps;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public IndexOperation(OperationContext context, IndexExpression expression)
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

            Reset();
        }

        /// <summary>
        /// Invoked when the value of Object is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void targetOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Reset();
        }

        /// <summary>
        /// Invoked when the value of an argument is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void parameterOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Reset();
        }

        /// <summary>
        /// Invokes the method and saves the result.
        /// </summary>
        void Reset()
        {
            var args = new object[parameterOps.Length];
            for (int i = 0; i < parameterOps.Length; i++)
                args[i] = parameterOps[i].Value;

            SetValue(GetValue(targetOp != null ? targetOp.Value : null, args));
        }

        /// <summary>
        /// Gets the value of the indexer for the target object and the set of parameters.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T GetValue(object target, params object[] parameters)
        {
            var property = ((IndexExpression)Expression).Indexer;
            if (target == null)
                // null target of non-static method should not fail, but simply return default value
                return default(T);
            else
                // invoke instance method
                return (T)property.GetValue(target, parameters);
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
