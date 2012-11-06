using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class WhereOperation<TElement> : Operation<IEnumerable<TElement>>, IEnumerable<TElement>, INotifyCollectionChanged
    {

        Expression<Func<TElement, bool>> predicateExpr;
        IOperation<IEnumerable<TElement>> source;
        Dictionary<TElement, LambdaOperation<bool>> predicates = new Dictionary<TElement, LambdaOperation<bool>>();

        public WhereOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            var sourceExpr = expression.Arguments[0];
            predicateExpr = expression.Arguments[1] as Expression<Func<TElement, bool>>;

            // attempt to unpack from unary
            if (predicateExpr == null)
            {
                var unaryExpr = expression.Arguments[1] as UnaryExpression;
                if (unaryExpr != null)
                    predicateExpr = unaryExpr.Operand as Expression<Func<TElement, bool>>;
            }

            if (sourceExpr != null)
            {
                source = (IOperation<IEnumerable<TElement>>)OperationFactory.FromExpression(context, sourceExpr);
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

            // iterate all new items
            source.Value.Select(i => GetPredicateValue(i)).ToList();

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
                    // iterate all new items
                    source.Value.Select(i => GetPredicateValue(i)).ToList();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems.Cast<TElement>().Any(i => GetPredicateValue(i)))
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Remove:

                    // remove cached predicates
                    foreach (TElement item in args.OldItems)
                    {
                        var predicate = GetPredicate(item);
                        predicate.ValueChanged -= predicate_ValueChanged;
                        predicate.Dispose();
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

        private LambdaOperation<bool> GetPredicate(TElement item)
        {
            LambdaOperation<bool> predicate;
            if (!predicates.TryGetValue(item, out predicate))
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = new ValueOperation<TElement>(item);
                var.Init();
                ctx.Variables[predicateExpr.Parameters[0].Name] = var;

                // create new predicate and subscribe to modifications
                predicate = new LambdaOperation<bool>(ctx, predicateExpr);
                predicate.Tag = item;
                predicate.Init(); // load before value changed to prevent double notification
                predicate.ValueChanged += predicate_ValueChanged;
                predicates[item] = predicate;
            }

            return predicate;
        }

        private bool GetPredicateValue(TElement item)
        {
            return GetPredicate(item).Value;
        }

        IEnumerator<TElement> GetEnumerator()
        {
            return source.Value.Where(i => GetPredicateValue(i)).GetEnumerator();
        }

        public override void Init()
        {
            if (source != null)
                source.Init();
            base.Init();

            SetValue(this);
        }

        public override void Dispose()
        {
            if (source != null)
            {
                source.ValueChanged -= source_ValueChanged;
                source.Dispose();
                var sourceValue = source.Value as INotifyCollectionChanged;
                if (sourceValue != null)
                    sourceValue.CollectionChanged -= source_CollectionChanged;
            }
            foreach (var predicate in predicates.Values)
            {
                predicate.ValueChanged -= predicate_ValueChanged;
                predicate.Dispose();
                foreach (var var in predicate.Context.Variables)
                    var.Value.Dispose();
            }
            predicates = null;

            base.Dispose();
        }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
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
