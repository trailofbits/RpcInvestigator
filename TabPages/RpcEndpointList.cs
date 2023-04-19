//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using NtApiDotNet.Win32;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using NtApiDotNet;
using RpcInvestigator.TabPages;

namespace RpcInvestigator
{
    using static TraceLogger;

    public class RpcEndpointList : TabPage
    {
        public FastObjectListView m_Listview;
        private RpcLibrary m_Library;
        private TabManager m_Manager;

        public RpcEndpointList (RpcLibrary Library, TabManager Manager)
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
            m_Listview.HideSelection = false;
            m_Listview.Location = new Point(0, 0);
            m_Listview.Margin = new Padding(5);
            m_Listview.Name = "RpcEndpointListview" + rand;
            m_Listview.UseAlternatingBackColors = true;
            m_Listview.AlternateRowBackColor = Color.LightBlue;
            m_Listview.UseCompatibleStateImageBehavior = false;
            m_Listview.View = View.Details;
            m_Listview.VirtualMode = true;
            m_Listview.ShowGroups = false;
            m_Listview.Alignment = ListViewAlignment.Left;
            Generator.GenerateColumns(m_Listview, typeof(RpcEndpoint), true);
            foreach (var column in m_Listview.AllColumns)
            {
                column.MaximumWidth = -1;
            }
            //
            // When a listview row is double-clicked, a new tab will open with procedures
            // for the selected interface on the given RPC server. When a row is right-clicked,
            // a context menu shows.
            //
            m_Listview.DoubleClick += new EventHandler((object obj, EventArgs e2) =>
            {
                if (m_Listview.SelectedObjects == null ||
                    m_Listview.SelectedObjects.Count == 0)
                {
                    return;
                }
                var selectedRow = m_Listview.SelectedObjects.Cast<RpcEndpoint>().ToList()[0];
                var id = selectedRow.InterfaceId;
                var version = selectedRow.InterfaceVersion;
                var server = m_Library.Get(id, version);
                if (server == null)
                {
                    MessageBox.Show("Unable to locate RPC server " +
                        id.ToString() + ", version " + version.ToString());
                    return;
                }

                m_Manager.LoadRpcProceduresTab(server.Name, server.Procedures.ToList());
            });
            m_Listview.CellRightClick += RightClickHandler;
            Controls.Add(m_Listview);
        }

        public void Build(string Name, List<RpcEndpoint> Endpoints)
        {
            this.Name = Name;
            Text = BuildTabTitle(Name);
            ImageIndex = 1;
            try
            {
                using (NtToken token = NtProcess.Current.OpenToken())
                {
                    if (Endpoints.Count > 0)
                    {
                        m_Listview.ClearObjects();
                        m_Listview.SetObjects(Endpoints);
                        m_Listview.RebuildColumns();
                        m_Listview.AutoResizeColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcEndpointList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC endpoint list: " + ex.Message);
            }
        }

        public int GetCount()
        {
            if (m_Listview.Objects == null)
            {
                return 0;
            }
            return m_Listview.Objects.Cast<RpcEndpoint>().Count();
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
            });
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
            // We have the endpoint, we just need to find the corresponding
            // RpcServer object in the library.
            //
            var endpoint = args.Model as RpcEndpoint;
            var id = endpoint.InterfaceId;
            var version = endpoint.InterfaceVersion;
            var server = m_Library.Get(id, version);
            if (server == null)
            {
                MessageBox.Show("Unable to locate an RPC server definition for id " +
                    id.ToString() + ", version " + version.ToString());
                return;
            }
            var clientWindow = new Client(server, endpoint, m_Manager.m_Settings);
            clientWindow.ShowDialog();
        }

        private static string BuildTabTitle(string Name)
        {
            var name = Name;
            if (name.Length > 30)
            {
                name = name.Substring(0, 15) + "..." + name.Substring(
                    name.Length - 15, 15);
            }
            return "Endpoints for " + name;
        }
    }
}
