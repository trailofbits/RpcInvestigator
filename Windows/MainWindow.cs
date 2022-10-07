using BrightIdeasSoftware;
using NtApiDotNet;
using NtApiDotNet.Win32;
using RpcInvestigator.TabPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace RpcInvestigator
{
    public partial class MainWindow : Form
    {
        private Settings m_Settings;
        private static readonly string m_WorkingDir = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + "\\RpcInvestigator";
        private static readonly string m_TraceFileDir = Path.Combine(
            new string[] { m_WorkingDir, "Logs\\" });
        private static TraceSwitch trace =
            new TraceSwitch("RPC Investigator", "RPC Investigator tracing");
        private RpcLibrary m_Library;
        private TabManager m_TabManager;

        public MainWindow()
        {
            InitializeComponent();
            //
            // Initialize trace
            //
            var location = Path.Combine(new string[] { m_TraceFileDir,
                            Path.GetRandomFileName() + ".txt" });
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(location, "RpcInvestigatorListener"));
            trace.Level = TraceLevel.Verbose;

            //
            // Load settings and database
            //
            try
            {
                m_Settings = Settings.LoadDefault();
                trace.Level = m_Settings.m_TraceLevel;
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

        private void mainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
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
            m_TabManager.LoadRpcServersTab(new List<string>() { dialog.FileName });
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
            m_TabManager.LoadRpcServersTab(servicesForm.m_SelectedDlls);
            ToggleMenu(true);
        }

        private void loadAllRPCALPCServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            m_TabManager.LoadRpcAlpcServersTab();
            ToggleMenu(true);
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(m_TraceFileDir);
        }

        private void libraryServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            m_TabManager.LoadRpcLibraryServersTab(new RpcLibraryFilter());
            ToggleMenu(true);
        }

        private void libraryProceduresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            m_TabManager.LoadRpcLibraryProceduresTab(new RpcLibraryFilter());
            ToggleMenu(true);
        }

        private async void refreshLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            _ = await m_Library.Refresh(m_Settings, progressBar, statusLabel);
            ToggleMenu(true);
        }

        private void exportLibraryAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenu(false);
            var location = Path.Combine(new string[] { m_TraceFileDir,
                            "library-text-"+Path.GetRandomFileName() + ".txt" });
            File.WriteAllText(location, m_Library.ToString());
            Process.Start(location);
            ToggleMenu(true);
        }

        #endregion

    }
}
