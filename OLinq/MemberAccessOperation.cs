using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    class MemberAccessOperation<T> : Operation<T>
    {

        IOperation target;

        public MemberAccessOperation(OperationContext context, MemberExpression expression)
            : base(context, expression)
        {
            if (expression.Expression != null)
            {
                target = OperationFactory.FromExpression(context, expression.Expression);
                target.ValueChanged += target_ValueChanged;
                target.Init();
                Refresh();
            }
        }

        /// <summary>
        /// Invoked when the result of the Object operation is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void target_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var oldValue = args.OldValue as INotifyPropertyChanged;
            if (oldValue != null)
                oldValue.PropertyChanged -= target_PropertyChanged;

            var newValue = args.NewValue as INotifyPropertyChanged;
            if (newValue != null)
                newValue.PropertyChanged += target_PropertyChanged;

            Refresh();
        }

        /// <summary>
        /// Invoked when a watched property is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void target_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ((MemberExpression)Expression).Member.Name)
                Refresh();
        }

        /// <summary>
        /// Sets the value from the referenced member.
        /// </summary>
        void Refresh()
        {
            var expression = (MemberExpression)Expression;
            if (expression.Member is PropertyInfo)
                SetValue((T)((PropertyInfo)expression.Member).GetValue(target.Value));
            else if (expression.Member is FieldInfo)
                SetValue((T)((FieldInfo)expression.Member).GetValue(target.Value));
            else
                throw new NotSupportedException(string.Format("MemberAccess does not support Member of type {0}.", expression.Member.MemberType));
        }

        public override void Init()
        {
            base.Init();

            OnValueChanged(null, Value);
        }

        public override void Dispose()
        {
            if (target != null)
            {
                var targetValue = target.Value as INotifyPropertyChanged;
                target.ValueChanged -= target_ValueChanged;
                target.Dispose();
                if (targetValue != null)
                    targetValue.PropertyChanged -= target_PropertyChanged;
            }


            base.Dispose();
        }

    }

}
