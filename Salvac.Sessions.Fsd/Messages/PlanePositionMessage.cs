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
using System.Collections.Generic;
using Salvac.Data.Types;

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

    public sealed class PlanePositionMessage : Message
    {
        public const string TYPE = "@";

        public SquawkMode SquawkMode 
        { get; private set; }

        public Squawk Squawk
        { get; private set; }

        public int Rating
        { get; private set; }


        public PlanePosition Position
        { get; set; }

        public uint PitchBankHeading
        { get; private set; }


        public PlanePositionMessage(string source, SquawkMode squawkMode, Squawk squawk, int rating, 
            PlanePosition position, uint pitchBankHeading) :
            base(TYPE, source, null)
        {
            if (squawkMode < Messages.SquawkMode.First || squawkMode > Messages.SquawkMode.Last) throw new ArgumentOutOfRangeException("squawkMode");

            this.SquawkMode = squawkMode;
            this.Squawk = squawk;
            this.Rating = rating;
            this.Position = position;
            this.PitchBankHeading = pitchBankHeading;
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
            yield return ((int)this.Rating).ToString();
            yield return this.Position.Position.Y.ToString(CultureInfo.InvariantCulture.NumberFormat);
            yield return this.Position.Position.X.ToString(CultureInfo.InvariantCulture.NumberFormat);
            yield return this.Position.Elevation.AsFeet.ToString("0");
            yield return this.Position.GroundSpeed.AsKnots.ToString("0");
            yield return this.PitchBankHeading.ToString();
            yield return (this.Position.PressureAltitude.AsFeet - this.Position.Elevation.AsFeet).ToString("0");
        }
    }
}
