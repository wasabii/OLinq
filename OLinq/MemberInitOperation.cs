using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace OLinq
{

    class MemberInitOperation<T> : Operation<T>
    {

        IOperation<T> newOp;
        List<IOperation> memberAssignmentOps = new List<IOperation>();

        public MemberInitOperation(OperationContext context, MemberInitExpression expression)
            : base(context, expression)
        {
            if (expression.NewExpression != null)
            {
                newOp = OperationFactory.FromExpression<T>(context, expression.NewExpression);
                newOp.Init();
                newOp.ValueChanged += newOp_ValueChanged;
                SetValue(newOp.Value);
            }

            foreach (var binding in expression.Bindings)
            {
                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment != null)
                {
                    var op = OperationFactory.FromExpression(Context, memberAssignment.Expression);
                    op.Tag = memberAssignment;
                    op.Init();
                    SetAssignment(Value, op);
                    op.ValueChanged += memberAssignmentOp_ValueChanged;
                    memberAssignmentOps.Add(op);
                }

                var list = binding as MemberListBinding;
                if (list != null)
                    throw new NotImplementedException();

                var member = binding as MemberMemberBinding;
                if (member != null)
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Invoked when the result of the Object operation is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void newOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            var value = newOp.Value;
            if (value != null)
                SetAssignments(value);

            SetValue(newOp.Value);
        }

        /// <summary>
        /// Invoked when a member assignment's value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void memberAssignmentOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (newOp.Value != null)
            {
                var op = (IOperation)sender;
                var assignment = (MemberAssignment)op.Tag;
                SetAssignment(newOp.Value, op);
            }
        }

        void SetAssignment(object target, IOperation op)
        {
            var assignment = (MemberAssignment)op.Tag;

            var prevValue = GetMemberValue(target, assignment.Member);
            var currValue = op.Value;

            if (!object.Equals(prevValue, currValue))
                SetMemberValue(target, assignment.Member, currValue);
        }

        void SetAssignments(object target)
        {
            foreach (var op in memberAssignmentOps)
                SetAssignment(target, op);
        }

        object GetMemberValue(object target, MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(target);

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(target);

            throw new NotImplementedException();
        }

        void SetMemberValue(object target, MemberInfo memberInfo, object value)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(target, value);
                return;
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
                return;
            }

            throw new NotImplementedException();
        }

        public override void Init()
        {
            base.Init();

            // set up initial value
            if (newOp.Value != null)
                SetAssignments(newOp.Value);

            OnValueChanged(null, Value);
        }

        public override void Dispose()
        {
            if (newOp != null)
            {
                newOp.ValueChanged -= newOp_ValueChanged;
                newOp.Dispose();
            }
            foreach (var assignment in memberAssignmentOps)
            {
                assignment.ValueChanged -= memberAssignmentOp_ValueChanged;
                assignment.Dispose();
            }

            base.Dispose();
        }

    }

}
