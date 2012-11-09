using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class DistinctOperation<TElement> : EnumerableSourceOperation<TElement, IEnumerable<TElement>>, IEnumerable<TElement>, INotifyCollectionChanged
    {

        public DistinctOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            SetValue(this);
        }

        public override void Init()
        {
            base.Init();

            OnValueChanged(null, Value);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Source.Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
