using System;

namespace OLinq
{

    /// <summary>
    /// Manages an Rx subscription for an <see cref="IObserver{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    class ObservableSubscription<TResult> : IDisposable
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="observer"></param>
        public ObservableSubscription(ObservableValue<TResult> value, IObserver<TResult> observer)
        {
            Value = value;
            Observer = observer;

            Value.ValueChanged += Value_ValueChanged;
        }

        /// <summary>
        /// Invoke when the value of the observed object changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Value_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Observer.OnNext((TResult)args.NewValue);
        }

        /// <summary>
        /// Value subscribed to.
        /// </summary>
        public ObservableValue<TResult> Value { get; private set; }

        /// <summary>
        /// Observer watching for changes in the value.
        /// </summary>
        public IObserver<TResult> Observer { get; private set; }

        /// <summary>
        /// Disposes of the subscription.
        /// </summary>
        public void Dispose()
        {
            Value.ValueChanged -= Value_ValueChanged;
            Observer.OnCompleted();
        }

    }

}
