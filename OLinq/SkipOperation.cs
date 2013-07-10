using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{
#if DEBUG //throw exception in production as not implemented
    class SkipOperation<TElement> : EnumerableSourceOperation<TElement, IEnumerable<TElement>>, IEnumerable<TElement>, INotifyCollectionChanged
    {
        public SkipOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {
            SetValue(this);
        }

        protected override void OnSourceCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TElement> newItems, int startingIndex)
        {
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TElement> oldItems, int startingIndex)
        {
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Source.GetEnumerator();
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
#endif
}