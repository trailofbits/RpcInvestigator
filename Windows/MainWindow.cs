//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using RpcInvestigator.TabPages;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RpcInvestigator.Windows;
using static RpcInvestigator.TraceLogger;
using System.Security.Principal;

namespace RpcInvestigator
{
    public partial class MainWindow : Form
    {
        private Settings m_Settings;
        private RpcLibrary m_Library;
        private TabManager m_TabManager;
        private Sniffer m_RpcSnifferWindow;
        private bool m_IsClosing;
        private object m_CloseLock = new object();

        public MainWindow()
        {
            InitializeComponent();
            DoubleBuffered = true;
            TraceLogger.Initialize();

            //
            // Load settings and database
            //
            try
            {
                m_Settings = Settings.LoadDefault();
                TraceLogger.SetLevel(m_Settings.m_TraceLevel);
            }
            catch (Exception) { } // swallow
            try
            {
                m_Library = new RpcLibrary(null);
                m_Library.Load();
            }
            catch (Exception) { } // swallow

            m_TabManager = new TabManager(
                mainTabControl, m_Settings, m_Library, progressBar, statusLabel);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (m_CloseLock)
            {
                m_IsClosing = true;
                if (m_RpcSnifferWindow != null)
                {
                    MessageBox.Show("Please close the RPC sniffer window.");
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void mainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            lock (m_CloseLock)
            {
                if (m_IsClosing)
                {
                    return;
                }
                TabPage thisTab = mainTabControl.TabPages[e.Index];
                string tabTitle = thisTab.Text;
                Rectangle tabRect = mainTabControl.GetTabRect(e.Index);
                tabRect.Inflate(-2, -2);

                //
                // Draw the icon for the tab's selected icon image.
                //
                var icon = imageList1.Images[thisTab.ImageIndex];
                e.Graphics.DrawImage(icon,
                    (tabRect.Left + 5),
                    tabRect.Top + (tabRect.Height - icon.Height) / 2);
                //
                // Draw the close tab button
                //
                Image closeIcon = imageList2.Images[0];
                e.Graphics.DrawImage(closeIcon,
                    (tabRect.Right - icon.Width) + 5,
                    tabRect.Top + (tabRect.Height - closeIcon.Height) / 2);
                //
                // Draw the tab title
                //
                var textRect = new Rectangle(tabRect.X + icon.Width + 5, tabRect.Y,
                    tabRect.Width - icon.Width - closeIcon.Width, tabRect.Height);
                TextRenderer.DrawText(e.Graphics, tabTitle, thisTab.Font,
                    textRect, thisTab.ForeColor, TextFormatFlags.Left);
            }
        }

        private void mainTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < mainTabControl.TabCount; i++)
            {
                Rectangle tabRect = mainTabControl.GetTabRect(i);
                tabRect.Inflate(-2, -2);
                Image closeIcon = imageList2.Images[0];
                var imageRect = new Rectangle(
                    (tabRect.Right - closeIcon.Width - 5),
                    tabRect.Top + (tabRect.Height - closeIcon.Height - 5) / 2,
                    closeIcon.Width + 5,
                    closeIcon.Height + 5);
                if (imageRect.Contains(e.Location))
                {
                    if (i > 0)
                    {
                        mainTabControl.SelectedIndex = i - 1;
                    }
                    mainTabControl.TabPages.RemoveAt(i);
                    break;
                }
            }
        }

        private void ToggleMenu(bool Enable)
        {
            menuStrip1.Enabled = Enable;
            mainTabControl.Enabled = Enable;
        }

        #region Menu actions

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var window = new SettingsWindow(m_Settings);
            if (window.ShowDialog() == DialogResult.OK)
            {
                m_Settings = window.m_Settings;
                TraceLogger.SetLevel(m_Settings.m_TraceLevel);
            }
        }

