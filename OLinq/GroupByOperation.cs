using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class GroupByOperation<TElement, TKey> : Operation<IEnumerable<IGrouping<TKey, TElement>>>, IEnumerable<IGrouping<TKey, TElement>>, INotifyCollectionChanged
    {

        Expression sourceExpr;
        Expression<Func<TElement, TKey>> keyFuncExpr;
        IOperation<IEnumerable<TElement>> source;
        Dictionary<TElement, LambdaOperation<TKey>> keyFuncs = new Dictionary<TElement, LambdaOperation<TKey>>();

        public GroupByOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            sourceExpr = expression.Arguments[0];
            keyFuncExpr = expression.Arguments[1] as Expression<Func<TElement, TKey>>;

            // attempt to unpack from unary
            if (keyFuncExpr == null)
            {
                var unaryExpr = expression.Arguments[1] as UnaryExpression;
                if (unaryExpr != null)
                    keyFuncExpr = unaryExpr.Operand as Expression<Func<TElement, TKey>>;
            }

            source = OperationFactory.FromExpression<IEnumerable<TElement>>(context, sourceExpr);
            source.ValueChanged += source_ValueChanged;
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

        void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        /// <summary>
        /// Invoked when any of the current keys change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void func_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = (TKey)args.OldValue;
            var newValue = (TKey)args.NewValue;

            // store new test result
            if (!object.Equals(oldValue, newValue))
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private LambdaOperation<TKey> GetKeyFunc(TElement item)
        {
            LambdaOperation<TKey> keyFunc;
            if (!keyFuncs.TryGetValue(item, out keyFunc))
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = new ValueOperation<TElement>(item);
                ctx.Variables[keyFuncExpr.Parameters[0].Name] = var;

                // create new test and subscribe to test modifications
                keyFunc = new LambdaOperation<TKey>(ctx, keyFuncExpr);
                keyFunc.Tag = item;
                keyFunc.Init(); // load before value changed to prevent double notification
                keyFunc.ValueChanged += func_ValueChanged;
                keyFuncs[item] = keyFunc;
            }

            return keyFunc;
        }

        private TKey GetKeyFuncResult(TElement item)
        {
            return GetKeyFunc(item).Value;
        }

        IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            return source.Value.GroupBy(i => GetKeyFuncResult(i)).GetEnumerator();
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
            foreach (var keyFunc in keyFuncs.Values)
            {
                keyFunc.ValueChanged -= func_ValueChanged;
                keyFunc.Dispose();
                foreach (var var in keyFunc.Context.Variables)
                    var.Value.Dispose();
            }
            keyFuncs = null;

            base.Dispose();
        }

        IEnumerator<IGrouping<TKey, TElement>> IEnumerable<IGrouping<TKey, TElement>>.GetEnumerator()
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
