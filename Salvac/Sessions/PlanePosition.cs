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
using Salvac.Data.Types;
using OpenTK;

namespace Salvac.Sessions
{
    public struct PlanePosition : IEquatable<PlanePosition>
    {
        private Vector2d _position;
        /// <summary>
        /// Position in WGS84.
        /// X -> longitude
        /// Y -> latitude
        /// </summary>
        public Vector2d Position
        { get { return _position; } set { _position = value; } }

        private Speed _groundSpeed;
        /// <summary>
        /// Speed above ground.
        /// </summary>
        public Speed GroundSpeed
        { get { return _groundSpeed; } set { _groundSpeed = value; } }

        private Distance _elevation;
        /// <summary>
        /// Elevation (or Altitude) above mean sea level (MSL).
        /// </summary>
        public Distance Elevation
        { get { return _elevation; } set { _elevation = value; } }

        private Distance _pressureAltitude;
        /// <summary>
        /// Indicated altitude on barometer setting standard.
        /// </summary>
        public Distance PressureAltitude
        { get { return _pressureAltitude; } set { _pressureAltitude = value; } }

        private Angle _trueHeading;
        /// <summary>
        /// Heading with reference to true north.
        /// </summary>
        public Angle TrueHeading
        { get { return _trueHeading; } set { _trueHeading = value; } }

        private bool _onGround;
        /// <summary>
        /// A value indicating wether the plane is on ground.
        /// </summary>
        public bool OnGround
        { get { return _onGround; } set { _onGround = value; } }


        public PlanePosition(Vector2d position, Speed groundSpeed, Distance elevation, Distance pressureAltitude, Angle trueHeading, bool onGround)
        {
            _position = position;
            _groundSpeed = groundSpeed;
            _elevation = elevation;
            _pressureAltitude = pressureAltitude;
            _trueHeading = trueHeading;
            _onGround = onGround;
        }

        public PlanePosition(PlanePosition other)
        {
            _position = other.Position;
            _groundSpeed = other.GroundSpeed;
            _elevation = other.Elevation;
            _pressureAltitude = other.PressureAltitude;
            _trueHeading = other.TrueHeading;
            _onGround = other.OnGround;
        }


        public bool Equals(PlanePosition other)
        {
            return Position == other.Position &&
                GroundSpeed == other.GroundSpeed &&
                Elevation == other.Elevation &&
                PressureAltitude == other.PressureAltitude &&
                OnGround == other.OnGround;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, null)) return false;
            else if (obj is PlanePosition) return Equals((PlanePosition)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ GroundSpeed.GetHashCode() ^ Elevation.GetHashCode() ^ PressureAltitude.GetHashCode() ^ OnGround.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{lat={0:0.0000}, lon={1:0.0000}, speed={2}, alt={3}, pressAlt={4}, onGround={5}}}", 
                Position.Y, Position.X, GroundSpeed.ToString(), Elevation.ToString(), PressureAltitude.ToString(), OnGround);
        }
    }
}
