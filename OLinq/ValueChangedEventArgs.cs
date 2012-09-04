namespace OLinq
{

    public class ValueChangedEventArgs
    {

        internal ValueChangedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

    }

}
