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
        private List<IMouseListener> _mouseListeners;

        public MainWindow()
        {
            if (!WorldManager.Current.IsLoaded)
                throw new NoWorldException();

            InitializeComponent();
            CreateGLWindow();

            _loaded = false;
            _mouseListeners = new List<IMouseListener>();
        }

        private void CreateGLWindow()
        {
            glWindow = new GLControl(new GraphicsMode(new ColorFormat(8), 0, 0, 8));
            glWindow.Dock = DockStyle.Fill;
            glWindow.Name = "glWindow";
            this.glWindow.VSync = false;
            glWindow.Load += this.glWindow_Load;
            glWindow.Paint += this.glWindow_Paint;
            glWindow.MouseClick += this.glWindow_MouseClick;
            glWindow.MouseDoubleClick += this.glWindow_MouseDoubleClick;
            glWindow.MouseDown += this.glWindow_MouseDown;
            glWindow.MouseMove += this.glWindow_MouseMove;
            glWindow.MouseUp += this.glWindow_MouseUp;
            glWindow.MouseWheel += glWindow_MouseWheel;
            glWindow.Resize += this.glWindow_Resize;

            this.Controls.Add(glWindow);
        }

        #region Radar Screen

        private void LoadProfile()
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
                _mouseListeners.Clear();
            }

            // Create new data
            _radarScreen = new RadarScreen(this);
            _radarScreen.Load();

            // Load menus
            mnuLayers.DropDownItems.Clear();
            foreach (Layer layer in ProfileManager.Current.Profile.Layers.Content)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = layer.Name;
                item.CheckOnClick = true;
                item.Checked = ProfileManager.Current.Profile.Layers.EnabledContent.Contains(layer);
                item.CheckedChanged += (s, e) =>
                {
                    ProfileManager.Current.Profile.Layers.EnableItem(layer, item.Checked);
                    glWindow.Invalidate();
                };
                mnuLayers.DropDownItems.Add(item);
            }

            _loaded = true;
        }
        

        private void glWindow_Load(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Debug.Fail("Error while loading", ex.ToString());
            }
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

            RefreshViewport();
            glWindow.Invalidate();
        }

        private void glWindow_Paint(object sender, PaintEventArgs e)
        {
            if (!_loaded) return;

#if DEBUG
            Stopwatch watch = Stopwatch.StartNew();
#endif

            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (_radarScreen != null)
                _radarScreen.Render();

            glWindow.SwapBuffers();

#if DEBUG
            DebugInfo.LastFrameTime = watch.Elapsed.TotalSeconds;
#endif
        }

        #endregion

        #region Mouse Input

        public void AddMouseListener(IMouseListener listener)
        {
            if (!_mouseListeners.Contains(listener))
            {
                _mouseListeners.Add(listener);
                _mouseListeners.Sort((l1, l2) => l2.Priority.CompareTo(l1.Priority));
            }
        }

        public void RemoveMouseListener(IMouseListener listener)
        {
            _mouseListeners.Remove(listener);
        }

        private IMouseListener GetMouseListener(Vector2 position)
        {
            IMouseListener listener = (from m in _mouseListeners
                                       where m.IsMouseOverListener(position)
                                       select m).FirstOrDefault();
            return listener;
        }


        private Vector2 _lastMousePosition = Vector2.Zero;
        private MouseButtons _dragButton = MouseButtons.None;
        private bool _dragged = false;
        private void glWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);
            IMouseListener listener = this.GetMouseListener(currentPos);
            if (listener == null) return;

            if (_dragButton != MouseButtons.None && (e.Button & _dragButton) != 0)
            {
                Vector2 delta;
                Vector2.Subtract(ref currentPos, ref _lastMousePosition, out delta);
                _lastMousePosition = currentPos;

                if (delta != Vector2.Zero)
                {
                    _dragged = true;
                    listener.MouseDrag(_dragButton, delta, 0);
                }
            }
            else
                listener.MouseMove(currentPos, 0);
        }

        private void glWindow_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);
            IMouseListener listener = this.GetMouseListener(currentPos);
            if (listener == null) return;

            if (_dragButton != MouseButtons.None)
                listener.MouseDrag(_dragButton, Vector2.Zero, e.Delta);
            else
                listener.MouseMove(currentPos, e.Delta);
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
            IMouseListener listener = this.GetMouseListener(currentPos);
            if (listener == null) return;

            listener.MouseClick(e.Button, currentPos);
        }

        private void glWindow_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!_loaded) return;
            if (_dragged) return;

            Vector2 currentPos = new Vector2(e.X, e.Y);
            IMouseListener listener = this.GetMouseListener(currentPos);
            if (listener == null) return;

            listener.MouseDoubleClick(e.Button, currentPos);
        }

        #endregion

        private void mnuBoundingBoxes_CheckStateChanged(object sender, EventArgs e)
        {
            DebugInfo.DrawBoundingBoxes = mnuBoundingBoxes.Checked;
            glWindow.Invalidate();
        }

    }
}
