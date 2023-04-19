//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
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
using System.Windows.Forms;

namespace RpcInvestigator
{
    using static TraceLogger;

    public class RpcServerList : TabPage
    {
        public FastObjectListView m_Listview;
        private TabManager m_Manager;

        public RpcServerList(TabManager Manager)
        {
            m_Manager = Manager;
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
            m_Listview.Name = "RpcServerListview" + rand;
            m_Listview.UseAlternatingBackColors = true;
            m_Listview.AlternateRowBackColor = Color.LightBlue;
            m_Listview.UseCompatibleStateImageBehavior = false;
            m_Listview.View = View.Details;
            m_Listview.VirtualMode = true;
            m_Listview.ShowGroups = false;
            m_Listview.Alignment = ListViewAlignment.Left;
            Generator.GenerateColumns(m_Listview, typeof(RpcServer), true);
            foreach (var column in m_Listview.AllColumns)
            {
                if (column.Name.ToLower() == "procedures" ||
                    column.Name.ToLower() == "complextypes" ||
                    column.Name.ToLower() == "name" ||
                    column.Name.ToLower() == "offset" ||
                    column.Name.ToLower() == "endpoints")
                {
                    column.IsVisible = false;
                }
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
                var selectedRow = m_Listview.SelectedObjects.Cast<RpcServer>().ToList()[0];
                m_Manager.LoadRpcProceduresTab(selectedRow.Name, selectedRow.Procedures.ToList());
            });
            m_Listview.CellRightClick += RightClickHandler;
            Controls.Add(m_Listview);
        }

        public void Build (
            List<string> FileNames,
            Settings Settings,
            RpcLibrary Library
            )
        {
            Text = BuildTabTitle(FileNames);
            ImageIndex = 0;
            try
            {
                using (NtToken token = NtProcess.Current.OpenToken())
                {
                    var allservers = new List<RpcServer>();
                    token.SetPrivilege(TokenPrivilegeValue.SeDebugPrivilege, PrivilegeAttributes.Enabled, true);
                    foreach (var filename in FileNames)
                    {
                        try
                        {
                            var servers = RpcServer.ParsePeFile(filename,
                                    Settings.m_DbghelpPath, Settings.m_SymbolPath);
                            if (servers.Count() == 0)
                            {
                                Trace(TraceLoggerType.RpcServerList,
                                    TraceEventType.Warning,
                                    "No RPC servers in file '" + filename + "'");
                                continue;
                            }
                            allservers.AddRange(servers);
                        }
                        catch (Exception ex)
                        {
                            Trace(TraceLoggerType.RpcServerList,
                                TraceEventType.Error,
                                "Unable to retrieve RPC server list for file '" +
                                filename + "': " + ex.Message);
                        }
                    }

                    if (allservers.Count > 0)
                    {
                        m_Listview.ClearObjects();
                        m_Listview.SetObjects(allservers);
                        m_Listview.RebuildColumns();
                        m_Listview.AutoResizeColumns();

                        //
                        // Cache any new RPC servers from this binary.
                        //
                        if (!Library.Merge(allservers))
                        {
                            Trace(TraceLoggerType.RpcServerList,
                                TraceEventType.Error,
                                "Unable to merge new RPC servers to database.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcServerList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC server list: " + ex.Message);
            }
        }

        public int GetCount()
        {
            if (m_Listview.Objects == null)
            {
                return 0;
            }
            return m_Listview.Objects.Cast<RpcServer>().Count();
        }

        public List<RpcServer> GetAll()
        {
            if (m_Listview.Objects == null)
            {
                return new List<RpcServer>();
            }
            return m_Listview.Objects.Cast<RpcServer>().ToList();
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
            // RpcServer type comes from parsing RPC server information out of a
            // binary (such as a DLL), so we have to scan for an ALPC port hosting
            // the interface using the RPC endpoint mapper.
            //
            var server = args.Model as RpcServer;
            var id = server.InterfaceId;
            var version = server.InterfaceVersion;
            var endpoint = RpcAlpcServerList.FindEndpoint(id, version);
            if (endpoint == null)
            {
                MessageBox.Show("Unable to locate an endpoint for RPC server UUID " +
                    id.ToString() + ", version " + version.ToString());
                return;
            }
            var clientWindow = new Client(server, endpoint, m_Manager.m_Settings);
            clientWindow.ShowDialog();
        }

        private static string BuildTabTitle(List<string> FileNames)
        {
            if (FileNames.Count == 1)
            {
                var path = Path.GetDirectoryName(FileNames[0]);
                string title = FileNames[0];
                if (path.Length > 25)
                {
                    title = path.Substring(0, 20) +
                        "...\\" + Path.GetFileName(FileNames[0]);
                }
                return title;
            }
            return "Multiple files";
        }
    }
}
