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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Salvac.Sessions;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Salvac
{
    public sealed class SessionManager
    {
        private const string PLUGINDIR = "..\\plugins";

        #region Singleton

        private static SessionManager _current;
        public static SessionManager Current
        {
            get
            {
                if (_current == null)
                    _current = new SessionManager();
                return _current;
            }
        }

        #endregion

        public event EventHandler SessionOpened;
        public event EventHandler SessionClosed;

        public bool IsLoaded
        { get; private set; }

        public ISession Session
        { get; private set; }


        public SessionManager()
        {
            this.IsLoaded = false;
            this.Session = null;
        }


        public void LoadSession(ISession session)
        {
            if (session == null) throw new ArgumentNullException("session");

            this.CloseSession();

            this.Session = session;
            this.IsLoaded = true;

            if (SessionOpened != null)
                SessionOpened(this, EventArgs.Empty);
        }

        public void CloseSession()
        {
            if (!this.IsLoaded) return;

            this.Session.Close();
            this.Session.Dispose();
            this.Session = null;
            this.IsLoaded = false;

            if (this.SessionClosed != null)
                this.SessionClosed(this, EventArgs.Empty);
        }

        public IList<ISessionProvider> GetProviders()
        {
            List<ISessionProvider> providers = new List<ISessionProvider>();
            foreach (string file in Directory.GetFiles(PLUGINDIR, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(Path.GetFullPath(file));
                    var types = (from t in assembly.GetTypes()
                                 where typeof(ISessionProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract & t.IsVisible && t.GetConstructor(Type.EmptyTypes) != null
                                 select Activator.CreateInstance(t) as ISessionProvider);
                    providers.AddRange(types);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Could not load plugin assembly '{0}': {1}", file, ex.Message);
                }
            }
            return providers;
        }
    }
}
