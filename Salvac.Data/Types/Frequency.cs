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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac.Data.Types
{
    public struct Frequency : IEquatable<Frequency>, IFormattable
    {
        private readonly double _value;
        public double AsMegaHertz
        { get { return _value; } }


        public bool IsHf
        { get { return 3d <= _value && _value < 30d; } }

        public bool IsVhf
        { get { return 30d <= _value && _value < 300d; } }

        public bool IsUhf
        { get { return 300d <= _value && _value <= 3000d; } }

        public bool IsAtc
        { get { return 117.975d <= _value && _value <= 137d; } }

        public bool IsDme
        { get { return 962d <= _value && _value <= 1213d; } }

        public bool IsVor
        { get { return 108d <= _value && _value <= 117.975d; } }

        public bool IsLocalizer
        { get { return 108.1d <= _value && _value <= 111.95d; } }

        public bool IsGlideslope
        { get { return 329d <= _value && _value <= 335d; } }

        public bool IsMarker
        { get { return 74.6d <= _value && _value <= 75.4d; } }

        public bool IsNdb
        { get { return 190d <= _value && _value <= 1750d; } }

        public bool IsTacan
        { get { return 962d <= _value && _value <= 1213d; } }


        /// <param name="value">Frequency in MegaHertz (mHz).</param>
        public Frequency(double value)
        {
            if (value <= 0) throw new ArgumentException("A frequency shall not be zero or negative.");
            _value = value;
        }


        public static bool operator ==(Frequency left, Frequency right)
        {
            return Utils.FloatingEqual(left.AsMegaHertz, right.AsMegaHertz, 1e-3);
        }

        public static bool operator !=(Frequency left, Frequency right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null)) return false;
            else if (obj is Frequency)
                return this.Equals((Frequency)obj);
            else
                return base.Equals(obj);
        }

        public bool Equals(Frequency other)
        {
            return this.AsMegaHertz == other.AsMegaHertz;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString("0.000");
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _value.ToString(format, formatProvider);
        }
    }
}
