using System;
using System.Linq.Expressions;

namespace OLinq
{

    class ParameterOperation<T> : Operation<T>
    {

        IOperation<T> variable;

        public ParameterOperation(OperationContext context, ParameterExpression expression)
            : base(context, expression)
        {
            variable = Context.GetVariable<T>(expression.Name);
            if (variable != null)
                variable.ValueChanged += variable_ValueChanged;
        }

        /// <summary>
        /// Invoked when the value of the referenced variable is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void variable_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            SetValue(variable.Value);
        }

        public override void Load()
        {
            if (variable != null)
                variable.Load();
            base.Load();
        }

    }

}
