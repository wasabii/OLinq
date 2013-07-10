using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            switch (args.Action)
            {
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    var newItems = args.NewItems.Cast<LambdaOperation<bool>>().Where(i => i.Value).Select(i => Predicates[i]).ToList();
                    if (newItems.Count > 0)
                        NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(OnCollectionChanged, newItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItems = args.OldItems.Cast<LambdaOperation<bool>>().Where(i => i.Value).Select(i => Predicates[i]).ToList();
                    if (oldItems.Count > 0)
                        NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(OnCollectionChanged, oldItems);
                    break;
            }
        }

        protected override void OnPredicateValueChanged(LambdaValueChangedEventArgs<TElement, bool> args)
        {
            if (!args.OldValue && args.NewValue)
                // was false, now true: item added
                NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(OnCollectionChanged, args.Item);
            else if (args.OldValue && !args.NewValue)
                // was true, now false: item removed
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(OnCollectionChanged, args.Item);;
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

        void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

    }

}
