using System;
using System.Collections.Generic;
using System.Text;

namespace BRCDAQdemo.WPF.Core
{
    public class MemoryReadOnlyListAdapter<T> : IReadOnlyList<T>
    {
        private readonly Memory<T> _memory;

        public MemoryReadOnlyListAdapter(Memory<T> memory)
        {
            _memory = memory;
        }

        public T this[int index] => _memory.Span[index];

        public int Count => _memory.Length;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _memory.Length; i++)
            {
                yield return _memory.Span[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
