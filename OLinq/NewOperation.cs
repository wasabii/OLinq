using System;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class NewOperation<T> : Operation<T>
    {

        IOperation[] arguments;

        public NewOperation(OperationContext context, NewExpression expression)
            : base(context, expression)
        {
            arguments = new IOperation[expression.Arguments.Count];
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                arguments[i] = OperationFactory.FromExpression(context, expression.Arguments[i]);
                arguments[i].ValueChanged += argument_ValueChanged;
            }
        }

        /// <summary>
        /// Invoked when the value of one of the arguments changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void argument_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (IsLoaded)
                Refresh();
        }

        void Refresh()
        {
            var args = new object[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
                args[i] = arguments[i].Value;

            SetValue((T)((NewExpression)Expression).Constructor.Invoke(args));
        }

        public override void Load()
        {
            foreach (var arg in arguments)
                arg.Load();
            base.Load();

            Refresh();
        }

    }

}
