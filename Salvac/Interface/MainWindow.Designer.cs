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
namespace Salvac.Interface
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuBar = new System.Windows.Forms.ToolStrip();
            this.btnConnect = new System.Windows.Forms.ToolStripButton();
            this.btnLayers = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDebug = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnDebugBoundingBoxes = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuBar
            // 
            this.menuBar.BackColor = System.Drawing.SystemColors.Control;
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnConnect,
            this.btnLayers,
            this.toolStripSeparator1,
            this.btnDebug});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(640, 25);
            this.menuBar.TabIndex = 1;
            this.menuBar.Text = "toolStrip1";
            // 
            // btnConnect
            // 
            this.btnConnect.AutoToolTip = false;
            this.btnConnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnConnect.Image = ((System.Drawing.Image)(resources.GetObject("btnConnect.Image")));
            this.btnConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(56, 22);
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnLayers
            // 
            this.btnLayers.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLayers.Image = ((System.Drawing.Image)(resources.GetObject("btnLayers.Image")));
            this.btnLayers.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLayers.Name = "btnLayers";
            this.btnLayers.Size = new System.Drawing.Size(53, 22);
            this.btnLayers.Text = "Layers";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnDebug
            // 
            this.btnDebug.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnDebugBoundingBoxes});
            this.btnDebug.Image = ((System.Drawing.Image)(resources.GetObject("btnDebug.Image")));
            this.btnDebug.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDebug.Name = "btnDebug";
            this.btnDebug.Size = new System.Drawing.Size(55, 22);
            this.btnDebug.Text = "Debug";
            // 
            // btnDebugBoundingBoxes
            // 
            this.btnDebugBoundingBoxes.CheckOnClick = true;
            this.btnDebugBoundingBoxes.Name = "btnDebugBoundingBoxes";
            this.btnDebugBoundingBoxes.Size = new System.Drawing.Size(186, 22);
            this.btnDebugBoundingBoxes.Text = "Draw BoundingBoxes";
            this.btnDebugBoundingBoxes.CheckStateChanged += new System.EventHandler(this.btnDebugBoundingBoxes_CheckStateChanged);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 426);
            this.Controls.Add(this.menuBar);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Salvac";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip menuBar;
        private System.Windows.Forms.ToolStripButton btnConnect;
        private System.Windows.Forms.ToolStripDropDownButton btnLayers;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton btnDebug;
        private System.Windows.Forms.ToolStripMenuItem btnDebugBoundingBoxes;
    }
}