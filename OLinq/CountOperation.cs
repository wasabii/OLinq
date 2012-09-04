using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class CountOperation : GroupOperation<int>
    {

        int count = 0;

        public CountOperation(OperationContext context, MethodCallExpression expression)
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
                    SetValue(count += args.NewItems.Count);
                    return;
                case NotifyCollectionChangedAction.Remove:
                    SetValue(count -= args.OldItems.Count);
                    return;
                default:
                    Reset(Source);
                    return;
            }
        }

        int Reset(IEnumerable source)
        {
            return SetValue(count = Enumerable.Count(source as IEnumerable<object> ?? source.Cast<object>()));
        }

    }

}
