namespace OLinq
{

    class ValueOperation<T> : Operation<T>
    {

        public ValueOperation(T value)
            : base(null, null)
        {
            SetValue(value);
        }

    }

}
