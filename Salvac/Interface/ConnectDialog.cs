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
using System.Threading.Tasks;
using Salvac.Sessions;

namespace Salvac.Interface
{
    public partial class ConnectDialog : Form
    {
        private IList<ISessionProvider> _providers;

        public ConnectDialog()
        {
            InitializeComponent();
        }

        private void DisplayText(string text)
        {
            pnlContent.Controls.Clear();
            Label lbl = new Label();
            lbl.Text = text;
            lbl.AutoSize = false;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            pnlContent.Controls.Add(lbl);
        }

        private async void ConnectDialog_Load(object sender, EventArgs e)
        {
            // Load providers
            _providers = await SessionManager.Current.GetProvidersAsync();

            if (_providers.Count > 0)
            {
                lstProviders.Items.AddRange(_providers.Select(p => p.Name).ToArray());
                lstProviders.Enabled = true;
                lstProviders.SelectedIndex = 0;
            }
        }

        private void lstProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load control
            pnlContent.Controls.Clear();
            Control control = _providers[lstProviders.SelectedIndex].ConnectDialog;
            control.Location = Point.Empty;
            this.ClientSize = new Size(control.Width + this.ClientSize.Width - pnlContent.Width, control.Height + this.ClientSize.Height - pnlContent.Height);
            pnlContent.Controls.Add(control);

            btnConnect.Enabled = true;
            btnCancel.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            ISessionProvider provider = _providers[lstProviders.SelectedIndex];

            DisplayText("Connecting ...");
            lstProviders.Enabled = false;
            btnCancel.Enabled = false;
            btnConnect.Enabled = false;

            ISession session = await provider.ConnectAsync();
            SessionManager.Current.LoadSession(session);

            this.Close();
        }
    }
}
