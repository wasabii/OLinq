using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectManyOperation<TSource, TResult> : EnumerableSourceWithLambdaOperation<TSource, IEnumerable<TResult>, IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        public SelectManyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TSource, IEnumerable<TResult>>(1))
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move));
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    var newItems = args.NewItems.Cast<LambdaOperation<IEnumerable<TResult>>>().SelectMany(i => i.Value).ToList();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItems = args.OldItems.Cast<LambdaOperation<IEnumerable<TResult>>>().SelectMany(i => i.Value).ToList();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems));
                    break;
            }
        }

        protected override void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, IEnumerable<TResult>> args)
        {
            var oldValues = args.OldValue.Except(args.NewValue).ToList();
            var newValues = args.NewValue.Except(args.OldValue).ToList();
            if (oldValues.Count == 0 && newValues.Count >= 1)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newValues));
            else if (oldValues.Count >= 1 && newValues.Count == 0)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValues));
            else if (oldValues.Count >= 1 && newValues.Count >= 1)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldValues, newValues));
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
