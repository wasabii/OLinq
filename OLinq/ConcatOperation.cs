using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class ConcatOperation<T> : Operation<IEnumerable<T>>, IEnumerable<T>, INotifyCollectionChanged
    {

        private Expression source1Expr;
        private Expression source2Expr;

        private IOperation<IEnumerable<T>> source1;
        private IOperation<IEnumerable<T>> source2;

        public ConcatOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            source1Expr = expression.Arguments[0];
            source2Expr = expression.Arguments[1];

            if (source1Expr != null)
            {
                source1 = OperationFactory.FromExpression<IEnumerable<T>>(Context, source1Expr);
                source1.ValueChanged += source1_ValueChanged;
            }

            if (source2Expr != null)
            {
                source2 = OperationFactory.FromExpression<IEnumerable<T>>(Context, source2Expr);
                source2.ValueChanged += source2_ValueChanged;
            }
        }

        void source1_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= source1_CollectionChanged;

            var newValue = args.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += source1_CollectionChanged;

            if (IsLoaded)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        void source2_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= source1_CollectionChanged;

            var newValue = args.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += source1_CollectionChanged;

            if (IsLoaded)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        void source1_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            sourceValue_CollectionChanged(args);
        }

        void source2_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            sourceValue_CollectionChanged(args);
        }

        void sourceValue_CollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems.Cast<T>().Any())
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (args.OldItems.Cast<T>().Any())
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Concat(source1.Value, source2.Value).GetEnumerator();
        }

        public override void Load()
        {
            source1.Load();
            source2.Load();
            base.Load();

            SetValue(this);
        }

        public override void Dispose()
        {
            if (source1 != null)
            {
                var source1value = source1.Value as INotifyCollectionChanged;
                source1.ValueChanged -= source1_ValueChanged;
                source1.Dispose();
                if (source1value != null)
                    source1value.CollectionChanged -= source1_CollectionChanged;
            }
            if (source2 != null)
            {
                var source2value = source2.Value as INotifyCollectionChanged;
                source2.ValueChanged -= source2_ValueChanged;
                source2.Dispose();
                if (source2value != null)
                    source2value.CollectionChanged -= source2_CollectionChanged;
            }

            base.Dispose();
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
