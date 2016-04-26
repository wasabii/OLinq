using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    /// <summary>
    /// Implements <see cref="IQueryable{T}"/>.Where.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    class WhereOperation<TElement> :
        EnumerableSourceWithPredicateOperation<TElement, IEnumerable<TElement>>, 
        IEnumerable<TElement>, 
        INotifyCollectionChanged
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public WhereOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.GetLambdaArgument<TElement, bool>(1))
        {
            SetValue(this);
        }

        protected override void OnPredicateCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
#if !SILVERLIGHT && !PCL
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    // items added to the underlying collection that currently have a true value
                    var newItems = args.NewItems.Cast<FuncOperation<bool>>().Where(i => i.Value).Select(i => Predicates[i]).ToList();
                    if (newItems.Count > 0)
                        NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(OnCollectionChanged, newItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // items removed from the underlying collection that currently have a true value
                    var oldItems = args.OldItems.Cast<FuncOperation<bool>>().Where(i => i.Value).Select(i => Predicates[i]).ToList();
                    if (oldItems.Count > 0)
                        NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(OnCollectionChanged, oldItems);
                    break;
            }
        }

        protected override void OnPredicateValueChanged(FuncValueChangedEventArgs<TElement, bool> args)
        {
            // value is now true, was false
            if (args.NewValue && !args.OldValue)
            {
                // TODO resolve indexes
                NotifyCollectionChangedUtil.RaiseAddEvent<TElement>(OnCollectionChanged, args.Item);
                return;
            }

            // values was true, now false
            if (args.OldValue && !args.NewValue)
            {
                // TODO resolve indexes
                NotifyCollectionChangedUtil.RaiseRemoveEvent<TElement>(OnCollectionChanged, args.Item);
                return;
            }
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
