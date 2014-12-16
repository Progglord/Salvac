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
    public enum PressureUnit
    {
        HectoPascals,
        InchHg
    }

    public struct Pressure
    {
        public static readonly Pressure Zero = new Pressure(0d, PressureUnit.HectoPascals);
        public static readonly Pressure Standard = new Pressure(1013.25d, PressureUnit.HectoPascals);

        #region Conversion Constants

        public const double HectoPascalsPerInchHg = 33.86d;
        public const double InchHgPerHectoPascal = 0.0295333727d;

        #endregion

        #region Conversion Properties

        public double AsHectoPascals
        {
            get
            {
                switch ( this.Unit)
                {
                    case PressureUnit.HectoPascals: return this.Value;
                    case PressureUnit.InchHg: return this.Value * HectoPascalsPerInchHg;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }

        public double AsInchHg
        {
            get
            {
                switch (this.Unit)
                {
                    case PressureUnit.HectoPascals: return this.Value * InchHgPerHectoPascal;
                    case PressureUnit.InchHg: return this.Value;

                    default:
                        throw new InvalidOperationException("Unrecognized unit.");
                }
            }
        }


        public double AsUnit(PressureUnit unit)
        {
            switch (unit)
            {
                case PressureUnit.HectoPascals: return this.AsHectoPascals;
                case PressureUnit.InchHg: return this.AsInchHg;

                default:
                    throw new InvalidOperationException("Unrecognized unit.");
            }
        }

        #endregion


        private readonly PressureUnit _unit;
        public PressureUnit Unit
        { get { return _unit; } }

        private readonly double _value;
        public double Value
        { get { return _value; } }


        public Pressure(double value, PressureUnit unit)
        {
            _value = value;
            _unit = unit;
        }


        public static bool IsPositiveInfinity(Pressure pressure)
        { return double.IsPositiveInfinity(pressure.Value); }

        public static bool IsNegativeInfinity(Pressure pressure)
        { return double.IsNegativeInfinity(pressure.Value); }

        public static bool IsInfinity(Pressure pressure)
        { return double.IsInfinity(pressure.Value); }

        public static bool IsNaN(Pressure pressure)
        { return double.IsNaN(pressure.Value); }


        #region Comparison Operators

        public static bool operator ==(Pressure left, Pressure right)
        {
            PressureUnit comparisonUnit = (PressureUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return Utils.FloatingEqual(left.AsUnit(comparisonUnit), right.AsUnit(comparisonUnit), 1e-5d);
        }

        public static bool operator !=(Pressure left, Pressure right)
        {
            return !(left == right);
        }

        public static bool operator >(Pressure left, Pressure right)
        {
            PressureUnit comparisonUnit = (PressureUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return left.AsUnit(comparisonUnit) > right.AsUnit(comparisonUnit);
        }

        public static bool operator <(Pressure left, Pressure right)
        {
            PressureUnit comparisonUnit = (PressureUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return left.AsUnit(comparisonUnit) < right.AsUnit(comparisonUnit);
        }

        public static bool operator >=(Pressure left, Pressure right)
        {
            return left > right || left == right;
        }

        public static bool operator <=(Pressure left, Pressure right)
        {
            return left < right || left == right;
        }


        public int CompareTo(Pressure other)
        {
            if (other == this) return 0;
            else if (other < this) return -1;
            else return +1;
        }

        #endregion

        #region Maths Operators

        public static Pressure operator +(Pressure left, Pressure right)
        {
            PressureUnit unit = (PressureUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new Pressure(left.AsUnit(unit) + right.AsUnit(unit), unit);
        }

        public static Pressure operator -(Pressure left, Pressure right)
        {
            PressureUnit unit = (PressureUnit)Math.Min((int)left.Unit, (int)right.Unit);
            return new Pressure(left.AsUnit(unit) - right.AsUnit(unit), unit);
        }

        public static Pressure operator -(Pressure value)
        {
            return new Pressure(-value.Value, value.Unit);
        }

        public static Pressure operator *(Pressure left, double right)
        {
            return new Pressure(left.Value * right, left.Unit);
        }

        public static Pressure operator *(double left, Pressure right)
        {
            return new Pressure(left * right.Value, right.Unit);
        }

        public static Pressure operator /(Pressure left, double right)
        {
            return new Pressure(left.Value / right, left.Unit);
        }

        #endregion

        #region Parsing

        public bool TryParse(string input, out Pressure pressure)
        {
            return this.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat, out pressure);
        }

        public bool TryParse(string input, NumberStyles style, IFormatProvider provider, out Pressure pressure)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");
            pressure = default(Pressure);

            PressureUnit unit;
            if (!this.TryParseUnit(ref input, out unit))
                return false;

            double value;
            if (!double.TryParse(input, style, provider, out value))
                return false;

            pressure = new Pressure(value, unit);
            return true;
        }

        public Pressure Parse(string input, NumberStyles style, IFormatProvider provider)
        {
            Pressure pressure;
            if (!this.TryParse(input, style, provider, out pressure))
                throw new ArgumentException("input is bad formatted.", "input");
            return pressure;
        }

        public Pressure Parse(string input)
        {
            Pressure pressure;
            if (!this.TryParse(input, out pressure))
                throw new ArgumentException("input is bad formatted.", "input");
            return pressure;
        }

        private bool TryParseUnit(ref string input, out PressureUnit unit)
        {
            input = input.TrimEnd();
            foreach (object val in Enum.GetValues(typeof(PressureUnit)))
            {
                string designator = Pressure.GetUnitDesignator((PressureUnit)val);
                if (input.EndsWith(designator, StringComparison.InvariantCultureIgnoreCase))
                {
                    input = input.Remove(input.Length - designator.Length, designator.Length);
                    unit = (PressureUnit)val;
                    return true;
                }
            }

            unit = default(PressureUnit);
            return false;
        }


        public static string GetUnitDesignator(PressureUnit unit)
        {
            switch (unit)
            {
                case PressureUnit.HectoPascals: return "hPa";
                case PressureUnit.InchHg: return "inHg";

                default:
                    throw new ArgumentException("Unknown unit.", "unit");
            }
        }

        #endregion

        #region Converters

        public Pressure ToUnit(PressureUnit unit)
        {
            switch (unit)
            {
                case PressureUnit.HectoPascals: return this.ToHectoPascals();
                case PressureUnit.InchHg: return this.ToInchHg();

                default:
                    throw new NotSupportedException("unit is not supported.");
            }
        }

        public Pressure ToHectoPascals()
        {
            return new Pressure(this.AsHectoPascals, PressureUnit.HectoPascals);
        }

        public Pressure ToInchHg()
        {
            return new Pressure(this.AsInchHg, PressureUnit.InchHg);
        }

        #endregion

        #region Static Creators

        public static Pressure FromHectoPascals(double hectoPascals)
        {
            return new Pressure(hectoPascals, PressureUnit.HectoPascals);
        }

        public static Pressure FromInchHg(double inchHg)
        {
            return new Pressure(inchHg, PressureUnit.InchHg);
        }

        #endregion

        public static double Convert(double value, PressureUnit source, PressureUnit target)
        {
            return (new Pressure(value, source)).ToUnit(target).Value;
        }


        public bool Equals(Pressure other)
        {
            return other.Value == this.Value && other.Unit == this.Unit;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
                return false;
            else if (obj is Pressure)
                return this.Equals((Pressure)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.Unit.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat(this.Value.ToString(), Pressure.GetUnitDesignator(this.Unit));
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Concat(this.Value.ToString(format, formatProvider), Pressure.GetUnitDesignator(this.Unit));
        }
    }
}
