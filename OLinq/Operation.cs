using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    /// <summary>
    /// Base operation type. An operation is associated with an <see cref="Expression"/> and implements its functionality.
    /// </summary>
    abstract class Operation : IDisposable
    {

        public static bool IsMethod(MethodInfo method, string name, int typeArgs, int parameters)
        {
            Contract.Requires<ArgumentNullException>(method != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(typeArgs >= 0);
            Contract.Requires<ArgumentNullException>(parameters >= 0);

            return typeof(Queryable).GetMember(name).Concat(typeof(Enumerable).GetMember(name))
                .OfType<MethodInfo>()
                .Where(i => i.IsGenericMethodDefinition)
                .Where(i => i.GetGenericArguments().Length == typeArgs)
                .Where(i => i.GetParameters().Length == parameters)
                .Where(i => i == method)
                .Any();
        }

        /// <summary>
        /// Generates a new method call operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        /// <param name="genericArgIndexes"></param>
        /// <returns></returns>
        public static IOperation CreateMethodCallOperation(Type type, OperationContext context, MethodCallExpression expression, params int[] genericArgIndexes)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Requires<ArgumentNullException>(genericArgIndexes != null);

            return (IOperation)Activator.CreateInstance(type.MakeGenericType(genericArgIndexes.Select(i => expression.Method.GetGenericArguments()[i]).ToArray()), context, expression);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        protected Operation(OperationContext context, Expression expression)
        {
            Context = context;
            Expression = expression;
        }

        /// <summary>
        /// Gets the associated <see cref="OperationContext"/>.
        /// </summary>
        public OperationContext Context { get; private set; }

        /// <summary>
        /// Gets the associated expression.
        /// </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        /// Holder for attached information.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Disposes of the operation.
        /// </summary>
        public virtual void Dispose()
        {

        }

    }

    /// <summary>
    /// Value-providing operation type.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    abstract class Operation<TResult> : Operation, IOperation<TResult>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        protected Operation(OperationContext context, Expression expression)
            : base(context, expression)
        {

        }

        /// <summary>
        /// Gets the typed value of the expression.
        /// </summary>
        public TResult Value { get; protected set; }

        /// <summary>
        /// Raised when the output value changes.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        protected void OnValueChanged(object oldValue, object newValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Sets the result value, raising appropriate events.
        /// </summary>
        /// <param name="value"></param>
        protected TResult SetValue(TResult value)
        {
            var oldValue = Value;
            var newValue = value;

            if (!object.Equals(oldValue, newValue))
            {
                Value = newValue;
                OnValueChanged(oldValue, newValue);
            }

            return newValue;
        }

        TResult IOperation<TResult>.Value
        {
            get { return Value; }
        }

        object IOperation.Value
        {
            get { return Value; }
        }

        event ValueChangedEventHandler IOperation.ValueChanged
        {
            add { ValueChanged += value; }
            remove { ValueChanged -= value; }
        }

    }

}
