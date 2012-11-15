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
            newOp = OperationFactory.FromExpression<T>(context, expression.NewExpression);
            newOp.ValueChanged += newOp_ValueChanged;

            foreach (var binding in expression.Bindings)
            {
                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment != null)
                {
                    var op = OperationFactory.FromExpression(Context, memberAssignment.Expression);
                    op.Tag = memberAssignment;
                    op.ValueChanged += memberAssignmentOp_ValueChanged;
                    memberAssignmentOps.Add(op);
                    SetAssignment(newOp.Value, op);
                }

                var list = binding as MemberListBinding;
                if (list != null)
                    throw new NotImplementedException();

                var member = binding as MemberMemberBinding;
                if (member != null)
                    throw new NotImplementedException();
            }

            SetValue(newOp.Value);
        }

        /// <summary>
        /// Invoked when the result of the Object operation is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void newOp_ValueChanged(object sender, ValueChangedEventArgs args)
        {
            // set assignments onto new object before setting as value to avoid multiple events
            var value = newOp.Value;
            if (value != null)
                SetAssignments(value);

            // set as value
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

        /// <summary>
        /// Sets all of the member assignment operation's values onto the target.
        /// </summary>
        /// <param name="target"></param>
        void SetAssignments(object target)
        {
            foreach (var op in memberAssignmentOps)
                SetAssignment(target, op);
        }

        /// <summary>
        /// Sets the given assignment operation's value onto the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="op"></param>
        void SetAssignment(object target, IOperation op)
        {
            var assignment = (MemberAssignment)op.Tag;

            var prevValue = GetMemberValue(target, assignment.Member);
            var currValue = op.Value;

            if (!object.Equals(prevValue, currValue))
                SetMemberValue(target, assignment.Member, currValue);
        }

        /// <summary>
        /// Gets the value of the specified member on <paramref name="target"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        object GetMemberValue(object target, MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(target, null);

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(target);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the value of the specified member on <paramref name="target"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="memberInfo"></param>
        /// <param name="value"></param>
        void SetMemberValue(object target, MemberInfo memberInfo, object value)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(target, value, null);
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

        public override void Dispose()
        {
            newOp.ValueChanged -= newOp_ValueChanged;
            newOp.Dispose();

            foreach (var op in memberAssignmentOps)
            {
                op.ValueChanged -= memberAssignmentOp_ValueChanged;
                op.Dispose();
            }

            base.Dispose();
        }

    }

}
