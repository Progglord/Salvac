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
        public event EventHandler<SessionClosedEventArgs> SessionClosed;

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
            if (this.IsLoaded) throw new InvalidOperationException("There is already a loaded session.");

            this.Session = session;
            this.Session.Closed += (s, e) =>
            {
                this.Session = null;
                this.IsLoaded = false;

                if (this.SessionClosed != null)
                    this.SessionClosed(this, e);
            };
            this.IsLoaded = true;

            if (SessionOpened != null)
                SessionOpened(this, EventArgs.Empty);
        }

        public void CloseSession()
        {
            if (!this.IsLoaded) return;
            this.Session.Close(); // Come back to ISession.Closed event
        }

        public async Task<IList<ISessionProvider>> GetProvidersAsync()
        {
            var taskList = new List<Task<IEnumerable<ISessionProvider>>>();
            foreach (string file in Directory.GetFiles(PLUGINDIR, "*.dll"))
            {
                taskList.Add(Task.Factory.StartNew<IEnumerable<ISessionProvider>>((s) =>
                {
                    Assembly assembly = Assembly.LoadFile(Path.GetFullPath(file));
                    var types = (from t in assembly.GetTypes()
                                 where typeof(ISessionProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract & t.IsVisible && t.GetConstructor(Type.EmptyTypes) != null
                                 select Activator.CreateInstance(t) as ISessionProvider);
                    return types;
                }, file));
            }

            await Task.WhenAll(taskList);

            List<ISessionProvider> providers = new List<ISessionProvider>();
            foreach (var task in taskList)
            {
                if (task.IsFaulted)
                    Debug.WriteLine("Could not load plugin assembly '{0}': {1}", task.AsyncState, task.Exception.Message);
                else
                    providers.AddRange(task.Result);
            }

            
            return providers;
        }
    }
}
