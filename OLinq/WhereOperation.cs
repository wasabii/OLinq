using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class WhereOperation<TElement> : EnumerableSourceWithPredicateOperation<TElement, IEnumerable<TElement>>, IEnumerable<TElement>, INotifyCollectionChanged
    {

        public WhereOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TElement, bool>(1))
        {
            SetValue(this);
        }

        protected override void OnPredicateCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnPredicateValueChanged(LambdaValueChangedEventArgs<TElement, bool> args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Predicates.Where(i => i.Value).Select(i => Predicates[i]).GetEnumerator();
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
