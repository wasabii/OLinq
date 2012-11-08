using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectManyOperation<TSource, TResult> : SingleEnumerableLambdaSourceOperation<TSource, IEnumerable<TResult>, IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        public SelectManyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, IEnumerable<TResult>> args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public override void Init()
        {
            base.Init();

            OnValueChanged(null, Value);
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Lambdas.SelectMany(i => i.Value).GetEnumerator();
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
