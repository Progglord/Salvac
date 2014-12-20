﻿// Salvac
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

#if DEBUG

using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Salvac.Interface.Rendering
{

    public sealed class DebugScreen : IRenderable, IInputListener
    {
        #region Static Information Collectors

        public static int DrawnEnvironment
        { get; set; }

        public static int DrawnPilots
        { get; set; }

        public static bool DrawBoundingBoxes
        { get; set; }

        public static double LastFrameTime
        { get; set; }

        #endregion


        public event EventHandler Updated;

        private TextRenderer _textRenderer;
        private Font _debugFont;


        public bool IsDisposed
        { get; private set; }

        public bool IsLoaded
        { get; private set; }

        public bool IsEnabled
        { get; set; }

        public int RenderPriority
        { get { return int.MinValue; } } // ALWAYS render at last!

        public int InputPriority
        { get { return 0; } }


        public DebugScreen(TextRenderer textRenderer)
        {
            if (textRenderer == null) throw new ArgumentNullException("textRenderer");

            _textRenderer = textRenderer;

            this.IsDisposed = false;
            this.IsLoaded = false;
            this.IsEnabled = true;
        }


        public void Load()
        {
            if (this.IsDisposed) throw new ObjectDisposedException("DebugScreen");
            if (this.IsLoaded) return;

            _debugFont = new Font("Terminal", 9, FontStyle.Regular);
            this.IsLoaded = true;
        }

        public void Render(Viewport viewport)
        {
            if (this.IsDisposed) throw new ObjectDisposedException("DebugScreen");
            if (!this.IsLoaded) return;
            if (!this.IsEnabled) return;

            string debugString = string.Format("Time: {0:00.000}ms, Environment: {1}, Pilots: {2}",
               DebugScreen.LastFrameTime * 1000d, DebugScreen.DrawnEnvironment, DebugScreen.DrawnPilots);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            _textRenderer.DrawString(debugString, _debugFont, Brushes.White, new Vector2(5, viewport.Height - 30));

            GL.PopMatrix();

            // Reset collect data
            DebugScreen.DrawnEnvironment = 0;
            DebugScreen.DrawnPilots = 0;
        }


        public bool IsMouseOver(Vector2 position)
        { return false; }

        public bool MouseClick(System.Windows.Forms.MouseButtons button, Vector2 position)
        { return false; }

        public bool MouseDoubleClick(System.Windows.Forms.MouseButtons button, Vector2 position)
        { return false; }

        public bool MouseDrag(System.Windows.Forms.MouseButtons button, Vector2 position, Vector2 delta, float wheelDelta)
        { return false; }

        public bool MouseMove(Vector2 position, float wheelDelta)
        { return false; }

        public bool KeyPress(Keys key)
        {
            if ((key & Keys.F3) != (Keys)0)
            {
                this.IsEnabled = !this.IsEnabled;
                if (this.Updated != null)
                    this.Updated(this, EventArgs.Empty);

                return true;
            }

            return false;
        }


        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    _debugFont.Dispose();
                }
                this.IsDisposed = true;
                this.IsLoaded = false;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}

#endif