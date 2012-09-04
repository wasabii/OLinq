using System;
using System.Collections.Generic;

namespace OLinq
{

    class OperationContext
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal OperationContext()
        {
            Variables = new Dictionary<string, IOperation>();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        internal OperationContext(OperationContext parent)
            : this()
        {
            Parent = parent;
        }

        public OperationContext Parent { get; private set; }

        public Dictionary<string, IOperation> Variables { get; private set; }

        public IOperation<T> GetVariable<T>(string name)
        {
            IOperation node;
            if (!Variables.TryGetValue(name, out node))
                if (Parent != null)
                    node = Parent.GetVariable<T>(name);

            return (IOperation<T>)node;
        }

    }

}
