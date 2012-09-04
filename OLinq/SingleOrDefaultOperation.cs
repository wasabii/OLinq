using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SingleOrDefaultOperation<T> : GroupOperation<T>
    {

        public SingleOrDefaultOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {

        }

        protected override void OnSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            Reset(Source);
        }

        protected override void OnSourceCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Reset(Source);
                    return;
                case NotifyCollectionChangedAction.Remove:
                    Reset(Source);
                    return;
                default:
                    Reset(Source);
                    return;
            }
        }

        T Reset(IEnumerable source)
        {
            return SetValue(Enumerable.SingleOrDefault(source as IEnumerable<T> ?? source.Cast<T>()));
        }

    }

}
