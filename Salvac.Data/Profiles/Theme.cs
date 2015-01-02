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
    public sealed class Theme
    {
        public static readonly Theme Default = new Theme()
        {
            NormalLabelTheme = LabelTheme.Default,
            InactiveLabelTheme = new LabelTheme()
            {
                DotType = AircraftDotType.Cross,
                DotLineColor = Color.White,
                DotFillColor = Color.Transparent,
                DotLineWidth = 0.5f,
                DotWidth = 3f,

                EnableSpeedVector = false,
                SpeedVectorColor = Color.Transparent,
                SpeedVectorLineWidth = 0f,

                LabelTextFont = new Font("Microsoft Sans Serif", 7),
                LabelTextColor = Color.DarkRed
            }
        };

        public LabelTheme NormalLabelTheme { get; set; }
        public LabelTheme InactiveLabelTheme { get; set; }

    }
}
