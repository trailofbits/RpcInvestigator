//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using System.Linq;
using NtApiDotNet.Win32;
using NtApiDotNet;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Drawing;
using System.Diagnostics;
using RpcInvestigator.TabPages;

namespace RpcInvestigator
{
    using static TraceLogger;

    public class RpcAlpcServerList : TabPage
    {
        public FastObjectListView m_Listview;
        private TabManager m_TabManager;

        public RpcAlpcServerList(TabManager Manager)
        {
            m_TabManager = Manager;
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
            m_Listview.Name = "RpcAlpcServerListview" + rand;
            m_Listview.UseAlternatingBackColors = true;
            m_Listview.AlternateRowBackColor = Color.LightBlue;
            m_Listview.UseCompatibleStateImageBehavior = false;
            m_Listview.View = View.Details;
            m_Listview.VirtualMode = true;
            m_Listview.ShowGroups = false;
            m_Listview.UseFiltering = true;
            m_Listview.Alignment = ListViewAlignment.Left;
            Generator.GenerateColumns(m_Listview, typeof(RpcAlpcServer), true);
            foreach (var column in m_Listview.AllColumns)
            {
                if (column.Name.ToLower() == "endpoints" ||
                    column.Name.ToLower() == "securitydescriptor")
                {
                    column.IsVisible = false;
                }
                if (column.Name.ToLower() == "securitydescriptor")
                {
                    column.AspectGetter = delegate (object Row)
                    {
                        if (Row == null)
                        {
                            return "";
                        }
                        var server = Row as RpcAlpcServer;
                        if (server.SecurityDescriptor == null)
                        {
                            return "";
                        }
                        return server.SecurityDescriptor.ToString();
                    };
                }
                column.MaximumWidth = -1;
            }

            //
            // When a listview row is double-clicked, a new tab will open with endpoints
            // for the selected RPC server. Right-click shows context menu.
            //
            m_Listview.DoubleClick += new EventHandler((object obj, EventArgs e2) =>
            {
                if (m_Listview.SelectedObjects == null ||
                    m_Listview.SelectedObjects.Count == 0)
                {
                    return;
                }
                var selectedRow = m_Listview.SelectedObjects.Cast<RpcAlpcServer>().ToList()[0];
                m_TabManager.LoadRpcEndpointsTab(
                    selectedRow.Name, selectedRow.Endpoints.ToList(), selectedRow.Name);
            });
            m_Listview.CellRightClick += RightClickHandler;
            m_Listview.ColumnRightClick += ColumnRightClickOverride;
            Controls.Add(m_Listview);
        }

        private void ColumnRightClickOverride(object sender, ColumnClickEventArgs e)
        {
            //
            // Some of the columns we hide by default should never be shown,
            // either because they contain binary data or lists of binary data.
            // Such information is not easy to disable in the listview, so we'll
            // strip those columns from the column selector context menu.
            //
            var args = (ColumnRightClickEventArgs)e;
            var list = new List<ToolStripItem>();
            foreach (var item in args.MenuStrip.Items)
            {
                if (item.GetType() == typeof(ToolStripMenuItem))
                {
                    var toolstripItem = item as ToolStripMenuItem;
                    if (toolstripItem.Text.ToLower() == "endpoints")
                    {
                        continue;
                    }
                }
                list.Add((ToolStripItem)item);
            }
            args.MenuStrip.Items.Clear();
            args.MenuStrip.Items.AddRange(list.ToArray());
        }

        public int GetCount()
        {
            if (m_Listview.Objects == null)
            {
                return 0;
            }
            return m_Listview.Objects.Cast<RpcAlpcServer>().Count();
        }

        public List<RpcAlpcServer> GetAll()
        {
            if (m_Listview.Objects == null)
            {
                return new List<RpcAlpcServer>();
            }
            return m_Listview.Objects.Cast<RpcAlpcServer>().ToList();
        }

