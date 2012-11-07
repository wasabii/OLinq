using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class CountOperation<TSource> : GroupOperation<TSource, int>
    {

        int count = 0;

        public CountOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override void SourceCollectionReset()
        {
            base.SourceCollectionReset();
            SetValue(count = SourceCollection.Count());
        }

        protected override void SourceCollectionAddItem(TSource item, int index)
        {
            base.SourceCollectionAddItem(item, index);
            SetValue(++count);
        }

        protected override void SourceCollectionRemoveItem(TSource item, int index)
        {
            base.SourceCollectionRemoveItem(item, index);
            SetValue(--count);
        }

    }

}
