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
                        Trace.TraceWarning("No RPC servers available.");
                    }
                    else
                    {
                        Trace.TraceInformation("Retrieved " + servers.Count() + " RPC ALPC servers.");
                        m_Listview.ClearObjects();
                        m_Listview.SetObjects(servers);
                        m_Listview.AutoResizeColumns();
                        m_Listview.RebuildColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to retrieve RPC server list: " + ex.Message);
            }
        }

        public
        static
        RpcEndpoint
        FindEndpoint(
            string InterfaceId,
            string InterfaceVersion
            )
        {
            var alpcServers = RpcAlpcServer.GetAlpcServers().ToList();
            RpcEndpoint match = null;

            if (alpcServers.Count() == 0)
            {
                Trace.TraceWarning("FindEndpoint: No RPC ALPC servers available.");
                return null;
            }

            foreach (var server in alpcServers)
            {
                match = server.Endpoints.FirstOrDefault(endpoint =>
                    endpoint.InterfaceId.Equals(new Guid(InterfaceId)) &&
                    endpoint.InterfaceVersion.Equals(new Version(InterfaceVersion)));
                if (match != null)
                {
                    break;
                }
            }

            if (match == null)
            {
                Trace.TraceWarning("FindEndpoint: No ALPC server match for UUID " +
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
            });
        }

        private
        void
        ContextMenuOpenAlpcServerInLibrary(
            object Sender,
            EventArgs Args
            )
        {
            object tag = ((ToolStripMenuItem)Sender).Tag;
            if (tag == null)
            {
                return;
            }
            var args = (CellRightClickEventArgs)tag;
            var alpcServer = args.Model as RpcAlpcServer;
            if (alpcServer.Endpoints == null ||
                alpcServer.Endpoints.Count() == 0)
            {
                return;
            }

            var filter = new RpcLibraryFilter
            {
                FilterType = RpcLibraryFilterType.FilterByInterfaceIdAndVersion,
                InterfaceIdAndVersion = new Dictionary<string, string>()
            };

            alpcServer.Endpoints.ToList().ForEach(endpoint =>
            {
                filter.InterfaceIdAndVersion.Add(
                    endpoint.InterfaceId.ToString(),
                    endpoint.InterfaceVersion.ToString());
            });

            m_TabManager.LoadRpcLibraryServersTab(filter);
        }
    }
}
