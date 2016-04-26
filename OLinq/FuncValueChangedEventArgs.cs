namespace OLinq
{

    class FuncValueChangedEventArgs<TSource, TResult>
    {

        internal FuncValueChangedEventArgs(TSource item, FuncOperation<TResult> operation, TResult oldValue, TResult newValue)
        {
            Item = item;
            Operation = operation;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public TSource Item { get; private set; }

        public FuncOperation<TResult> Operation { get; private set; }

        public TResult OldValue { get; private set; }

        public TResult NewValue { get; private set; }

    }

}
