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
