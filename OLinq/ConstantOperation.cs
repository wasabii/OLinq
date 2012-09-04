using System.Linq.Expressions;

namespace OLinq
{

    class ConstantOperation<T> : Operation<T>
    {

        public ConstantOperation(OperationContext context, ConstantExpression expression)
            : base(context, expression)
        {

        }

        public override void Load()
        {
            SetValue((T)((ConstantExpression)Expression).Value);
            base.Load();
        }

    }

}
