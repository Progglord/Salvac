// Salvac
// Copyright (C) 2014 Oliver Schmidt
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac.Data
{
    public sealed class SortedEnablingSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private sealed class Comparer : IComparer<KeyValuePair<T, bool>>
        {
            public IComparer<T> TComparer;

            public Comparer(IComparer<T> tComparer)
            {
                TComparer = tComparer;
            }


            public int Compare(KeyValuePair<T, bool> x, KeyValuePair<T, bool> y)
            {
                if (TComparer != null)
                {
                    int tComp = TComparer.Compare(x.Key, y.Key);
                    if (tComp == 0) return 0;
                    else if (x.Value == y.Value) return tComp;
                    else if (!x.Value) return +1;
                    else return -1;
                }
                else if (x.Value == y.Value && x.Key.Equals(y.Key)) return 0;
                else if (!x.Value) return +1;
                else return -1;
            }
        }


        private List<KeyValuePair<T, bool>> _content;
        private int _enabledCount;
        private Comparer _comparer;


        public IEnumerable<T> Content
        {
            get
            {
                foreach (KeyValuePair<T, bool> content in _content)
                    yield return content.Key;
            }
        }

        public IEnumerable<T> EnabledContent
        {
            get
            {
                foreach (KeyValuePair<T, bool> content in _content)
                {
                    if (!content.Value) break;
                    yield return content.Key;
                }
            }
        }

        public int Count
        { get { return _content.Count; ; } }

        public bool IsReadOnly
        { get { return false; } }


        public SortedEnablingSet(IEnumerable<KeyValuePair<T, bool>> content, IComparer<T> comparer)
        {
            if (content == null) throw new ArgumentNullException("context");

            _enabledCount = 0;
            _content = new List<KeyValuePair<T, bool>>(content);
            _comparer = new Comparer(comparer);
            _content.Sort(_comparer);
        }

        public SortedEnablingSet(IEnumerable<T> content, Func<T, bool> enabledSeletor, IComparer<T> comparer) :
            this((content != null ? content.Select(c => new KeyValuePair<T, bool>(c, enabledSeletor(c))) : null), comparer)
        { }

        public SortedEnablingSet(IEnumerable<T> content, IComparer<T> comparer) :
            this(content, l => true, comparer)
        { }

        public SortedEnablingSet(IComparer<T> comparer) :
            this(Enumerable.Empty<KeyValuePair<T, bool>>(), comparer)
        { }


        public SortedEnablingSet(IEnumerable<KeyValuePair<T, bool>> content) :
            this(content, null)
        { }

        public SortedEnablingSet(IEnumerable<T> content, Func<T, bool> enabledSeletor) :
            this((content != null ? content.Select(c => new KeyValuePair<T, bool>(c, enabledSeletor(c))) : null))
        { }

        public SortedEnablingSet(IEnumerable<T> content) :
            this(content, l => true)
        { }

        public SortedEnablingSet() :
            this(Enumerable.Empty<KeyValuePair<T, bool>>())
        { }


        public void Add(T item, bool enabled)
        {
            if (this.Contains(item))
                return;

            _content.Add(new KeyValuePair<T, bool>(item, enabled));
            _content.Sort(_comparer);

            if (enabled)
                _enabledCount++;
        }

        public void Add(T item)
        {
            this.Add(item, true);
        }

        public void Clear()
        {
            _content.Clear();
        }

        public bool Contains(T item)
        {
            return FindItem(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (KeyValuePair<T, bool> content in _content)
            {
                if (arrayIndex >= array.Length)
                    throw new ArgumentOutOfRangeException("array is not big enough.");

                array[arrayIndex] = content.Key;
                arrayIndex++;
            }
        }

        public void EnableItem(T item, bool enabled)
        {
            int idx = this.FindItem(item);
            if (idx < 0) throw new ArgumentException("item is not in this SortedEnablingSet", "item");

            bool wasEnabled = _content[idx].Value;
            if (wasEnabled != enabled)
            {
                _content[idx] = new KeyValuePair<T, bool>(item, enabled);
                _content.Sort(_comparer);

                if (enabled) _enabledCount++;
                else _enabledCount--;
            }
        }
        
        public bool IsEnabled(T item)
        {
            return this.FindItem(item, true) != -1;
        }


        public bool Remove(T item)
        {
            int idx = FindItem(item);
            if (idx < 0) return false;
            bool enabled = _content[idx].Value;
            _content.RemoveAt(idx);
            if (enabled)
                _enabledCount--;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Content.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Content.GetEnumerator();
        }


        private int FindItem(T item, bool onlyEnabled = false)
        {
            if (_comparer.TComparer != null)
            {
                int search = _content.BinarySearch(0, _enabledCount, new KeyValuePair<T, bool>(item, true), _comparer);
                if (search != -1 || onlyEnabled) return search;
                search = _content.BinarySearch(_enabledCount, _content.Count - _enabledCount, new KeyValuePair<T, bool>(item, false), _comparer);
                return search;
            }
            else
            {
                // Straight forward search
                for (int i = 0; i < (onlyEnabled ? _enabledCount : _content.Count); i++)
                {
                    if (_content[i].Key.Equals(item))
                        return i;
                }

                return -1;
            }
        }
    }
}
