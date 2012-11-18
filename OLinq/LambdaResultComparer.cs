using System.Collections.Generic;

namespace OLinq
{

    /// <summary>
    /// An <see cref="IComparer`1"/> implementation that compares the values returned by lambda operations.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    class LambdaResultComparer<TResult> : IComparer<LambdaOperation<TResult>>
    {
        
        /// <summary>
        /// Default value type comparer.
        /// </summary>
        Comparer<TResult> comparer = Comparer<TResult>.Default;

        public int Compare(LambdaOperation<TResult> x, LambdaOperation<TResult> y)
        {
            return comparer.Compare(x.Value, y.Value);
        }

    }

}
