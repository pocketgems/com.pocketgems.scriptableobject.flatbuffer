using System;
using System.Collections;
using System.Collections.Generic;

namespace PocketGems.Parameters.Types
{
    public class ReadOnlyListContainer<T> : IReadOnlyList<T>
    {
        private int _cachedCount = -1;
        private readonly Func<int> _countFunc;
        private readonly Func<int, T> _getterFunc;

        public ReadOnlyListContainer(Func<int> countFunc, Func<int, T> getterFunc)
        {
            _countFunc = countFunc;
            _getterFunc = getterFunc;
        }

        public T this[int index] => _getterFunc(index);

        public int Count
        {
            get
            {
                if (_cachedCount < 0)
                    _cachedCount = _countFunc.Invoke();
                return _cachedCount;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => new FlatBufferArrayWrapperEnumerator<T>(Count, _getterFunc);

        private class FlatBufferArrayWrapperEnumerator<G> : IEnumerator<G>
        {
            private int _currentIndex;
            private readonly int _count;
            private readonly Func<int, G> _getterFunc;

            public FlatBufferArrayWrapperEnumerator(int count, Func<int, G> getterFunc)
            {
                _currentIndex = -1;
                _count = count;
                _getterFunc = getterFunc;
            }

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _count;
            }

            public void Reset() => _currentIndex = -1;

            object IEnumerator.Current => Current;

            public G Current => _getterFunc(_currentIndex);

            public void Dispose()
            {
            }
        }
    }
}
