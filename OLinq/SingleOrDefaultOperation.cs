using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SingleOrDefaultOperation<T> : GroupOperation<T,T>
    {

        public SingleOrDefaultOperation(OperationContext context, MethodCallExpression expression)
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
            return SetValue(SourceCollection.SingleOrDefault());
        }

    }

}
