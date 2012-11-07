using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SingleOperation<T> : GroupOperation<T, T>
    {

        public SingleOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            Reset();
        }

        protected override void SourceCollectionAddItem(T item, int index)
        {
            base.SourceCollectionAddItem(item, index);
            Reset();
        }

        protected override void SourceCollectionRemoveItem(T item, int index)
        {
            base.SourceCollectionRemoveItem(item, index);
            Reset();
        }

        T Reset()
        {
            return SetValue(SourceCollection.Single());
        }

    }

}
