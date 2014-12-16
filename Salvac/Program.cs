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
using System.Linq;
using System.Windows.Forms;
using Salvac.Interface;
using System.Diagnostics;
using OpenTK;

namespace Salvac
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Debug.Listeners.Add(new ConsoleTraceListener());

            // TODO: GIT managment -> show (independent of MainWindow) world model dialog
            WorldManager.Current.LoadModel("..\\world.sqlite");
            ProfileManager.Current.LoadDummyProfile(); // normally MainWindow has to take care of profiles -> for no we do it here

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

            ProfileManager.Current.Dispose();
            WorldManager.Current.Dispose();
        }
    }
}