        private void loadFromBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ToggleMenu(false);
            _ = m_TabManager.LoadRpcServersTab(new List<string>() { dialog.FileName });
            ToggleMenu(true);
        }

        private void loadFromServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Services servicesForm = new Services(m_Settings);
            if (servicesForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (servicesForm.m_SelectedDlls.Count == 0)
            {
                return;
            }
            ToggleMenu(false);
            _ = m_TabManager.LoadRpcServersTab(servicesForm.m_SelectedDlls);
            ToggleMenu(true);
        }

        private void loadAllRPCALPCServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            _ = m_TabManager.LoadRpcAlpcServersTab();
            ToggleMenu(true);
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = Settings.m_LogDir;
            psi.WorkingDirectory = Settings.m_LogDir;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private async void libraryServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DetectEmptyLibrary())
            {
                return;
            }
            ToggleMenu(false);
            _ = await m_TabManager.LoadRpcLibraryServersTab(new RpcLibraryFilter());
            ToggleMenu(true);
        }

        private async void libraryProceduresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DetectEmptyLibrary())
            {
                return;
            }
            ToggleMenu(false);
            _ = await m_TabManager.LoadRpcLibraryProceduresTab(new RpcLibraryFilter());
            ToggleMenu(true);
        }

        private void eraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DetectEmptyLibrary())
            {
                return;
            }
            DialogResult result = MessageBox.Show(
                "Are you sure you want to erase the library?",
                "Confirmation",
                MessageBoxButtons.YesNoCancel);
            if (result != DialogResult.Yes)
            {
                return;
            }
            ToggleMenu(false);
            try
            {
                m_Library.Clear();
                m_Library.Save();
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Information,
                    "Database erased.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred when erasing the database: " +
                    ex.Message);
            }
            ToggleMenu(true);
        }

        private async void refreshLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            int workSize = 0;
            int max = 0;
            _ = await m_Library.Refresh(
                m_Settings,
                //
                // Initialize progress bar.
                //
                (ArrayList args) =>
                {
                    statusStrip1.Invoke((MethodInvoker)(() =>
                    {
                        workSize = (int)args[0];
                        max = (int)args[1];
                        var message = (string)args[2];
                        progressBar.Visible = true;
                        progressBar.Step = 1;
                        progressBar.Value = 0;
                        progressBar.Visible = true;
                        progressBar.Maximum = max;
                        statusLabel.Text = message;
                    }));
                },
                //
                // Update progress bar
                //
                (string message) =>
                {
                    statusStrip1.Invoke((MethodInvoker)(() =>
                    {
                        progressBar.PerformStep();
                        statusLabel.Text = message.Replace("<current>",
                            (progressBar.Value * workSize).ToString()).Replace(
                            "<total>", (max * workSize).ToString());
                    }));
                },
                //
                // Complete
                //
                (string message) =>
                {
                    statusLabel.Text = message;
                    progressBar.Value = 0;
                    progressBar.Visible = false;
                });
            ToggleMenu(true);
        }

        private void exportLibraryAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DetectEmptyLibrary())
            {
                return;
            }
            ToggleMenu(false);
            var location = Path.Combine(new string[] { Settings.m_LogDir,
                 "library-text-"+Path.GetRandomFileName() + ".txt" });
            File.WriteAllText(location, m_Library.ToString());
            var psi = new ProcessStartInfo();
            psi.FileName = location;
            psi.WorkingDirectory = Directory.GetParent(location).FullName;
            psi.UseShellExecute = true;
            Process.Start(psi);
            ToggleMenu(true);
        }

        private void rPCSnifferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_RpcSnifferWindow != null)
            {
                MessageBox.Show("Only one RPC sniffer window is allowed.");
                return;
            }
            m_RpcSnifferWindow = new Sniffer(
                m_Library,
                new SnifferCallbackTable()
                {
                    ShowRpcServerDetailsCallback = SnifferCallbackShowRpcServerDetails,
                },
                ref m_Settings);
            m_RpcSnifferWindow.Show();
            m_RpcSnifferWindow.FormClosed += delegate (object w, FormClosedEventArgs args)
            {
                m_RpcSnifferWindow = null;
            };
        }

        private async void SnifferCallbackShowRpcServerDetails(string InterfaceId)
        {
            if (DetectEmptyLibrary())
            {
                return;
            }
            if (!Guid.TryParse(InterfaceId, out Guid id))
            {
                MessageBox.Show(InterfaceId + " is not a valid GUID.");
                return;
            }
            ToggleMenu(false);
            var tab = await m_TabManager.LoadRpcLibraryServersTab(new RpcLibraryFilter());
            tab.ScrollToServer(id);
            ToggleMenu(true);
        }

        private bool DetectEmptyLibrary()
        {
            if (m_Library.GetServerCount() == 0)
            {
                MessageBox.Show("The library is empty. Please click the Refresh menu " +
                    "item under the Library menu to regenerate it.");
                return true;
            }
            return false;
        }

        #endregion

        private async void MainWindow_Shown(object sender, EventArgs e)
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show("Warning: RPC Investigator does not have administrator " +
                        "privileges. RPC interfaces inside privileged processes will not " +
                        "be accessible, and many features will be limited.",
                        "Administrator access is required",
                        buttons: MessageBoxButtons.OK,
                        icon: MessageBoxIcon.Warning);

                }
            }

            if (m_Settings.m_DisplayLibraryOnStart && !DetectEmptyLibrary())
            {
                ToggleMenu(false);
                var rpcServerTab = await m_TabManager.LoadRpcLibraryServersTab(new RpcLibraryFilter());
                _ = await m_TabManager.LoadRpcLibraryProceduresTab(new RpcLibraryFilter());
                ((TabControl)rpcServerTab.Parent).SelectedIndex = 0;
                ToggleMenu(true);
            }
        }
    }
}