        public void Build()
        {
            IEnumerable<RpcAlpcServer> servers;
            Text = "RPC ALPC Servers by Process";
            Name = "RPC ALPC Servers by Process";
            ImageIndex = 0;
            try
            {
                using (NtToken token = NtProcess.Current.OpenToken())
                {
                    token.SetPrivilege(TokenPrivilegeValue.SeDebugPrivilege, PrivilegeAttributes.Enabled, true);
                    servers = RpcAlpcServer.GetAlpcServers();
                    if (servers.Count() == 0)
                    {
                        Trace(TraceLoggerType.RpcAlpcServerList,
                            TraceEventType.Error,
                            "No RPC servers available.");
                    }
                    else
                    {
                        Trace(TraceLoggerType.RpcAlpcServerList,
                            TraceEventType.Information,
                            "Retrieved " + servers.Count() + " RPC ALPC servers.");
                        m_Listview.ClearObjects();
                        m_Listview.SetObjects(servers);
                        m_Listview.AutoResizeColumns();
                        m_Listview.RebuildColumns();
                        m_Listview.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcAlpcServerList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC server list: " + ex.Message);
            }
        }

        public
        static
        RpcEndpoint
        FindEndpoint(
            Guid InterfaceId,
            Version InterfaceVersion
            )
        {
            var alpcServers = RpcAlpcServer.GetAlpcServers().ToList();
            RpcEndpoint match = null;

            if (alpcServers.Count() == 0)
            {
                Trace(TraceLoggerType.RpcAlpcServerList,
                    TraceEventType.Warning,
                    "FindEndpoint: No RPC ALPC servers available.");
                return null;
            }

            foreach (var server in alpcServers)
            {
                match = server.Endpoints.FirstOrDefault(endpoint =>
                    endpoint.InterfaceId.Equals(InterfaceId) &&
                    endpoint.InterfaceVersion.Equals(InterfaceVersion));
                if (match != null)
                {
                    break;
                }
            }

            if (match == null)
            {
                Trace(TraceLoggerType.RpcAlpcServerList,
                    TraceEventType.Warning,
                    "FindEndpoint: No ALPC server match for UUID " +
                    InterfaceId + " and version " + InterfaceVersion);
            }
            return match;
        }

        private
        void
        RightClickHandler(
            object Obj,
            CellRightClickEventArgs Args
            )
        {
            TabPages.ContextMenu.BuildRightClickMenu(Args, new List<ToolStripMenuItem>{
                new ToolStripMenuItem("Open in Library", null, ContextMenuOpenAlpcServerInLibrary),
                new ToolStripMenuItem("View Security Descriptor", null, TabPages.ContextMenu.ContextMenuViewSecurityDescriptor),
                new ToolStripMenuItem("View Procedures", null, ContextMenuViewProcedures),
            });
        }

        private void ContextMenuViewProcedures(object Sender, EventArgs Args)
        {
            object tag = ((ToolStripMenuItem)Sender).Tag;
            if (tag == null)
            {
                return;
            }
            var args = (CellRightClickEventArgs)tag;
            var server = args.Model as RpcAlpcServer;
            _ = m_TabManager.LoadRpcLibraryProceduresTab(new RpcLibraryFilter{
                FilterType = RpcLibraryFilterType.FilterByKeyword,
                Keyword = server.Name });
        }

        private
        async
        void
        ContextMenuOpenAlpcServerInLibrary(
            object Sender,
            EventArgs Args
            )
        {
            if (m_TabManager.IsLibraryEmpty())
            {
                MessageBox.Show("The library is empty. Please click the Refresh menu " +
                    "item under the Library menu to regenerate it.");
                return;
            }
            object tag = ((ToolStripMenuItem)Sender).Tag;
            if (tag == null)
            {
                return;
            }
            var args = (CellRightClickEventArgs)tag;
            var alpcServer = args.Model as RpcAlpcServer;
            var filter = new RpcLibraryFilter
            {
                FilterType = RpcLibraryFilterType.FilterByKeyword,
                Keyword = alpcServer.Name
            };
            _ = await m_TabManager.LoadRpcLibraryServersTab(filter);
        }
    }
}
