using System;
using System.Globalization;

namespace Salvac.Data.Types
{
    public enum SpeedUnit
    {
        MetersPerSecond,
        KilometersPerHour,
        Knots
    }

    public struct Speed
    {
        public static readonly Speed Zero = new Speed(0d, SpeedUnit.MetersPerSecond);

        #region Conversion Constants

        public const double MetersPerSecondPerKilometersPerHour = 0.277777778d;
        public const double KilometersPerHourPerMetersPerSecond = 3.6d;
        public const double MetersPerSecondPerKnots = 0.514444444d;
        public const double KnotsPerMetersPerSecond = 1.94384449d;
        public const double KilometersPerHourPerKnots = 1.852d;
        public const double KnotsPerKilometersPerHour = 0.539956803d;

        #endregion

        #region Conversion Properties

        public double AsKilometersPerHour
        {
            get
            {
                switch (this.Unit)
                {
                    case SpeedUnit.KilometersPerHour: return this.Value;
                    case SpeedUnit.Knots: return this.Value * KilometersPerHourPerKnots;
                    case SpeedUnit.MetersPerSecond: return this.Value * KilometersPerHourPerMetersPerSecond;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }

        public double AsMetersPerSecond
        {
            get
            {
                switch (this.Unit)
                {
                    case SpeedUnit.KilometersPerHour: return this.Value * MetersPerSecondPerKilometersPerHour;
                    case SpeedUnit.Knots: return this.Value * MetersPerSecondPerKnots;
                    case SpeedUnit.MetersPerSecond: return this.Value;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }

        public double AsKnots
        {
            get
            {
                switch (this.Unit)
                {
                    case SpeedUnit.KilometersPerHour: return this.Value * KnotsPerKilometersPerHour;
                    case SpeedUnit.Knots: return this.Value;
                    case SpeedUnit.MetersPerSecond: return this.Value * KnotsPerMetersPerSecond;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }


        public double AsUnit(SpeedUnit unit)
        {
            switch (unit)
            {
                case SpeedUnit.KilometersPerHour: return this.AsKilometersPerHour;
                case SpeedUnit.Knots: return this.AsKnots;
                case SpeedUnit.MetersPerSecond: return this.AsMetersPerSecond;

                default:
                    throw new InvalidOperationException("Unrecognized unit.");
            }
        }

        #endregion


        private readonly SpeedUnit _unit;
        public SpeedUnit Unit
        { get { return _unit; } }

        private readonly double _value;
        public double Value
        { get { return _value; } }


        public Speed(double value, SpeedUnit unit)
        {
            _value = value;
            _unit = unit;
        }


        public static bool IsPositiveInfinity(Speed speed)
        { return double.IsPositiveInfinity(speed.Value); }

        public static bool IsNegativeInfinity(Speed speed)
        { return double.IsNegativeInfinity(speed.Value); }

        public static bool IsInfinity(Speed speed)
        { return double.IsInfinity(speed.Value); }

        public static bool IsNaN(Speed speed)
        { return double.IsNaN(speed.Value); }


        #region Comparison Operators

        public static bool operator ==(Speed left, Speed right)
        {
            SpeedUnit comparisonUnit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return Utils.FloatingEqual(left.AsUnit(comparisonUnit), right.AsUnit(comparisonUnit), 1e-10d);
        }

        public static bool operator !=(Speed left, Speed right)
        {
            return !(left == right);
        }

        public static bool operator >(Speed left, Speed right)
        {
            SpeedUnit comparisonUnit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return left.AsUnit(comparisonUnit) > right.AsUnit(comparisonUnit);
        }

        public static bool operator <(Speed left, Speed right)
        {
            SpeedUnit comparisonUnit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return left.AsUnit(comparisonUnit) < right.AsUnit(comparisonUnit);
        }

        public static bool operator >=(Speed left, Speed right)
        {
            return left > right || left == right;
        }

        public static bool operator <=(Speed left, Speed right)
        {
            return left < right || left == right;
        }


        public int CompareTo(Speed other)
        {
            if (other == this) return 0;
            else if (other < this) return -1;
            else return +1;
        }

        #endregion

        #region Maths Operators

        public static Speed operator +(Speed left, Speed right)
        {
            SpeedUnit unit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new Speed(left.AsUnit(unit) + right.AsUnit(unit), unit);
        }

        public static Speed operator -(Speed left, Speed right)
        {
            SpeedUnit unit = (SpeedUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new Speed(left.AsUnit(unit) - right.AsUnit(unit), unit);
        }

        public static Speed operator -(Speed value)
        {
            return new Speed(-value.Value, value.Unit);
        }

        public static Speed operator *(Speed left, double right)
        {
            return new Speed(left.Value * right, left.Unit);
        }

        public static Speed operator *(double left, Speed right)
        {
            return new Speed(left * right.Value, right.Unit);
        }

        public static Speed operator /(Speed left, double right)
        {
            return new Speed(left.Value / right, left.Unit);
        }

        #endregion

        #region Parsing

        public bool TryParse(string input, out Speed speed)
        {
            return this.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat, out speed);
        }

        public bool TryParse(string input, NumberStyles style, IFormatProvider provider, out Speed speed)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");
            speed = default(Speed);

            SpeedUnit unit;
            if (!this.TryParseUnit(ref input, out unit))
                return false;

            double value;
            if (!double.TryParse(input, style, provider, out value))
                return false;

            speed = new Speed(value, unit);
            return true;
        }

        public Speed Parse(string input, NumberStyles style, IFormatProvider provider)
        {
            Speed speed;
            if (!this.TryParse(input, style, provider, out speed))
                throw new ArgumentException("input is bad formatted.", "input");
            return speed;
        }

        public Speed Parse(string input)
        {
            Speed speed;
            if (!this.TryParse(input, out speed))
                throw new ArgumentException("input is bad formatted.", "input");
            return speed;
        }

        private bool TryParseUnit(ref string input, out SpeedUnit unit)
        {
            input = input.TrimEnd();
            foreach (object val in Enum.GetValues(typeof(SpeedUnit)))
            {
                string designator = Speed.GetUnitDesignator((SpeedUnit)val);
                if (input.EndsWith(designator, StringComparison.InvariantCultureIgnoreCase))
                {
                    input = input.Remove(input.Length - designator.Length, designator.Length);
                    unit = (SpeedUnit)val;
                    return true;
                }
            }

            unit = default(SpeedUnit);
            return false;
        }


        public static string GetUnitDesignator(SpeedUnit unit)
        {
            switch (unit)
            {
                case SpeedUnit.MetersPerSecond: return "ms";
                case SpeedUnit.KilometersPerHour: return "kmh";
                case SpeedUnit.Knots: return "kt";

                default:
                    throw new ArgumentException("Unknown unit.", "unit");
            }
        }

        #endregion

        #region Converters

        public Speed ToUnit(SpeedUnit unit)
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

        public Speed ToKilometersPerHour()
        {
            return new Speed(this.AsKilometersPerHour, SpeedUnit.KilometersPerHour);
        }

        public Speed ToKnots()
        {
            return new Speed(this.AsKnots, SpeedUnit.Knots);
        }

        public Speed ToMetersPerSecond()
        {
            return new Speed(this.AsMetersPerSecond, SpeedUnit.MetersPerSecond);
        }

        #endregion

        #region Static Creators

        public static Speed FromKilometersPerHour(double kilometersPerHour)
        {
            return new Speed(kilometersPerHour, SpeedUnit.KilometersPerHour);
        }

        public static Speed FromKnots(double knots)
        {
            return new Speed(knots, SpeedUnit.Knots);
        }

        public static Speed FromMetersPerSecond(double metersPerSecond)
        {
            return new Speed(metersPerSecond, SpeedUnit.MetersPerSecond);
        }

        #endregion

        public static double Convert(double value, SpeedUnit source, SpeedUnit target)
        {
            return (new Speed(value, source)).ToUnit(target).Value;
        }


        public bool Equals(Speed other)
        {
            return other.Value == this.Value && other.Unit == this.Unit;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return false;
            else if (obj is Speed)
                return this.Equals((Speed)obj);
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

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(this.Value.ToString(format, formatProvider), Speed.GetUnitDesignator(this.Unit));
        }
    }
}
