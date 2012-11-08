using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    class SelectManyOperation<TSource, TResult> : SingleEnumerableSourceOperation<TSource, IEnumerable<TResult>>, IEnumerable<TResult>, INotifyCollectionChanged
    {

        LambdaOperationContainer<TSource, IEnumerable<TResult>> selectors;

        public SelectManyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression)
        {
            selectors = new LambdaOperationContainer<TSource, IEnumerable<TResult>>(
                ((MethodCallExpression)Expression).GetArgument(1).UnpackLambda<TSource, IEnumerable<TResult>>(),
                CreatePredicateContext);
            selectors.CollectionChanged += selectors_CollectionChanged;
            selectors.LambdaValueChanged += selectors_LambdaValueChanged;
            selectors.Items = SourceCollection;

            SetValue(this);
        }

        /// <summary>
        /// Creates a context for a new predicate operation.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        OperationContext CreatePredicateContext(TSource item)
        {
            // generate new parameter
            var ctx = new OperationContext(Context);
            var var = OperationFactory.FromValue(item);
            ctx.Variables[selectors.Expression.Parameters[0].Name] = var;
            return ctx;
        }

        void selectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
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
        void selectors_LambdaValueChanged(object sender, LambdaValueChangedEventArgs<TSource, IEnumerable<TResult>> args)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void SourceChanged(IEnumerable<TSource> oldSource, IEnumerable<TSource> newSource)
        {
            base.SourceChanged(oldSource, newSource);

            selectors.Items = SourceCollection;
        }

        IEnumerator<TResult> GetEnumerator()
        {
            return selectors.SelectMany(i => i.Value).GetEnumerator();
        }

        public override void Init()
        {
            base.Init();

            OnValueChanged(null, Value);
        }

        public override void Dispose()
        {
            base.Dispose();
            selectors.Dispose();
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
