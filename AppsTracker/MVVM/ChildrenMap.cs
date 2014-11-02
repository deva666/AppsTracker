using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    internal sealed class ChildrenMap<T> : IDictionary<T, Func<T>> where T : ViewModelBase
    {
        public void Add(T key, Func<T> value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(T key)
        {
            throw new NotImplementedException();
        }

        public ICollection<T> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(T key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(T key, out Func<T> value)
        {
            throw new NotImplementedException();
        }

        public ICollection<Func<T>> Values
        {
            get { throw new NotImplementedException(); }
        }

        public Func<T> this[T key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<T, Func<T>> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<T, Func<T>> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<T, Func<T>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<T, Func<T>> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<T, Func<T>>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
