using System;
using System.Drawing;

namespace Salvac.Data.Profiles
{
    public class LayerTheme : IDisposable
    {
        public static readonly LayerTheme Default = new LayerTheme()
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


        internal LayerTheme()
        {
            // Initialize nothing, use this for parsing
        }

        public LayerTheme Copy()
        {
            return new LayerTheme()
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
