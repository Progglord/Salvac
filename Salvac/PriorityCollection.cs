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
using System.Linq;
using System.Collections.Generic;

namespace Salvac
{
    public class PriorityCollection<T> : ICollection<T>
    {
        private List<T> _list;
        private Func<T, int> _prioritySelector;

        public int Count
        { get { return _list.Count; } }

        public bool IsReadOnly
        { get { return false; } }


        public PriorityCollection(Func<T, int> prioritySelector)
        {
            _list = new List<T>();
            _prioritySelector = prioritySelector;
        }


        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (items.Any(i => i == null)) throw new ArgumentNullException("items contains null values.");

            _list.AddRange(items);
            _list.Sort((l, r) => _prioritySelector(r).CompareTo(_prioritySelector(l)));
        }

        public void AddRange(params T[] items)
        {
            this.AddRange(items as IEnumerable<T>);
        }

        public void Add(T item)
        {
            this.AddRange(item);
        }


        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }


        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
