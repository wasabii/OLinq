using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class DefaultIfEmptyOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            return Operation.CreateMethodCallOperation(typeof(DefaultIfEmptyOperation<>), context, expression, 0);
        }

    }

    class DefaultIfEmptyOperation<TElement> : EnumerableSourceOperation<TElement, IEnumerable<TElement>>
    {

        IOperation<TElement> defaultValueOp;
        ObservableCollection<TElement> defaultValues = new ObservableCollection<TElement>()
        {
            default(TElement),
        };

        public DefaultIfEmptyOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0])
        {
            if (expression.Arguments.Count >= 2)
            {
                defaultValueOp = OperationFactory.FromExpression<TElement>(context, expression.Arguments[1]);
                defaultValueOp.ValueChanged += defaultOp_ValueChanged;
            }

            Evaluate();
        }

        /// <summary>
        /// Invoked when the default value operation changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void defaultOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            Evaluate();
        }

        /// <summary>
        /// Evaluates whether the default state has changed, and if so, raises a collection changed notification.
        /// </summary>
        void Evaluate()
        {
            // determine default value
            var defaultValue = defaultValueOp != null ? defaultValueOp.Value : default(TElement);

            // value of default operation has changed
            if (!object.Equals(defaultValues[0], defaultValue))
                // change known default set value
                defaultValues[0] = defaultValue;

            SetValue(Source.Any() ? Source : defaultValues);
        }

        protected override void OnSourceCollectionReset()
        {
            Evaluate();
        }

        protected override void OnSourceCollectionItemsAdded(IEnumerable<TElement> newItems, int startingIndex)
        {
            Evaluate();
        }

        protected override void OnSourceCollectionItemsRemoved(IEnumerable<TElement> oldItems, int startingIndex)
        {
            Evaluate();
        }

    }

}
