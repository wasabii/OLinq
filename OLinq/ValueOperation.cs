namespace OLinq
{

    class ValueOperation<T> : Operation<T>
    {

        public ValueOperation(T value)
            : base(null, null)
        {
            Value = value;
        }

        public override void Init()
        {
            base.Init();
            OnValueChanged(null, Value);
        }

    }

}
