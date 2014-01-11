using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace OLinq
{

    /// <summary>
    /// Provides information to an operation and its children.
    /// </summary>
    class OperationContext
    {

        OperationContext parent;
        Dictionary<string, IOperation> variables;
        bool? nullSafe;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal OperationContext()
        {
            this.variables = new Dictionary<string, IOperation>();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        internal OperationContext(OperationContext parent)
            : this()
        {
            this.parent = parent;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        internal OperationContext(OperationContext parent, bool? nullSafe = null)
            : this(parent)
        {
            this.nullSafe = nullSafe;
        }

        /// <summary>
        /// Gets the parent <see cref="OperationContext"/>.
        /// </summary>
        public OperationContext Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets the set of variables defined in scope.
        /// </summary>
        public Dictionary<string, IOperation> Variables
        {
            get { return variables; }
        }

        /// <summary>
        /// Gets the operation for the variable of the specified name if available, otherwise checks the parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IOperation<T> GetVariable<T>(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(name));

            IOperation node;
            if (!variables.TryGetValue(name, out node))
                if (parent != null)
                    node = parent.GetVariable<T>(name);

            return (IOperation<T>)node;
        }

        /// <summary>
        /// Gets whether the operation should be null-safe.
        /// </summary>
        public bool IsNullSafe
        {
            get { return GetIsNullSafe(); }
        }

        /// <summary>
        /// Implements the getter for IsNullSafe.
        /// </summary>
        /// <returns></returns>
        bool GetIsNullSafe()
        {
            bool? v;
            if ((v = nullSafe) == null)
                if (parent != null)
                    v = parent.IsNullSafe;

            return v ?? false;
        }

    }

}
