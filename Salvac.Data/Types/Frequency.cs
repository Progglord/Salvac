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
        private readonly int _hertz;

        public int AsHertz
        { get { return _hertz; } }

        public double AsKiloHertz
        { get { return (double)_hertz / 1000d; } }

        public double AsMegaHertz
        { get { return this.AsKiloHertz / 1000d; } }


        public bool IsHf
        { get { return 3000000d <= _hertz && _hertz < 30000000d; } }

        public bool IsVhf
        { get { return 30000000d <= _hertz && _hertz < 300000000d; } }

        public bool IsUhf
        { get { return 300000000d <= _hertz && _hertz <= 3000000000d; } }

        public bool IsAtc
        { get { return 117975000d <= _hertz && _hertz <= 137000000d; } }

        public bool IsDme
        { get { return 962000000d <= _hertz && _hertz <= 1213000000d; } }

        public bool IsVor
        { get { return 108000000d <= _hertz && _hertz <= 117975000d; } }

        public bool IsLocalizer
        { get { return 108100000d <= _hertz && _hertz <= 111950000d; } }

        public bool IsGlideslope
        { get { return 329000000d <= _hertz && _hertz <= 335000000d; } }

        public bool IsMarker
        { get { return 74600000d <= _hertz && _hertz <= 75400000d; } }

        public bool IsNdb
        { get { return 190000000d <= _hertz && _hertz <= 1750000000d; } }

        public bool IsTacan
        { get { return 962000000d <= _hertz && _hertz <= 1213000000d; } }


        /// <param name="value">Frequency in Hertz.</param>
        public Frequency(int hertz)
        {
            if (hertz <= 0) throw new ArgumentException("A frequency shall not be zero or negative.");
            _hertz = hertz;
        }


        public static bool operator ==(Frequency left, Frequency right)
        {
            return left._hertz == right._hertz;
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
            return this._hertz == other._hertz;
        }

        public override int GetHashCode()
        {
            return _hertz.GetHashCode();
        }

        public override string ToString()
        {
            return this.AsMegaHertz.ToString("0.000mHz");
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _hertz.ToString(format, formatProvider);
        }
    }
}
