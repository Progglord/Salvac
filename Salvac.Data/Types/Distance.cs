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
    public enum DistanceUnit
    {
        Feet,
        Meters,
        Kilometers,
        NauticalMiles
    }

    public struct Distance : IEquatable<Distance>, IFormattable, IComparable<Distance>
    {
        public static readonly Distance Zero = new Distance(0d, DistanceUnit.Meters);

        #region Conversion Constants

        public const double NauticalMilesPerMeter = 0.000539956803d;
        public const double MetersPerNauticalMile = 1852d;
        public const double NauticalMilesPerKilometer = 0.539956803d;
        public const double KilometersPerNauticalMile = 1.852d;
        public const double MetersPerKilometer = 1000d;
        public const double KilometersPerMeter = 0.001d;
        public const double FeetPerMeter = 3.2808399d;
        public const double MetersPerFeet = 0.3048d;
        public const double FeetPerKilometer = 3280.8399d;
        public const double KilometersPerFeet = 0.0003048d;
        public const double FeetPerNauticalMile = 6076.11549d;
        public const double NauticalMilesPerFeet = 0.000164578834d;

        #endregion

        #region Conversion Properties

        public double AsMeters
        {
            get
            {
                switch (this.Unit)
                {
                    case DistanceUnit.Meters: return this.Value;
                    case DistanceUnit.NauticalMiles: return this.Value * MetersPerNauticalMile;
                    case DistanceUnit.Kilometers: return this.Value * MetersPerKilometer;
                    case DistanceUnit.Feet: return this.Value * MetersPerFeet;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }

        public double AsNauticalMiles
        {
            get
            {
                switch (this.Unit)
                {
                    case DistanceUnit.Meters: return this.Value * NauticalMilesPerMeter;
                    case DistanceUnit.NauticalMiles: return this.Value;
                    case DistanceUnit.Kilometers: return this.Value * NauticalMilesPerKilometer;
                    case DistanceUnit.Feet: return this.Value * NauticalMilesPerFeet;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }

        public double AsKilometers
        {
            get
            {
                switch (this.Unit)
                {
                    case DistanceUnit.Meters: return this.Value * KilometersPerMeter;
                    case DistanceUnit.Kilometers: return this.Value;
                    case DistanceUnit.NauticalMiles: return this.Value * KilometersPerNauticalMile;
                    case DistanceUnit.Feet: return this.Value * KilometersPerFeet;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }

        public double AsFeet
        {
            get
            {
                switch (this.Unit)
                {
                    case DistanceUnit.Feet: return this.Value;
                    case DistanceUnit.Kilometers: return this.Value * FeetPerKilometer;
                    case DistanceUnit.Meters: return this.Value * FeetPerMeter;
                    case DistanceUnit.NauticalMiles: return this.Value * FeetPerNauticalMile;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }


        public double AsUnit(DistanceUnit unit)
        {
            switch (unit)
            {
                case DistanceUnit.Meters: return this.AsMeters;
                case DistanceUnit.Kilometers: return this.AsKilometers;
                case DistanceUnit.NauticalMiles: return this.AsNauticalMiles;
                case DistanceUnit.Feet: return this.AsFeet;

                default:
                    throw new InvalidOperationException("Unrecognized unit.");
            }
        }

        #endregion


        private readonly DistanceUnit _unit;
        public DistanceUnit Unit
        { get { return _unit; } }

        private readonly double _value;
        public double Value
        { get { return _value; } }


        public Distance(double value, DistanceUnit unit)
        {
            _value = value;
            _unit = unit;
        }


        public static bool IsPositiveInfinity(Distance distance)
        { return double.IsPositiveInfinity(distance.Value); }

        public static bool IsNegativeInfinity(Distance distance)
        { return double.IsNegativeInfinity(distance.Value); }

        public static bool IsInfinity(Distance distance)
        { return double.IsInfinity(distance.Value); }

        public static bool IsNaN(Distance distance)
        { return double.IsNaN(distance.Value); }


        #region Comparison Operators

        public static bool operator ==(Distance left, Distance right)
        {
            DistanceUnit comparisonUnit = (DistanceUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return Utils.FloatingEqual(left.AsUnit(comparisonUnit), right.AsUnit(comparisonUnit), 1e-5d);
        }

        public static bool operator !=(Distance left, Distance right)
        {
            return !(left == right);
        }

        public static bool operator >(Distance left, Distance right)
        {
            DistanceUnit comparisonUnit = (DistanceUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return left.AsUnit(comparisonUnit) > right.AsUnit(comparisonUnit);
        }

        public static bool operator <(Distance left, Distance right)
        {
            DistanceUnit comparisonUnit = (DistanceUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return left.AsUnit(comparisonUnit) < right.AsUnit(comparisonUnit);
        }

        public static bool operator >=(Distance left, Distance right)
        {
            return left > right || left == right;
        }

        public static bool operator <=(Distance left, Distance right)
        {
            return left < right || left == right;
        }


        public int CompareTo(Distance other)
        {
            if (other == this) return 0;
            else if (other < this) return -1;
            else return +1;
        }

        #endregion

        #region Maths Operators

        public static Distance operator +(Distance left, Distance right)
        {
            DistanceUnit unit = (DistanceUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new Distance(left.AsUnit(unit) + right.AsUnit(unit), unit);
        }

        public static Distance operator -(Distance left, Distance right)
        {
            DistanceUnit unit = (DistanceUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new Distance(left.AsUnit(unit) - right.AsUnit(unit), unit);
        }

        public static Distance operator -(Distance value)
        {
            return new Distance(-value.Value, value.Unit);
        }

        public static Distance operator *(Distance left, double right)
        {
            return new Distance(left.Value * right, left.Unit);
        }

        public static Distance operator *(double left, Distance right)
        {
            return new Distance(left * right.Value, right.Unit);
        }

        public static Distance operator /(Distance left, double right)
        {
            return new Distance(left.Value / right, left.Unit);
        }

        #endregion

        #region Parsing

        public bool TryParse(string input, out Distance distance)
        {
            return this.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat, out distance);
        }

        public bool TryParse(string input, NumberStyles style, IFormatProvider provider, out Distance distance)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");
            distance = default(Distance);

            DistanceUnit unit;
            if (!this.TryParseUnit(ref input, out unit))
                return false;

            double value;
            if (!double.TryParse(input, style, provider, out value))
                return false;

            distance = new Distance(value, unit);
            return true;
        }

        public Distance Parse(string input, NumberStyles style, IFormatProvider provider)
        {
            Distance distance;
            if (!this.TryParse(input, style, provider, out distance))
                throw new ArgumentException("input is bad formatted.", "input");
            return distance;
        }

        public Distance Parse(string input)
        {
            Distance distance;
            if (!this.TryParse(input, out distance))
                throw new ArgumentException("input is bad formatted.", "input");
            return distance;
        }

        private bool TryParseUnit(ref string input, out DistanceUnit unit)
        {
            input = input.TrimEnd();
            foreach (object val in Enum.GetValues(typeof(DistanceUnit)))
            {
                string designator = Distance.GetUnitDesignator((DistanceUnit)val);
                if (input.EndsWith(designator, StringComparison.InvariantCultureIgnoreCase))
                {
                    input = input.Remove(input.Length - designator.Length, designator.Length);
                    unit = (DistanceUnit)val;
                    return true;
                }
            }

            unit = default(DistanceUnit);
            return false;
        }


        public static string GetUnitDesignator(DistanceUnit unit)
        {
            switch (unit)
            {
                case DistanceUnit.Meters: return "m";
                case DistanceUnit.Kilometers: return "km";
                case DistanceUnit.NauticalMiles: return "nm";
                case DistanceUnit.Feet: return "ft";

                default:
                    throw new ArgumentException("Unknown unit.", "unit");
            }
        }

        #endregion

        #region Converters

        public Distance ToUnit(DistanceUnit unit)
        {
            switch (unit)
            {
                case DistanceUnit.Meters: return this.ToMeters();
                case DistanceUnit.NauticalMiles: return this.ToNauticalMiles();
                case DistanceUnit.Feet: return this.ToFeet();
                case DistanceUnit.Kilometers: return this.ToKilometers();

                default:
                    throw new NotSupportedException("unit is not supported.");
            }
        }

        public Distance ToMeters()
        {
            return new Distance(this.AsMeters, DistanceUnit.Meters);
        }

        public Distance ToNauticalMiles()
        {
            return new Distance(this.AsNauticalMiles, DistanceUnit.NauticalMiles);
        }

        public Distance ToKilometers()
        {
            return new Distance(this.AsKilometers, DistanceUnit.Kilometers);
        }

        public Distance ToFeet()
        {
            return new Distance(this.AsFeet, DistanceUnit.Feet);
        }

        #endregion

        #region Static Creators

        public static Distance FromMeters(double meters)
        {
            return new Distance(meters, DistanceUnit.Meters);
        }

        public static Distance FromNauticalMiles(double nauticalMiles)
        {
            return new Distance(nauticalMiles, DistanceUnit.NauticalMiles);
        }

        public static Distance FromKilometers(double kilometers)
        {
            return new Distance(kilometers, DistanceUnit.Kilometers);
        }

        public static Distance FromFeet(double feet)
        {
            return new Distance(feet, DistanceUnit.Feet);
        }

        #endregion

        public static double Convert(double value, DistanceUnit source, DistanceUnit target)
        {
            return (new Distance(value, source)).ToUnit(target).Value;
        }


        public bool Equals(Distance other)
        {
            return other.Value == this.Value && other.Unit == this.Unit;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return false;
            else if (obj is Distance)
                return this.Equals((Distance)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.Unit.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(this.Value.ToString(), Distance.GetUnitDesignator(this.Unit));
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(this.Value.ToString(format, formatProvider), Distance.GetUnitDesignator(this.Unit));
        }
    }
}
