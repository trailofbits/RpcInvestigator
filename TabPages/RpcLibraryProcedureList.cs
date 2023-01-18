//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using BrightIdeasSoftware;
using NtApiDotNet.Ndr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NtApiDotNet.Win32;
using System.Diagnostics;
using System.Linq;

namespace RpcInvestigator
{
    using static TraceLogger;

    public class RpcLibraryProcedureList : TabPage
    {
        public FastObjectListView m_Listview;
        private RpcLibrary m_Library;

        public RpcLibraryProcedureList(RpcLibrary Library)
        {
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
            m_Listview.Name = "RpcLibraryProceduresListview" + rand;
            m_Listview.UseAlternatingBackColors = true;
            m_Listview.AlternateRowBackColor = Color.LightBlue;
            m_Listview.UseCompatibleStateImageBehavior = false;
            m_Listview.View = View.Details;
            m_Listview.VirtualMode = true;
            m_Listview.ShowGroups = false;
            m_Listview.UseFiltering = true;
            m_Listview.Alignment = ListViewAlignment.Left;
            Generator.GenerateColumns(m_Listview, typeof(RpcLibraryProcedure), true);
            foreach (var column in m_Listview.AllColumns)
            {
                column.MaximumWidth = -1;
            }
            m_Listview.CellRightClick += RightClickHandler;
            Controls.Add(m_Listview);
        }

        public async Task<bool> Build(
            RpcLibraryFilter Filter,
            ToolStripProgressBar ProgressBar,
            ToolStripStatusLabel ProgressLabel
            )
        {
            var tabName = "Procedures";
            if (Filter.FilterType != RpcLibraryFilterType.NoFilter)
            {
                tabName += " for '" + Filter.Keyword + "'";
            }
            Text = tabName;
            Name = tabName;
            ImageIndex = 3;
            try
            {
                var searchResults = new List<RpcLibraryProcedure>();
                ProgressBar.Step = 1;
                ProgressBar.Value = 0;
                ProgressBar.Visible = true;
                await Task.Run(() =>
                {
                    var results = m_Library.Find(Filter);
                    foreach (var server in results)
                    {
                        foreach (var procedure in server.Procedures)
                        {
                            var formatter = DefaultNdrFormatter.Create(DefaultNdrFormatterFlags.RemoveComments);
                            string friendly = formatter.FormatProcedure(procedure);
                            searchResults.Add(new RpcLibraryProcedure()
                            {
                                FilePath = server.FilePath,
                                InterfaceId = server.InterfaceId,
                                InterfaceVersion = server.InterfaceVersion,
                                Name = friendly,
                            });
                        };
                    }
                });

                if (searchResults.Count > 0)
                {
                    m_Listview.SetObjects(searchResults);
                    m_Listview.AutoResizeColumns();
                    m_Listview.RebuildColumns();
                }
                ProgressBar.Value = 0;
                ProgressBar.Visible = false;
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcLibraryProcedureList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC library procedure list: " + ex.Message);
            }
            return true;
        }

        public bool Build(RpcServer Server)
        {
            Text = "Procedures";
            Name = "Procedures";
            ImageIndex = 3;
            var procs = new List<RpcLibraryProcedure>();
            try
            {
                foreach (var procedure in Server.Procedures)
                {
                    var formatter = DefaultNdrFormatter.Create(DefaultNdrFormatterFlags.RemoveComments);
                    string friendly = formatter.FormatProcedure(procedure);
                    procs.Add(new RpcLibraryProcedure()
                    {
                        FilePath = Server.FilePath,
                        InterfaceId = Server.InterfaceId,
                        InterfaceVersion = Server.InterfaceVersion,
                        Name = friendly,
                    });
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcLibraryProcedureList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC library procedure list: " + ex.Message);
            }
            if (procs.Count > 0)
            {
                m_Listview.SetObjects(procs);
                m_Listview.AutoResizeColumns();
                m_Listview.RebuildColumns();
            }
            return true;
        }

        public int GetCount()
        {
            if (m_Listview.Objects == null)
            {
                return 0;
            }
            return m_Listview.Objects.Cast<RpcLibraryProcedure>().Count();
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
            var procedure = args.Model as RpcLibraryProcedure;
            var id = procedure.InterfaceId;
            var version = procedure.InterfaceVersion;
            var server = m_Library.Get(id, version);
            if (server == null)
            {
                MessageBox.Show("Unable to locate RPC server in library with UUID " + id +
                    " and version " + version);
                return;
            }
            var endpoint = RpcAlpcServerList.FindEndpoint(id, version);
            if (endpoint == null)
            {
                MessageBox.Show("Unable to locate an endpoint for RPC server UUID " +
                    id + " and version " + version);
                return;
            }
            var clientWindow = new Client(server, endpoint);
            clientWindow.ShowDialog();
        }
    }

    public class RpcLibraryProcedure
    {
        public string FilePath { get; set; }
        public Guid InterfaceId { get; set; }
        public Version InterfaceVersion { get; set; }
        public string Name { get; set; }

    }
}
