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
using OpenTK;

namespace Salvac.Data.Types
{
    public struct SpeedVector
    {
        public static readonly SpeedVector Zero = new SpeedVector(Vector2d.Zero, SpeedUnit.MetersPerSecond);

        #region Conversion Properties

        public Vector2d AsKilometersPerHour
        { get { return this.AsUnit(SpeedUnit.KilometersPerHour); } }

        public Vector2d AsMetersPerSecond
        { get { return this.AsUnit(SpeedUnit.MetersPerSecond); } }

        public Vector2d AsKnots
        { get { return this.AsUnit(SpeedUnit.Knots); } }

        public Vector2d AsUnit(SpeedUnit unit)
        {
            if (this.Unit == unit) return this.Value;
            else
            {
                double length = this.Length.AsUnit(unit);
                return this.Value.Normalized() * (float)length;
            }
        }

        #endregion


        private readonly SpeedUnit _unit;
        public SpeedUnit Unit
        { get { return _unit; } }

        private readonly Vector2d _value;
        public Vector2d Value
        { get { return _value; } }


        public Speed Length
        { get { return new Speed(_value.Length, _unit); } }

        public Vector2d Direction
        { get { return this.Value.Normalized(); } }


        public SpeedVector(Vector2d value, SpeedUnit unit)
        {
            _value = value;
            _unit = unit;
        }


        #region Comparison Operators

        public static bool operator ==(SpeedVector left, SpeedVector right)
        {
            SpeedUnit comparisonUnit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return Utils.FloatingEqual2D(left.AsUnit(comparisonUnit), right.AsUnit(comparisonUnit), 1e-5d);
        }

        public static bool operator !=(SpeedVector left, SpeedVector right)
        {
            return !(left == right);
        }

        #endregion

        #region Maths Operators

        public static SpeedVector operator +(SpeedVector left, SpeedVector right)
        {
            SpeedUnit unit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new SpeedVector(left.AsUnit(unit) + right.AsUnit(unit), unit);
        }

        public static SpeedVector operator -(SpeedVector left, SpeedVector right)
        {
            SpeedUnit unit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new SpeedVector(left.AsUnit(unit) - right.AsUnit(unit), unit);
        }

        public static SpeedVector operator -(SpeedVector value)
        {
            return new SpeedVector(-value.Value, value.Unit);
        }

        public static SpeedVector operator *(SpeedVector left, double right)
        {
            return new SpeedVector(left.Value * right, left.Unit);
        }

        public static SpeedVector operator *(double left, SpeedVector right)
        {
            return new SpeedVector(left * right.Value, right.Unit);
        }

        public static SpeedVector operator /(SpeedVector left, double right)
        {
            return new SpeedVector(left.Value / right, left.Unit);
        }

        #endregion


        #region Converters

        public SpeedVector ToUnit(SpeedUnit unit)
        {
            switch (unit)
            {
                case SpeedUnit.KilometersPerHour: return this.ToKilometersPerHour();
                case SpeedUnit.Knots: return this.ToKnots();
                case SpeedUnit.MetersPerSecond: return this.ToMetersPerSecond();

                default:
                    throw new NotSupportedException("unit is not supported.");
            }
        }

        public SpeedVector ToKilometersPerHour()
        {
            return new SpeedVector(this.AsKilometersPerHour, SpeedUnit.KilometersPerHour);
        }

        public SpeedVector ToKnots()
        {
            return new SpeedVector(this.AsKnots, SpeedUnit.Knots);
        }

        public SpeedVector ToMetersPerSecond()
        {
            return new SpeedVector(this.AsMetersPerSecond, SpeedUnit.MetersPerSecond);
        }

        #endregion

        #region Static Creators

        public static SpeedVector FromKilometersPerHour(Vector2d kilometersPerHour)
        {
            return new SpeedVector(kilometersPerHour, SpeedUnit.KilometersPerHour);
        }

        public static SpeedVector FromKnots(Vector2d knots)
        {
            return new SpeedVector(knots, SpeedUnit.Knots);
        }

        public static SpeedVector FromMetersPerSecond(Vector2d metersPerSecond)
        {
            return new SpeedVector(metersPerSecond, SpeedUnit.MetersPerSecond);
        }

        #endregion

        public static Vector2d Convert(Vector2d value, SpeedUnit source, SpeedUnit target)
        {
            return (new SpeedVector(value, source)).ToUnit(target).Value;
        }


        public bool Equals(SpeedVector other)
        {
            return other.Value == this.Value && other.Unit == this.Unit;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return false;
            else if (obj is SpeedVector)
                return this.Equals((SpeedVector)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.Unit.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(this.Value.ToString(), Speed.GetUnitDesignator(this.Unit));
        }
    }
}
