using System;

namespace OLinq
{

    public class PropertyChangingEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="propertyName"></param>
        public PropertyChangingEventArgs(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get;private set; }

    }

}
