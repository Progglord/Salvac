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
    public class GeometryTheme : IDisposable
    {
        public static readonly GeometryTheme Default = new GeometryTheme()
        {
            EnableLineStippling = false,
            LineStipplePattern = 0xFFFF,
            LineStipplingFactor = 1,
            LineWidth = 1f,

            LineColor = Color.Gray,
            FillColor = Color.FromArgb(100, 10, 10, 10),

            TextFont = new Font("Arial", 10),
            TextColor = Color.White,
            EnableTextBackground = true,
            TextBackgroundColor = Color.FromArgb(200, 0, 0, 0),
            TextBackgroundBuffer = 2.0f
        };


        #region Lines

        public bool EnableLineStippling { get; set; }

        public ushort LineStipplePattern { get; set; }

        public int LineStipplingFactor { get; set; }

        public float LineWidth { get; set; }

        public Color LineColor { get; set; }

        #endregion

        #region Other Geometry

        public Color FillColor { get; set; }

        #endregion

        #region Text

        public Font TextFont { get; set; }

        public Color TextColor { get; set; }

        public bool EnableTextBackground { get; set; }

        public Color TextBackgroundColor { get; set; }

        public float TextBackgroundBuffer { get; set; }

        #endregion


        internal GeometryTheme()
        {
            // Initialize nothing, use this for parsing
        }

        public GeometryTheme Copy()
        {
            return new GeometryTheme()
            {
                EnableLineStippling = this.EnableLineStippling,
                LineStipplePattern = this.LineStipplePattern,
                LineStipplingFactor = this.LineStipplingFactor,
                LineWidth = this.LineWidth,
                LineColor = this.LineColor,
                FillColor = this.FillColor,
                TextFont = this.TextFont,
                TextColor = this.TextColor,
                EnableTextBackground = this.EnableTextBackground,
                TextBackgroundColor = this.TextBackgroundColor,
                TextBackgroundBuffer = this.TextBackgroundBuffer
            };
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.TextFont != null)
                    this.TextFont.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
