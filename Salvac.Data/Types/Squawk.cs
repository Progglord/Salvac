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
using System.Globalization;

namespace Salvac.Data.Types
{
    public struct Squawk : IEquatable<Squawk>, IComparable<Squawk>
    {
        public static readonly Squawk Emergency = new Squawk(0xFC0); // 7700
        public static readonly Squawk ComFail = new Squawk(0xF80); // 7600
        public static readonly Squawk HiJack = new Squawk(0xF40); // 7500


        private int _squawk;
        public int Value
        { get { return _squawk; } }


        public Squawk(int squawk)
        {
            if (squawk < 0x000 || squawk > 0xFFF) throw new ArgumentOutOfRangeException(string.Format("Squawk is out of range: {0}", squawk));
            _squawk = squawk;
        }


        public static Squawk Parse(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");

            try
            {
                int squawk = Convert.ToInt32(input, 8);
                return new Squawk(squawk);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentException(string.Format("Squawk is out of range: {0}", input));
            }
            catch (FormatException)
            {
                throw new ArgumentException(string.Format("Squawk could not be parsed as Int32: {0}", input));
            }
        }


        public int CompareTo(Squawk other)
        {
            return this._squawk.CompareTo(other._squawk);
        }

        public bool Equals(Squawk other)
        {
            return this._squawk == other._squawk;
        }

        public override int GetHashCode()
        {
            return _squawk.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null)) return false;
            else if (obj is Squawk) return this.Equals((Squawk)obj);
            else return base.Equals(obj);
        }

        public override string ToString()
        {
            return Convert.ToString(_squawk, 8).PadLeft(4, '0');
        }
    }
}
