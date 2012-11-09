using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class GroupByOperation<TElement, TKey> : EnumerableSourceWithLambdaOperation<TElement, TKey, IEnumerable<IGrouping<TKey, TElement>>>, IEnumerable<IGrouping<TKey, TElement>>, INotifyCollectionChanged
    {

        public GroupByOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TElement, TKey>(1))
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TElement, TKey> args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return Lambdas.GroupBy(i => i.Value, i => Lambdas[i]).GetEnumerator();
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
