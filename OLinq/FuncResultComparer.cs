using System.Collections.Generic;

namespace OLinq
{

    /// <summary>
    /// An <see cref="IComparer`1"/> implementation that compares the values returned by func operations.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    class FuncResultComparer<TResult> :
        IComparer<FuncOperation<TResult>>
    {
        
        /// <summary>
        /// Default value type comparer.
        /// </summary>
        Comparer<TResult> comparer = Comparer<TResult>.Default;

        public int Compare(FuncOperation<TResult> x, FuncOperation<TResult> y)
        {
            return comparer.Compare(x.Value, y.Value);
        }

    }

}
