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
using System.Diagnostics;

namespace Salvac.Sessions.Fsd
{
    public abstract class FsdEntity : IEntity
    {
        private int INACTIVE_TIME = 10000;
        private int TIMEOUT_TIME = 60000;

        public event EventHandler Updated;
        public event EventHandler TimedOut;


        private Stopwatch _timer;
        private bool _prevInactive;

        public string FsdName
        { get; private set; }

        public bool IsInactive
        { get { return _timer.ElapsedMilliseconds >= INACTIVE_TIME; } }

        protected TimeSpan TimerElapsedTime
        { get { return _timer.Elapsed; } }


        public FsdEntity(string fsdName)
        {
            if (string.IsNullOrEmpty(fsdName)) throw new ArgumentNullException("fsdName");

            this.FsdName = fsdName;

            _prevInactive = false;
            this.WakeUp();
        }


        protected void WakeUp()
        {
            // Reset timeout timer or create a new one if there is no.
            if (_timer == null)
                _timer = Stopwatch.StartNew();
            else
            {
                _timer.Reset();
                _timer.Start();
            }
        }

        public void Refresh()
        {
            // Check for timeout and inactive states
            if (_timer.ElapsedMilliseconds >= TIMEOUT_TIME)
                this.OnTimeOut();
            else if (this.IsInactive && !_prevInactive || !this.IsInactive && _prevInactive)
            {
                _prevInactive = this.IsInactive;
                this.OnUpdated();
            }
        }


        protected void OnUpdated()
        {
            if (this.Updated != null)
                this.Updated(this, EventArgs.Empty);
        }

        protected void OnTimeOut()
        {
            if (this.TimedOut != null)
                this.TimedOut(this, EventArgs.Empty);
        }

    }
}
