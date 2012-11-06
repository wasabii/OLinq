using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectManyOperation<TSource, TResult> : Operation<IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        Expression sourceExpr;
        Expression<Func<TSource, IEnumerable<TResult>>> lambdaExpr;
        IOperation<IEnumerable<TSource>> source;
        Dictionary<TSource, LambdaOperation<IEnumerable<TResult>>> funcs = new Dictionary<TSource, LambdaOperation<IEnumerable<TResult>>>();

        public SelectManyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            sourceExpr = expression.Arguments[0];
            lambdaExpr = expression.Arguments[1] as Expression<Func<TSource, IEnumerable<TResult>>>;

            // attempt to unpack from unary
            if (lambdaExpr == null)
            {
                var unaryExpr = expression.Arguments[1] as UnaryExpression;
                if (unaryExpr != null)
                    lambdaExpr = unaryExpr.Operand as Expression<Func<TSource, IEnumerable<TResult>>>;
            }

            source = OperationFactory.FromExpression<IEnumerable<TSource>>(context, sourceExpr);
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

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
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
        /// Invoked when any of the current tests change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void func_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                oldValue.CollectionChanged -= func_CollectionChanged;

            var newValue = args.NewValue as INotifyCollectionChanged;
            if (newValue != null)
                newValue.CollectionChanged += func_CollectionChanged;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        void func_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // TODO suscribe to new items, unsubscribe from old, raise proper event
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private LambdaOperation<IEnumerable<TResult>> GetFunc(TSource item)
        {
            LambdaOperation<IEnumerable<TResult>> func;
            if (!funcs.TryGetValue(item, out func))
            {
                // generate new parameter
                var ctx = new OperationContext(Context);
                var var = new ValueOperation<TSource>(item);
                ctx.Variables[lambdaExpr.Parameters[0].Name] = var;

                // create new test and subscribe to test modifications
                func = new LambdaOperation<IEnumerable<TResult>>(ctx, lambdaExpr);
                func.Tag = item;
                func.Load(); // load before value changed to prevent double notification
                func.ValueChanged += func_ValueChanged;
                funcs[item] = func;
            }

            return func;
        }

        private IEnumerable<TResult> GetFuncResult(TSource item)
        {
            return GetFunc(item).Value;
        }

        IEnumerator<TResult> GetEnumerator()
        {
            return source.Value.SelectMany(i => GetFuncResult(i)).GetEnumerator();
        }

        public override void Load()
        {
            if (source != null)
                source.Load();
            base.Load();

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
            foreach (var func in funcs.Values)
            {
                func.ValueChanged -= func_ValueChanged;
                func.Dispose();
                foreach (var var in func.Context.Variables)
                    var.Value.Dispose();
            }
            funcs = null;

            base.Dispose();
        }

        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
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
