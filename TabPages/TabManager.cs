//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace RpcInvestigator.TabPages
{
    public class TabManager
    {
        private TabControl m_TabControl;
        private Settings m_Settings;
        private RpcLibrary m_Library;
        private ToolStripProgressBar m_ProgressBar;
        private ToolStripStatusLabel m_StatusLabel;

        public TabManager (
            TabControl Control,
            Settings Settings,
            RpcLibrary Library,
            ToolStripProgressBar Bar,
            ToolStripStatusLabel Label
            )
        {
            m_TabControl = Control;
            m_Settings = Settings;
            m_Library = Library;
            m_ProgressBar = Bar;
            m_StatusLabel = Label;
        }

        public RpcAlpcServerList LoadRpcAlpcServersTab()
        {
            TabPage tab;
            RpcAlpcServerList rpcAlpcTab;

            if (!TabExists("RPC ALPC Servers by Process", out tab))
            {
                tab = new RpcAlpcServerList(this);
                m_TabControl.TabPages.Add(tab);
            }
            rpcAlpcTab = tab as RpcAlpcServerList;
            rpcAlpcTab.Build();
            m_TabControl.SelectedTab = rpcAlpcTab;
            m_StatusLabel.Text = "Discovered " + rpcAlpcTab.GetCount() + " RPC ALPC servers.";
            return rpcAlpcTab;
        }

        public RpcServerList LoadRpcServersTab(List<string> FileNames)
        {
            RpcServerList rpcTab = new RpcServerList(this);
            m_TabControl.TabPages.Add(rpcTab);
            rpcTab.Build(FileNames, m_Settings, m_Library);
            m_TabControl.SelectedTab = rpcTab;
            m_StatusLabel.Text = "Discovered " + rpcTab.GetCount() + " RPC servers.";
            return rpcTab;
        }

        public RpcEndpointList LoadRpcEndpointsTab(string ServerName, List<RpcEndpoint> Endpoints, string AlpcPortName)
        {
            TabPage tab;
            RpcEndpointList endpointsTab;

            if (!TabExists(ServerName, out tab))
            {
                tab = new RpcEndpointList(m_Library, this);
                m_TabControl.TabPages.Add(tab);
            }
            endpointsTab = tab as RpcEndpointList;
            endpointsTab.Build(ServerName, Endpoints);
            m_TabControl.SelectedTab = endpointsTab;
            m_StatusLabel.Text = "Discovered " + endpointsTab.GetCount() + " endpoints.";
            return endpointsTab;
        }

        public TabPage GetSelectedTabPage()
        {
            return m_TabControl.SelectedTab;
        }
        public RpcProcedureList LoadRpcProceduresTab(string Name, List<NdrProcedureDefinition> Procedures)
        {
            TabPage tab;
            RpcProcedureList proceduresTab;
            string name = Name + " Procedures";
            if (!TabExists(name, out tab))
            {
                tab = new RpcProcedureList();
                m_TabControl.TabPages.Add(tab);
            }
            proceduresTab = tab as RpcProcedureList;
            proceduresTab.Build(Name, Procedures);
            m_TabControl.SelectedTab = proceduresTab;
            m_StatusLabel.Text = "Discovered " + proceduresTab.GetCount() +
                " procedures.";
            return proceduresTab;
        }

        public async Task<RpcLibraryServerList> LoadRpcLibraryServersTab(RpcLibraryFilter Filter)
        {
            TabPage tab;
            RpcLibraryServerList libraryServersTab;

            if (!TabExists("Servers", out tab))
            {
                tab = new RpcLibraryServerList(m_Library, this);
                m_TabControl.TabPages.Add(tab);
            }
            libraryServersTab = tab as RpcLibraryServerList;
            _ = await libraryServersTab.Build(Filter, m_ProgressBar, m_StatusLabel);
            m_TabControl.SelectedTab = libraryServersTab;
            m_StatusLabel.Text = "Loaded " + libraryServersTab.GetCount() +
                " servers from the library";
            return libraryServersTab;
        }

        public async Task<RpcLibraryProcedureList> LoadRpcLibraryProceduresTab(RpcLibraryFilter Filter)
        {
            TabPage tab;
            RpcLibraryProcedureList libraryProceduresTab;

            if (!TabExists("Procedures", out tab))
            {
                tab = new RpcLibraryProcedureList(m_Library);
                m_TabControl.TabPages.Add(tab);
            }
            libraryProceduresTab = tab as RpcLibraryProcedureList;
            _ = await libraryProceduresTab.Build(Filter, m_ProgressBar, m_StatusLabel);
            m_TabControl.SelectedTab = libraryProceduresTab;
            m_StatusLabel.Text = "Loaded " + libraryProceduresTab.GetCount() +
                " procedures from the library";
            return libraryProceduresTab;
        }

        public RpcLibraryProcedureList LoadRpcLibraryProceduresTab(RpcServer Server)
        {
            TabPage tab;
            RpcLibraryProcedureList libraryProceduresTab;
            var tabName = "Procedures for ";
            if (!string.IsNullOrEmpty(Server.ServiceName))
            {
                tabName += Server.ServiceName;
            }
            else if (!string.IsNullOrEmpty(Server.Name))
            {
                tabName += Server.Name;
            }
            else
            {
                tabName += Server.InterfaceId.ToString();
            }

            if (!TabExists(tabName, out tab))
            {
                tab = new RpcLibraryProcedureList(m_Library);
                tab.Text = tabName; // override name
                m_TabControl.TabPages.Add(tab);
            }
            libraryProceduresTab = tab as RpcLibraryProcedureList;
            _ = libraryProceduresTab.Build(Server);
            m_TabControl.SelectedTab = libraryProceduresTab;
            m_StatusLabel.Text = "Loaded " + libraryProceduresTab.GetCount() +
                " procedures from the library";
            return libraryProceduresTab;
        }

        public bool TabExists(string Name, out TabPage Match)
        {
            Match = null;
            foreach (var tab in m_TabControl.TabPages)
            {
                var tabPage = tab as TabPage;
                if (tabPage.Name == Name)
                {
                    Match = tabPage;
                    return true;
                }
            }
            return false;
        }

        public bool IsLibraryEmpty()
        {
            return m_Library.GetServerCount() == 0;
        }
    }
}
