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
using DotSpatial.Topology;
using Salvac.Data.Types;
using System.Globalization;

namespace Salvac.Sessions.Fsd.Messages
{
    public enum SquawkMode
    {
        Standby = 0, // S
        Charlie = 1, // C
        Ident = 2,   // Y

        First = Standby,
        Last = Ident
    }

    public sealed class PilotPositionMessage : Message
    {
        public const string TYPE = "@";

        public SquawkMode SquawkMode 
        { get; private set; }

        public Squawk Squawk
        { get; private set; }

        public PilotRating PilotRating
        { get; private set; }

        public Coordinate Position
        { get; private set; }

        public Distance TrueAltitude
        { get; private set; }

        public Speed GroundSpeed
        { get; private set; }

        public uint PitchBankHeading
        { get; private set; }

        public bool OnGround
        {
            get
            {
                if (BitConverter.IsLittleEndian)
                    return (this.PitchBankHeading & 0x2) > 0;
                else
                    return (this.PitchBankHeading & 0x80000000) > 0;
            }
        }

        public int AltitudeDifference
        { get; private set; }


        public PilotPositionMessage(string source, SquawkMode squawkMode, Squawk squawk, PilotRating rating, Coordinate position,
            Distance trueAltitude, Speed groundSpeed, uint pitchBankHeading, int altitudeDifference) :
            base(TYPE, source, null)
        {
            if (squawkMode < Messages.SquawkMode.First || squawkMode > Messages.SquawkMode.Last) throw new ArgumentOutOfRangeException("squawkMode");
            if (rating < Messages.PilotRating.First || rating > Messages.PilotRating.Last) throw new ArgumentOutOfRangeException("rating");

            this.SquawkMode = squawkMode;
            this.Squawk = squawk;
            this.PilotRating = rating;
            this.Position = position;
            this.TrueAltitude = trueAltitude;
            this.GroundSpeed = groundSpeed;
            this.PitchBankHeading = pitchBankHeading;
            this.AltitudeDifference = altitudeDifference;
        }

        protected override IEnumerable<string> GetTokens()
        {
            switch (this.SquawkMode)
            {
                case Messages.SquawkMode.Standby: yield return "S"; break;
                case Messages.SquawkMode.Charlie: yield return "N"; break;
                case Messages.SquawkMode.Ident: yield return "Y"; break;
            }
            yield return this.Source;
            yield return this.Squawk.ToString();
            yield return ((int)this.PilotRating).ToString();
            yield return this.Position.Y.ToString(CultureInfo.InvariantCulture.NumberFormat);
            yield return this.Position.X.ToString(CultureInfo.InvariantCulture.NumberFormat);
            yield return ((int)this.TrueAltitude.AsFeet).ToString();
            yield return ((int)this.GroundSpeed.AsKnots).ToString();
            yield return this.PitchBankHeading.ToString();
            yield return this.AltitudeDifference.ToString();
        }
    }
}
