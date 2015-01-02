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

namespace Salvac.Data.Types
{
    public struct Angle : IEquatable<Angle>
    {
        public static readonly Angle Zero = new Angle(0d);
        public static readonly Angle North = Zero;
        public static readonly Angle East = new Angle(90d);
        public static readonly Angle South = new Angle(180d);
        public static readonly Angle West = new Angle(270d);

        private const double ToRadians = Math.PI / 180d;
        private const double ToDegrees = 180d / Math.PI;

        private readonly double _degrees;

        public double AsDegrees
        { get { return _degrees; } }

        public double AsRadians
        { get { return _degrees * ToRadians; } }


        private Angle(double degrees)
        {
            if (double.IsNaN(degrees) || double.IsInfinity(degrees))
                throw new ArgumentOutOfRangeException("degrees");

            // Transform degrees to range 0 to 360 (exclusive)
            degrees = degrees % 360;
            if (degrees < 0d)
                degrees += 360d;

            _degrees = degrees;
        }

        public static Angle FromDegrees(double degrees)
        {
            if (double.IsNaN(degrees) || double.IsInfinity(degrees))
                throw new ArgumentOutOfRangeException("degrees");
            return new Angle(degrees);
        }

        public static Angle FromRadians(double radians)
        {
            if (double.IsNaN(radians) || double.IsInfinity(radians))
                throw new ArgumentOutOfRangeException("radians");
            return new Angle(radians * ToDegrees);
        }


        #region Operators

        public static Angle operator +(Angle left, Angle right)
        {
            return new Angle(left._degrees + right._degrees);
        }

        public static Angle operator -(Angle left, Angle right)
        {
            return new Angle(left._degrees - right._degrees);
        }

        public static Angle operator *(Angle left, double right)
        {
            return new Angle(left._degrees * right);
        }

        public static Angle operator *(double left, Angle right)
        {
            return new Angle(left * right._degrees);
        }

        public static Angle operator /(Angle left, double right)
        {
            return new Angle(left._degrees / right);
        }

        #endregion

        #region Static Math Operators

        public static double Sin(Angle angle)
        {
            return Math.Sin(angle.AsRadians);
        }

        public static double Sinh(Angle angle)
        {
            return Math.Sinh(angle.AsRadians);
        }

        public static double Cos(Angle angle)
        {
            return Math.Cos(angle.AsRadians);
        }

        public static double Cosh(Angle angle)
        {
            return Math.Cosh(angle.AsRadians);
        }

        public static double Tan(Angle angle)
        {
            return Math.Tan(angle.AsRadians);
        }

        public static double Tanh(Angle angle)
        {
            return Math.Tanh(angle.AsRadians);
        }

        public static double Cot(Angle angle)
        {
            return 1d / Math.Tan(angle.AsRadians);
        }

        public static double Coth(Angle angle)
        {
            return 1d / Math.Tanh(angle.AsRadians);
        }


        public static Angle Asin(double sin)
        {
            return Angle.FromRadians(Math.Asin(sin));
        }

        public static Angle Acos(double cos)
        {
            return Angle.FromRadians(Math.Acos(cos));
        }

        public static Angle Atan(double tan)
        {
            return Angle.FromRadians(Math.Atan(tan));
        }

        public static Angle Atan2(double y, double x)
        {
            return Angle.FromRadians(Math.Atan2(y, x));
        }

        #endregion


        public bool Equals(Angle other)
        {
            return _degrees == other._degrees;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null)) return false;
            if (obj is Angle) return Equals((Angle)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return _degrees.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0:0.00}°", _degrees);
        }
    }
}
