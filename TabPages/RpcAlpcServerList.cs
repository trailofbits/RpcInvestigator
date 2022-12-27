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
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Newtonsoft.Json.Linq;
using RpcInvestigator.Util;
using System.ServiceModel.Channels;
using System.Text;
using RpcInvestigator.Windows;

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
            m_Listview.Alignment = ListViewAlignment.Left;
            Generator.GenerateColumns(m_Listview, typeof(RpcAlpcServer), true);
            foreach (var column in m_Listview.AllColumns)
            {
                if (column.Name.ToLower() == "endpoints")
                {
                    column.IsVisible = false;
                }
                column.MaximumWidth = -1;
            }

            m_Listview.AllColumns.ForEach(col =>
            {
                if (col.Name == "SecurityDescriptor")
                {
                    col.IsVisible = false;
                }
            });

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
            Controls.Add(m_Listview);
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
            });
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
