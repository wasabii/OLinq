namespace OLinq
{

    class LambdaValueChangedEventArgs<TSource, TResult>
    {

        internal LambdaValueChangedEventArgs(TSource item, LambdaOperation<TResult> operation, TResult oldValue, TResult newValue)
        {
            Item = item;
            Operation = operation;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public TSource Item { get; private set; }

        public LambdaOperation<TResult> Operation { get; private set; }

        public TResult OldValue { get; private set; }

        public TResult NewValue { get; private set; }

    }

}
