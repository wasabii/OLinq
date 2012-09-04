using System;
using System.Linq.Expressions;

namespace OLinq
{

    class CallOperation<T> : Operation<T>
    {

        IOperation target;
        IOperation[] arguments;

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
                target = OperationFactory.FromExpression<object>(context, expression.Object);
                target.ValueChanged += target_ValueChanged;
            }

            arguments = new IOperation<object>[expression.Arguments.Count];
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                arguments[i] = OperationFactory.FromExpression<object>(context, expression.Arguments[i]);
                arguments[i].ValueChanged += argument_ValueChanged;
            }
        }

        /// <summary>
        /// Invoked when the value of Object is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void target_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (IsLoaded)
                Invoke();
        }

        /// <summary>
        /// Invoked when the value of an argument is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void argument_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (IsLoaded)
                Invoke();
        }

        public override void Load()
        {
            if (target != null)
                target.Load();

            foreach (var arg in arguments)
                arg.Load();

            base.Load();

            // obtain initial value
            Invoke();
        }

        protected virtual T Invoke(object target, params object[] args)
        {
            return (T)((MethodCallExpression)Expression).Method.Invoke(target, args);
        }

        /// <summary>
        /// Invokes the method and saves the result.
        /// </summary>
        void Invoke()
        {
            var args = new object[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
                args[i] = arguments[i].Value;

            SetValue(Invoke(target.Value, args));
        }

        public override void Dispose()
        {
            if (target != null)
            {
                target.ValueChanged -= target_ValueChanged;
                target.Dispose();
            }
            foreach (var arg in arguments)
            {
                arg.ValueChanged -= argument_ValueChanged;
                arg.Dispose();
            }

            base.Dispose();
        }

    }

}
