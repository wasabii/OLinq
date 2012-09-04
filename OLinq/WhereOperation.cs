using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class WhereOperation<T> : Operation<IEnumerable<T>>, IEnumerable<T>, INotifyCollectionChanged
    {

        Expression<Func<T, bool>> predicateExpr;
        IOperation<IEnumerable<T>> source;
        Dictionary<T, LambdaOperation<bool>> predicates = new Dictionary<T, LambdaOperation<bool>>();

        public WhereOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            var sourceExpr = expression.Arguments[0];
            predicateExpr = expression.Arguments[1] as Expression<Func<T, bool>>;

            // attempt to unpack from unary
            if (predicateExpr == null)
            {
                var unaryExpr = expression.Arguments[1] as UnaryExpression;
                if (unaryExpr != null)
                    predicateExpr = unaryExpr.Operand as Expression<Func<T, bool>>;
            }

            if (sourceExpr != null)
            {
                source = (IOperation<IEnumerable<T>>)OperationFactory.FromExpression(context, sourceExpr);
                source.ValueChanged += source_ValueChanged;
            }
        }

        void source_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= source_CollectionChanged;

            var newValue = args.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += source_CollectionChanged;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Invoked when the source collection is modified.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems.Cast<T>().Any(i => GetPredicateValue(i)))
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Remove:

                    // remove cached predicates
                    foreach (T item in args.OldItems)
                    {
                        var predicate = GetPredicate(item);
                        predicate.ValueChanged -= predicate_ValueChanged;
                        predicates.Remove(item);
                    }

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        /// <summary>
        /// Invoked when any of the current tests change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void predicate_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = (bool)args.OldValue;
            var newValue = (bool)args.NewValue;

            if (oldValue != newValue)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private LambdaOperation<bool> GetPredicate(T item)
        {
            LambdaOperation<bool> predicate;
            if (!predicates.TryGetValue(item, out predicate))
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = new ValueOperation<T>(item);
                var.Load();
                ctx.Variables[predicateExpr.Parameters[0].Name] = var;

                // create new predicate and subscribe to modifications
                predicate = new LambdaOperation<bool>(ctx, predicateExpr);
                predicate.Tag = item;
                predicate.Load(); // load before value changed to prevent double notification
                predicate.ValueChanged += predicate_ValueChanged;
                predicates[item] = predicate;
            }

            return predicate;
        }

        private bool GetPredicateValue(T item)
        {
            return GetPredicate(item).Value;
        }

        IEnumerator<T> GetEnumerator()
        {
            return source.Value.Where(i => GetPredicateValue(i)).GetEnumerator();
        }

        public override void Load()
        {
            if (source != null)
                source.Load();
            base.Load();

            SetValue(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
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
