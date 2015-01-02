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
using System.Drawing;

namespace Salvac.Data.Profiles
{
    public enum AircraftDotType
    {
        /// <summary>
        /// Do not render any dot.
        /// </summary>
        None,

        /// <summary>
        /// Draw a cross.
        /// </summary>
        Cross,

        /// <summary>
        /// Draw a square.
        /// </summary>
        Square,

        /// <summary>
        /// Draw a diamond.
        /// </summary>
        Diamond
    }

    public class LabelTheme
    {
        public static readonly LabelTheme Default = new LabelTheme()
        {
            DotType = AircraftDotType.Diamond,
            DotWidth = 3d,
            DotLineColor = Color.White,
            DotLineWidth = 0.5f,
            DotFillColor = Color.Transparent,

            EnableSpeedVector = true,
            SpeedVectorColor = Color.White,
            SpeedVectorLineWidth = 1f,

            LabelTextFont = new Font("Microsoft Sans Serif", 9),
            LabelTextColor = Color.SteelBlue
        };


        public AircraftDotType DotType { get; set; }
        public double DotWidth { get; set; }
        public Color DotLineColor { get; set; }
        public float DotLineWidth { get; set; }
        public Color DotFillColor { get; set; }

        public bool EnableSpeedVector { get; set; }
        public Color SpeedVectorColor { get; set; }
        public float SpeedVectorLineWidth { get; set; }

        public Font LabelTextFont { get; set; }
        public Color LabelTextColor { get; set; }
    }
}
