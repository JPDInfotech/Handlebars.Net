using System;
using System.Collections;
using System.Collections.Generic;

namespace HandlebarsDotNet.Collections
{
    public class WeakCollection<T> : IEnumerable<T> where T : class
    {
        private readonly List<WeakReference<T>> _store = new List<WeakReference<T>>();
        private int _firstAvailableIndex = 0;

        public int Size => _store.Count;
        
        public void Add(T value)
        {
            for (var index = _firstAvailableIndex; index < _store.Count; index++)
            {
                if (_store[index] == null)
                {
                    _firstAvailableIndex = index + 1;
                    _store[index] = new WeakReference<T>(value);
                    return;
                }

                if (!_store[index].TryGetTarget(out _))
                {
                    _firstAvailableIndex = index + 1;
                    _store[index].SetTarget(value);
                    return;
                }
            }
            
            _store.Add(new WeakReference<T>(value));
            _firstAvailableIndex = _store.Count;
        }
        
        public void Remove(T value)
        {
            for (var index = 0; index < _store.Count; index++)
            {
                if (_store[index] == null)
                {
                    _firstAvailableIndex = Math.Min(_firstAvailableIndex, index);
                    continue;
                }

                if (!_store[index].TryGetTarget(out var target))
                {
                    _firstAvailableIndex = Math.Min(_firstAvailableIndex, index);
                    continue;
                }

                if (target.Equals(value))
                {
                    _store[index] = null;
                    _firstAvailableIndex = Math.Min(_firstAvailableIndex, index);
                    return;
                }
            }
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            for (var index = 0; index < _store.Count; index++)
            {
                var reference = _store[index];
                if (reference == null)
                {
                    _firstAvailableIndex = Math.Min(_firstAvailableIndex, index);
                    continue;
                }

                if (!reference.TryGetTarget(out var target))
                {
                    _firstAvailableIndex = Math.Min(_firstAvailableIndex, index);
                    continue;
                }

                yield return target;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}