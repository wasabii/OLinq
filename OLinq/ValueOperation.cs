namespace OLinq
{

    class ValueOperation<T> : Operation<T>
    {

        public ValueOperation(T value)
            : base(null, null)
        {
            Value = value;
        }

        public override void Load()
        {
            base.Load();
            OnValueChanged(null, Value);
        }

    }

}
