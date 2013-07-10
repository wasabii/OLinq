using System;

namespace OLinq
{

    /// <summary>
    /// Read-only access to an <see cref="Operation"/>.
    /// </summary>
    interface IOperation : IDisposable
    {

        /// <summary>
        /// Gets the context of the operation.
        /// </summary>
        OperationContext Context { get; }

        /// <summary>
        /// Gets the currently provided value of this <see cref="Operation"/>.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Raised when the value of the <see cref="Operation"/> is changed.
        /// </summary>
        event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// Holder for attached information.
        /// </summary>
        object Tag { get; set; }

    }

    /// <summary>
    /// Typed read-only access to an <see cref="Operation"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IOperation<out T> : IOperation
    {

        /// <summary>
        /// Gets the currently provided value of this <see cref="Operation"/>.
        /// </summary>
        new T Value { get; }

    }

    interface IOperation<P, out TResult> : IOperation<TResult>
    {

        IOperation<P> Arg1 { get; }

    }

    interface IOperation<P1, P2, out TResult> : IOperation<TResult>
    {

        IOperation<P1> Arg1 { get; }

        IOperation<P2> Arg2 { get; }

    }

    interface IOperation<P1, P2, P3, out TResult> : IOperation<TResult>
    {

        IOperation<P1> Arg1 { get; }

        IOperation<P2> Arg2 { get; }

        IOperation<P3> Arg3 { get; }

    }

}
