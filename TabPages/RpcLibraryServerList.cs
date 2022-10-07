//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NtApiDotNet.Win32;
using NtApiDotNet;
using System.Diagnostics;
using System.Linq;
using RpcInvestigator.TabPages;

namespace RpcInvestigator
{
    using static TraceLogger;

    public class RpcLibraryServerList : TabPage
    {
        public FastObjectListView m_Listview;
        private RpcLibrary m_Library;
        private TabManager m_Manager;

        public RpcLibraryServerList(RpcLibrary Library, TabManager Manager)
        {
            m_Manager = Manager;
            m_Library = Library;
            m_Listview = new FastObjectListView();
            Random random = new Random();
            string rand = random.Next().ToString();
            m_Listview.OwnerDraw = true;
            m_Listview.BorderStyle = BorderStyle.FixedSingle;
            m_Listview.CellEditUseWholeCell = false;
            m_Listview.Cursor = Cursors.Default;
            m_Listview.Dock = DockStyle.Fill;
            m_Listview.FullRowSelect = true;
            m_Listview.GridLines = true;
            m_Listview.Location = new Point(0, 0);
            m_Listview.Margin = new Padding(5);
            m_Listview.Name = "RpcLibraryServersListview" + rand;
            m_Listview.UseAlternatingBackColors = true;
            m_Listview.AlternateRowBackColor = Color.LightBlue;
            m_Listview.UseCompatibleStateImageBehavior = false;
            m_Listview.View = View.Details;
            m_Listview.VirtualMode = true;
            m_Listview.ShowGroups = false;
            m_Listview.UseFiltering = true;
            m_Listview.Alignment = ListViewAlignment.Left;
            m_Listview.HideSelection = false;
            //
            // When a listview row is double-clicked, a new tab will open with endpoints
            // for the selected RPC server. When a row is right-clicked,
            // a context menu shows.
            //
            m_Listview.DoubleClick += new EventHandler((object obj, EventArgs e2) =>
            {
                if (m_Listview.SelectedObjects == null ||
                    m_Listview.SelectedObjects.Count == 0)
                {
                    return;
                }
                var selectedRow = m_Listview.SelectedObjects.Cast<RpcServer>().ToList()[0];
                m_Manager.LoadRpcEndpointsTab(
                    selectedRow.Name, selectedRow.Endpoints.ToList(), selectedRow.Name);
            });
            m_Listview.CellRightClick += RightClickHandler;
            Generator.GenerateColumns(m_Listview, typeof(RpcServer), true);
            foreach (var column in m_Listview.AllColumns)
            {
                if (column.Name.ToLower() == "transfersyntaxid" ||
                    column.Name.ToLower() == "transfersyntaxversion" ||
                    column.Name.ToLower() == "procedures" ||
                    column.Name.ToLower() == "server" ||
                    column.Name.ToLower() == "complextypes" ||
                    column.Name.ToLower() == "offset" ||
                    column.Name.ToLower() == "endpoints" ||
                    column.Name.ToLower() == "client")
                {
                    column.IsVisible = false;
                }
                column.MaximumWidth = -1;
            }
            Controls.Add(m_Listview);
        }

        public void ScrollToServer(Guid InterfaceId)
        {
            if (m_Listview.Objects == null)
            {
                return;
            }

            var servers = m_Listview.Objects.Cast<RpcServer>().ToList();
            for (int i = 0; i < servers.Count; i++)
            {
                if (servers[i].InterfaceId.Equals(InterfaceId))
                {
                    m_Listview.EnsureModelVisible(servers[i]);
                    m_Listview.SelectObject(servers[i]);
                    return;
                }
            }
            MessageBox.Show("RPC server " + InterfaceId.ToString() + " doesn't exist " +
                "in the library.");
        }

        public async Task<bool> Build(
            RpcLibraryFilter Filter,
            ToolStripProgressBar ProgressBar,
            ToolStripStatusLabel ProgressLabel
            )
        {
            Text = "Servers";
            Name = "Servers";
            ImageIndex = 3;
            try
            {
                using (NtToken token = NtProcess.Current.OpenToken())
                {
                    ProgressBar.Step = 1;
                    ProgressBar.Value = 0;
                    ProgressBar.Visible = true;
                    var results = new List<RpcServer>();
                    await Task.Run(() =>
                    {
                        results = m_Library.Find(Filter);
                    });
                    if (results.Count > 0)
                    {
                        m_Listview.SetObjects(results);
                        m_Listview.AutoResizeColumns();
                        m_Listview.RebuildColumns();
                    }
                    ProgressBar.Value = 0;
                    ProgressBar.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcLibraryServerList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC library server list: " + ex.Message);
            }
            return true;
        }

        public int GetCount()
        {
            if (m_Listview.Objects == null)
            {
                return 0;
            }
            return m_Listview.Objects.Cast<RpcServer>().Count();
        }

        private
        void
        RightClickHandler(
            object Obj,
            CellRightClickEventArgs Args
            )
        {
            TabPages.ContextMenu.BuildRightClickMenu(Args, new List<ToolStripMenuItem>{
                new ToolStripMenuItem("New Client", null, ContextMenuNewClient),
                new ToolStripMenuItem("Reset", null, ContextMenuResetSearch)
            });
            TabPages.ContextMenu.AddSearchElements(Args);
        }

        private void ContextMenuResetSearch(object Sender, EventArgs Args)
        {
            m_Listview.ModelFilter = null;
        }

        private void ContextMenuNewClient(object Sender, EventArgs Args)
        {
            object tag = ((ToolStripMenuItem)Sender).Tag;
            if (tag == null)
            {
                return;
            }

            var args = (CellRightClickEventArgs)tag;

            //
            // We need to find the corresponding RpcServer object in the library.
            //
            var server = args.Model as RpcServer;
            var id = server.InterfaceId;
            var version = server.InterfaceVersion;
            var match = m_Library.Get(id, version);
            if (match == null)
            {
                MessageBox.Show("Unable to locate RPC server in library with id " +
                    id.ToString() + ", version " + version.ToString());
                return;
            }
            var endpoint = RpcAlpcServerList.FindEndpoint(id, version);
            if (endpoint == null)
            {
                MessageBox.Show("Unable to locate an endpoint for RPC server id " +
                    id.ToString() + ", version " + version.ToString());
                return;
            }
            var clientWindow = new Client(server, endpoint);
            clientWindow.ShowDialog();
        }
    }
}
