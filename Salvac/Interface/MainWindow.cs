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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Salvac.Data;
using System.Diagnostics;
using Salvac.Data.World;
using Salvac.Data.Profiles;
using Salvac.Interface.Rendering;

namespace Salvac.Interface
{
    public partial class MainWindow : Form
    {
        public GLControl glWindow;

        private bool _loaded;
        private RadarScreen _radarScreen;

        public MainWindow()
        {
            if (!WorldManager.Current.IsLoaded)
                throw new NoWorldException();

            InitializeComponent();
            CreateGLWindow();

            _loaded = false;

            SessionManager.Current.SessionOpened += (s, e) =>
                {
                    btnConnect.Text = "Disconnect";
                };
            SessionManager.Current.SessionClosed += (s, e) =>
                {
                    btnConnect.Text = "Connect";
                };
        }

        private void CreateGLWindow()
        {
            glWindow = new GLControl(new GraphicsMode(new ColorFormat(8), 0, 0, 8));
            glWindow.Dock = DockStyle.Fill;
            glWindow.Name = "glWindow";
            glWindow.VSync = true;

            glWindow.Load += glWindow_Load;
            glWindow.Paint += glWindow_Paint;
            glWindow.MouseClick += glWindow_MouseClick;
            glWindow.MouseDoubleClick += glWindow_MouseDoubleClick;
            glWindow.MouseDown += glWindow_MouseDown;
            glWindow.MouseMove += glWindow_MouseMove;
            glWindow.MouseUp += glWindow_MouseUp;
            glWindow.MouseWheel += glWindow_MouseWheel;
            glWindow.KeyUp += glWindow_KeyUp;
            glWindow.Resize += glWindow_Resize;

            this.Controls.Add(glWindow);
        }

        #region Radar Screen

        private async void LoadProfile()
        {
            if (!ProfileManager.Current.IsLoaded)
                throw new NoProfileException();
            if (GraphicsContext.CurrentContext == null)
                throw new InvalidOperationException("There is no valid GraphicsContext on this thread.");

            // Dispose current data
            if (_loaded)
            {
                _loaded = false;

                _radarScreen.Dispose();
            }

            // Create new data
            _radarScreen = new RadarScreen(glWindow);
            await _radarScreen.LoadAsync();

            // Load menus
            btnLayers.DropDownItems.Clear();
            foreach (Layer layer in ProfileManager.Current.Profile.Layers)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = layer.Name;
                item.CheckOnClick = true;
                item.Checked = ProfileManager.Current.Profile.IsLayerEnabled(layer);
                item.CheckedChanged += (s, e) =>
                {
                    ProfileManager.Current.Profile.Sectors.EnableAll(item.Checked, x => layer.Content.Contains(x.Id));
                    glWindow.Invalidate();
                };
                btnLayers.DropDownItems.Add(item);
            }

            _loaded = true;
        }


        private void glWindow_Load(object sender, EventArgs e)
        {
            // Load OpenGL
            GraphicsContext.CurrentContext.ErrorChecking = true;
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            RefreshViewport();

            if (ProfileManager.Current.IsLoaded)
                this.LoadProfile();

            ProfileManager.Current.ProfileLoaded += (s, ea) => { this.LoadProfile(); };
        }

        private void RefreshViewport()
        {
            GL.Viewport(0, 0, glWindow.Width, glWindow.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0d, glWindow.Width, 0d, glWindow.Height, -1d, 1d);
        }

        private void glWindow_Resize(object sender, EventArgs e)
        {
            if (!_loaded) return;
            if (this.WindowState == FormWindowState.Minimized) return;

            RefreshViewport();
            glWindow.Invalidate();
        }

        private void glWindow_Paint(object sender, PaintEventArgs e)
        {
            if (!_loaded) return;
            if (this.WindowState == FormWindowState.Minimized) return;

#if DEBUG
            Stopwatch watch = Stopwatch.StartNew();
#endif
            try
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);

                if (_radarScreen != null)
                    _radarScreen.Render();

                glWindow.SwapBuffers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Rendering Error: " + ex.ToString());
            }

#if DEBUG
            DebugScreen.FrameTime.AddValue(watch.Elapsed.TotalMilliseconds);
#endif
        }

        #endregion

        #region Input

        private Vector2 _lastMousePosition = Vector2.Zero;
        private MouseButtons _dragButton = MouseButtons.None;
        private bool _dragged = false;

        private void glWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);

            if (_dragButton != MouseButtons.None && (e.Button & _dragButton) != 0)
            {
                Vector2 delta;
                Vector2.Subtract(ref currentPos, ref _lastMousePosition, out delta);
                _lastMousePosition = currentPos;

                if (delta != Vector2.Zero)
                {
                    _dragged = true;
                    _radarScreen.MouseDrag(_dragButton, currentPos, delta, 0);
                }
            }
            else
                _radarScreen.MouseMove(currentPos, 0);
        }

        private void glWindow_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);

            if (_dragButton != MouseButtons.None)
                _radarScreen.MouseDrag(_dragButton, currentPos, Vector2.Zero, e.Delta);
            else
                _radarScreen.MouseMove(currentPos, e.Delta);
        }

        private void glWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (_dragButton == MouseButtons.None)
            {
                _lastMousePosition = new Vector2(e.X, e.Y);
                _dragButton = e.Button;
            }
        }

        private void glWindow_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & _dragButton) != 0)
            {
                _dragButton = MouseButtons.None;
                _dragged = false;
            }
        }

        private void glWindow_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;
            if (_dragged) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);
            _radarScreen.MouseClick(e.Button, currentPos);
        }

        private void glWindow_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;
            if (_dragged) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);
            _radarScreen.MouseDoubleClick(e.Button, currentPos);
        }

        private void glWindow_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!_loaded) return;

            e.Handled = _radarScreen.KeyPress(e.KeyData);
        }

        #endregion


        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _loaded = false;
            glWindow.Dispose();
            _radarScreen.Dispose();

            if (SessionManager.Current.IsLoaded)
                SessionManager.Current.CloseSession();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (SessionManager.Current.IsLoaded)
                SessionManager.Current.CloseSession();
            else
                (new ConnectDialog()).ShowDialog();
        }

        private void btnDebugBoundingBoxes_CheckStateChanged(object sender, EventArgs e)
        {
            DebugScreen.DrawBoundingBoxes = btnDebugBoundingBoxes.Checked;
            glWindow.Invalidate();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

    }
}
