using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Benchmarks.Metrics
{
    class MetricsCollection<T> : IEnumerable<T>
    {
        private readonly List<T>[] _collection = new List<T>[50];

        public int Count => _collection.Sum(x => x.Count);

        public MetricsCollection()
        {
            for (int i = 0; i < _collection.Length; i++)
            {
                _collection[i] = new List<T>();
            }
        }

        public void Add(T value) => _collection[Thread.CurrentThread.ManagedThreadId].Add(value);

        public void Clear()
        {
            foreach (var list in _collection)
            {
                list.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator() 
            => _collection.SelectMany(x => x).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();
    }
}
