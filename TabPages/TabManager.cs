using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

        public void LoadRpcAlpcServersTab()
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
        }

        public void LoadRpcServersTab(List<string> FileNames)
        {
            RpcServerList rpcTab = new RpcServerList(this);
            m_TabControl.TabPages.Add(rpcTab);
            rpcTab.Build(FileNames, m_Settings, m_Library);
            m_TabControl.SelectedTab = rpcTab;
            m_StatusLabel.Text = "Discovered " + rpcTab.GetCount() + " RPC servers.";
        }

        public void LoadRpcEndpointsTab(string ServerName, List<RpcEndpoint> Endpoints, string AlpcPortName)
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
        }

        public void LoadRpcProceduresTab(string Name, List<NdrProcedureDefinition> Procedures)
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
            m_StatusLabel.Text = "Discovered " + proceduresTab.GetCount() + " procedures.";
        }

        public async void LoadRpcLibraryServersTab(RpcLibraryFilter Filter)
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
            m_StatusLabel.Text = "Discovered " + libraryServersTab.GetCount() + " library servers.";
        }

        public async void LoadRpcLibraryProceduresTab(RpcLibraryFilter Filter)
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
            m_StatusLabel.Text = "Discovered " + libraryProceduresTab.GetCount() + " library procedures.";
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
    }
}
