using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLinq
{
    class CountingEnumerator<T> : IEnumerator<T>
    {
        public CountingEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }
        
        private readonly IEnumerator<T> _inner;
        
        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}
