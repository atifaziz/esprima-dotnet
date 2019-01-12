using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Esprima.JitHelper;

namespace Esprima.Ast
{
    // This structure is like List<> from the BCL except it is designed
    // to be modifiable by this library during the construction of the AST
    // but publicly read-only thereafter. The only allocation required on the
    // heap is the backing array storage for the element. An empty list,
    // however causes no heap allocation; that is, the array is allocated
    // on first addition.

    public struct List<T> : IReadOnlyList<T>
    {
        private static readonly T[] _emptyArray = new T[0];

        private T[] _items;
        private int _count;

        internal List(int capacity)
        {
            _items = capacity == 0 ? _emptyArray : new T[capacity];
            _count = 0;
        }

        public List(List<T> list) : this()
        {
            if (list._count <= 0)
            {
                return;
            }

            _items = new T[list._count];
            list._items.CopyTo(_items, 0);
            _count = list._count;
        }

        private int Capacity
        {
            set
            {
                Debug.Assert(value >= _count);
                _items = _items ?? (_items = _emptyArray);
                if (value == _items.Length)
                {
                    return;
                }
                if (value > 0)
                {
                    T[] newArray = new T[value];
                    if (_count > 0)
                    {
                        Array.Copy(_items, 0, newArray, 0, _count);
                    }
                    _items = newArray;
                }
                else
                {
                    _items = _emptyArray;
                }
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        internal List<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : class
        {
            _items = _items ?? (_items = _emptyArray);

            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var list = new List<TResult>
            {
                _count = _count,
                _items = new TResult[_count]
            };

            for (var i = 0; i < _count; i++)
            {
                list._items[i] = selector(_items[i]);
            }

            return list;
        }

        internal void AddRange<TSource>(List<TSource> list)
        {
            var count = list._count;
            if (count == 0)
            {
                return;
            }

            EnsureCapacity(_count + count);
            Array.Copy(list._items, 0, _items, _count, count);
            _count += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(T item)
        {
            var items = _items ?? (_items = _emptyArray);
            var count = _count;
            if ((uint) count < (uint) items.Length)
            {
                _count = count + 1;
                items[count] = item;
            }
            else
            {
                AddWithResize(item);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddWithResize(T item)
        {
            int count = _count;
            EnsureCapacity(count + 1);
            _items[_count++] = item;
        }

        private void EnsureCapacity(int min)
        {
            _items = _items ?? (_items = _emptyArray);

            if (_items.Length >= min)
            {
                return;
            }
            int num = _items.Length == 0 ? 4 : _items.Length * 2;
            if (num < min)
            {
                num = min;
            }
            Capacity = num;
        }

        internal void RemoveAt(int index)
        {
            Debug.Assert(index >= 0 && index < _count);

            _items[index] = default;
            _count--;

            if (index == _count)
            {
                return;
            }

            Array.Copy(_items, index + 1, _items, index, _count - index);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint) index >= (uint) _count)
                {
                    Throw<ArgumentOutOfRangeException>();
                }
                return _items[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => _items[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Push(T item) => Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Pop()
        {
            var lastIndex = _count - 1;
            var last = this[lastIndex];
            RemoveAt(lastIndex);
            return last;
        }

        public Enumerator GetEnumerator() => new Enumerator(_items ?? _emptyArray, _count);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <remarks>
        /// This implementation does not detect changes to the list
        /// during iteration and therefore the behaviour is undefined
        /// under those conditions.
        /// </remarks>

        public struct Enumerator : IEnumerator<T>
        {
            private int _index;
            private readonly T[] _items;
            private readonly int _count;
            private T _current;

            internal Enumerator(T[] items, int count) : this()
            {
                _index = 0;
                _items = items;
                _count = count;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                T[] items = _items;
                if ((uint) _index >= (uint) _count)
                {
                    return MoveNextRare();
                }
                _current = items[_index];
                ++_index;
                return true;
            }

            private bool MoveNextRare()
            {
                _index = _count + 1;
                _current = default (T);
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public T Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _count + 1)
                    {
                        Throw<InvalidOperationException>();
                    }
                    return Current;
                }
            }
        }
    }
}