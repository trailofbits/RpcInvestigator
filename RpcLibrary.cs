using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NtApiDotNet.Win32;

namespace RpcInvestigator
{
    public enum RpcLibraryFilterType
    {
        NoFilter,
        FilterByInterfaceId,
        FilterByInterfaceIdAndVersion,
        FilterByKeyword
    }

    public class RpcLibraryFilter
    {
        public RpcLibraryFilterType FilterType;
        public Dictionary<string, string> InterfaceIdAndVersion;
        public string Keyword;

        public RpcLibraryFilter()
        {
            FilterType = RpcLibraryFilterType.NoFilter;
            InterfaceIdAndVersion = new Dictionary<string, string>();
        }
    }

    [Serializable]
    public class RpcServerEntry
    {
        public RpcServerEntry()
        {
        }
        public string InterfaceVersion;
        public byte[] SerializedServer;
    }

    [Serializable]
    public class RpcServerData
    {
        //
        // Note: this class is NOT thread-safe.
        //

        public int m_Version;
        private Dictionary<string, List<RpcServerEntry>> m_Entries;

        public RpcServerData()
        {
            m_Version = 0;
            m_Entries = new Dictionary<string, List<RpcServerEntry>>();
        }

        public bool Add(RpcServer Server)
        {
            string id = Server.InterfaceId.ToString().ToUpper();
            string version = Server.InterfaceVersion.ToString();
            if (!m_Entries.ContainsKey(id))
            {
                m_Entries.Add(id, new List<RpcServerEntry>());
            }
            foreach (var server in m_Entries[id])
            {
                if (server.InterfaceVersion == version)
                {
                    Trace.TraceInformation("An RPC server with ID " + id +
                        " and version " + version + " already exists in the database.");
                    return false;
                }
            }

            m_Entries[id].Add(new RpcServerEntry()
            {
                InterfaceVersion = version,
                SerializedServer = Server.Serialize()
            });
            Trace.WriteLine("Library: Added RPC server UUID " + id +
                " and version " + version);
            return true;
        }

        public bool Remove(RpcServer Server)
        {
            string id = Server.InterfaceId.ToString().ToUpper();
            string version = Server.InterfaceVersion.ToString();
            var item = Get(id, version);
            if (item == null)
            {
                Trace.TraceWarning("Server " + id + ", Version " + version +
                    " does not exist in the data set.");
                return false;
            }
            if (!m_Entries[id].Remove(item))
            {
                Trace.TraceError("Failed to remove server with ID " + id);
                return false;
            }
            Trace.WriteLine("Library: Removed RPC server UUID " + id +
                " and version " + version);
            return true;
        }

        public RpcServerEntry Get(string InterfaceUuid, string InterfaceVersion)
        {
            var id = InterfaceUuid.ToUpper();
            if (!m_Entries.ContainsKey(id))
            {
                return null;
            }
            return m_Entries[id].FirstOrDefault(
                entry => entry.InterfaceVersion == InterfaceVersion);
        }

        public List<RpcServerEntry> Get(string InterfaceUuid)
        {
            var id = InterfaceUuid.ToUpper();
            if (!m_Entries.ContainsKey(id))
            {
                return null;
            }
            return m_Entries[id];
        }


        internal Dictionary<string, List<RpcServerEntry>> Get()
        {
            return m_Entries;
        }

        public int GetCount()
        {
            return m_Entries.Count;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in m_Entries)
            {
                sb.AppendLine("Server " + kvp.Key + " has " + kvp.Value.Count + " versions:");
                kvp.Value.ForEach(entry =>
                {
                    var server = RpcServer.Deserialize(entry.SerializedServer);
                    sb.AppendLine("   Server " + server.InterfaceId.ToString() +
                        " / " + server.InterfaceVersion.ToString());
                    sb.AppendLine("      FilePath        : " + server.FilePath);
                    sb.AppendLine("      Procedure Count : " + server.ProcedureCount);
                    sb.AppendLine("      Service Name: " + server.ServiceName);
                    sb.AppendLine("      Endpoints:");
                    foreach (var endpoint in server.Endpoints.ToList())
                    {
                        sb.AppendLine("         " + endpoint.InterfaceId.ToString() +
                            " / " + endpoint.InterfaceVersion.ToString() +
                            " - " + endpoint.BindingString);
                    }
                });
            }
            return sb.ToString();
        }
    }

    public class RpcLibrary
    {
        private static string m_DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RpcInvestigator");
        private static string m_DefaultRpcServerDb = "rpcserver.db";
        private readonly string m_Path;
        private RpcServerData m_Data;

        public RpcLibrary(string TargetPath)
        {
            m_Path = TargetPath;
            if (string.IsNullOrEmpty(m_Path))
            {
                m_Path = Path.Combine(m_DefaultPath, m_DefaultRpcServerDb);
            }
            m_Data = new RpcServerData();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Database '" + m_Path + "' with " + m_Data.GetCount() + " entries.");
            sb.AppendLine("Generated on " + DateTime.Now);
            sb.AppendLine(m_Data.ToString());
            return sb.ToString();
        }

        public void Load()
        {
            if (!File.Exists(m_Path))
            {
                try
                {
                    File.Create(m_Path).Close();
                    Trace.WriteLine("Library: successfully created '" + m_Path + "'");
                }
                catch (Exception ex) // swallow
                {
                    Trace.TraceError("Failed to create database '" +
                        m_Path + "': " + ex.Message);
                }
            }
            else
            {
                try
                {
                    using (var stream = File.OpenRead(m_Path))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        m_Data = (RpcServerData)formatter.Deserialize(stream);
                        Trace.WriteLine("Library: successfully loaded '" + m_Path + "'");
                        Trace.WriteLine("Library: " + GetServerCount() + " entries.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to load database '" + m_Path +
                        "': " + ex.Message);
                }
            }
        }

        public void Save()
        {
            try
            {
                using (var stream = File.Open(m_Path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, m_Data);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save '" + m_Path + "': " + ex.Message);
            }
            Trace.WriteLine("Library: successfully saved '" + m_Path + "'");
        }

        public bool Add(RpcServer Server)
        {
            return m_Data.Add(Server);
        }

        public bool Remove(RpcServer Server)
        {
            return m_Data.Remove(Server);
        }

        public RpcServer Get(string InterfaceUuid, string InterfaceVersion)
        {
            var item = m_Data.Get(InterfaceUuid, InterfaceVersion);
            if (item == null)
            {
                return null;
            }
            return RpcServer.Deserialize(item.SerializedServer);
        }


        public List<RpcServer> Find(RpcLibraryFilter Filter)
        {
            var matches = new List<RpcServer>();
            var interfaceAndVersion = Filter.InterfaceIdAndVersion;
            var keyword = Filter.Keyword;

            switch (Filter.FilterType)
            {
                case RpcLibraryFilterType.FilterByInterfaceId:
                    {
                        foreach (var kvp in interfaceAndVersion)
                        {
                            var match = m_Data.Get(kvp.Key);
                            if (match != null)
                            {
                                match.ForEach(
                                    m => matches.Add(
                                        RpcServer.Deserialize(m.SerializedServer)));
                            }
                        }
                        break;
                    }
                case RpcLibraryFilterType.FilterByInterfaceIdAndVersion:
                    {
                        foreach (var kvp in interfaceAndVersion)
                        {
                            var match = m_Data.Get(kvp.Key, kvp.Value);
                            if (match != null)
                            {
                                matches.Add(RpcServer.Deserialize(match.SerializedServer));
                            }
                        }
                        break;
                    }
                case RpcLibraryFilterType.FilterByKeyword:
                    {
                        var all = m_Data.Get();
                        foreach (var kvp in all)
                        {
                            foreach (var serverEntry in kvp.Value)
                            {
                                var server = RpcServer.Deserialize(serverEntry.SerializedServer);
                                if ((!string.IsNullOrEmpty(server.ServiceDisplayName) &&
                                     server.ServiceDisplayName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(server.ServiceName) &&
                                     server.ServiceName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(server.FilePath) &&
                                     server.FilePath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (server.InterfaceId.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                                {
                                    matches.Add(server);
                                }
                            }
                        }
                        break;
                    }
                default:
                    {
                        var all = m_Data.Get();
                        foreach (var kvp in all)
                        {
                            foreach (var serverEntry in kvp.Value)
                            {
                                var server = RpcServer.Deserialize(serverEntry.SerializedServer);
                                matches.Add(server);
                            }
                        }
                        break;
                    }
            }

            return matches;
        }

        public int GetServerCount()
        {
            return m_Data.GetCount();
        }

        public bool Merge(
            List<RpcServer> Servers
            )
        {
            if (Servers.Count() == 0)
            {
                Trace.TraceInformation("Merge: No RPC servers were provided.");
                return true;
            }

            //
            // Add to database and save.
            // Note: a failure to add means that RPC server is already in the
            // database, which is entirely possible.
            //
            int numAdded = 0;
            Servers.ForEach(server =>
            {
                if (Add(server))
                {
                    numAdded++;
                }
            });

            try
            {
                Save();
                Trace.TraceInformation("Merge: Added " + numAdded + " RPC servers.");
            }
            catch (Exception ex)
            {
                Trace.TraceError("An error occurred when saving the database: " +
                    ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> Refresh(
            Settings Settings,
            ToolStripProgressBar ProgressBar,
            ToolStripStatusLabel ProgressLabel
            )
        {
            int workSize = 16;
            ProgressBar.Step = 1;
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            bool success = false;

            var alpcServers = RpcAlpcServer.GetAlpcServers().ToList();

            if (alpcServers.Count() == 0)
            {
                Trace.TraceWarning("Refresh: No RPC ALPC servers available.");
                return true;
            }

            try
            {
                //
                // Get unique service process binaries from all loaded modules
                //
                int max = alpcServers.Count() / workSize;
                ProgressBar.Maximum = max;
                ProgressBar.Text = "Finding unique modules...";
                var allTaskResults = await TaskWorker.Run(
                    alpcServers,
                    workSize,
                    async (serverList) =>
                    {
                        var result = new TaskWorker.TaskWorkerResult<List<string>>();
                        result.Messages = new StringBuilder();
                        result.TaskResult = new List<string>();
                        try
                        {
                            result.TaskResult = new List<string>();
                            serverList.ForEach(server =>
                            {
                                result.TaskResult = result.TaskResult.Union(
                                    GetLoadedModulesForProcess(server.ProcessId)).ToList();
                            });
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError(ex.Message); // swallow
                        }
                        ProgressBar.GetCurrentParent().Invoke((MethodInvoker)(() =>
                        {
                            ProgressBar.PerformStep();
                            ProgressLabel.Text = "Processing ALPC server " +
                                (ProgressBar.Value * workSize) + " of " + (max * workSize);
                        }));
                        return result;
                    });
                var uniqueModules = new List<string>();
                allTaskResults.ForEach(
                    result => uniqueModules = uniqueModules.Union(result.TaskResult).ToList());
                Trace.TraceInformation("Refresh: Discovered " + uniqueModules.Count() +
                    " unique modules.");

                //
                // Get RPC servers from unique modules.
                //
                max = uniqueModules.Count() / workSize;
                ProgressBar.Maximum = max;
                ProgressBar.Text = "Processing " + max + " unique modules, please wait...";
                var allTaskResults2 = await TaskWorker.Run(
                    uniqueModules,
                    workSize,
                    async (moduleList) =>
                    {
                        var result = new TaskWorker.TaskWorkerResult<List<RpcServer>>();
                        result.Messages = new StringBuilder();
                        result.TaskResult = new List<RpcServer>();
                        moduleList.ForEach(module =>
                        {
                            try
                            {
                                var results = RpcServer.ParsePeFile(
                                    module, Settings.m_DbghelpPath, Settings.m_SymbolPath, false);
                                if (results != null && results.Count() > 0)
                                {
                                    result.TaskResult.AddRange(results);
                                }
                            }
                            catch (Exception ex) // swallow
                            {
                                Trace.TraceError("Unable to parse PE file '" +
                                    module + "': " + ex.Message);
                            }
                        });

                        ProgressBar.GetCurrentParent().Invoke((MethodInvoker)(() =>
                        {
                            ProgressBar.PerformStep();
                            ProgressLabel.Text = "Processing module " +
                                (ProgressBar.Value * workSize) + " of " + (max * workSize);
                        }));
                        return result;
                    });
                //
                // Add to database and save.
                // Note: a failure to add means that RPC server is already in the
                // database, which is entirely possible.
                //
                int numAdded = 0;
                allTaskResults2.ForEach(result =>
                {
                    result.TaskResult.ForEach(r =>
                    {
                        if (Add(r))
                        {
                            numAdded++;
                        }
                    });
                });

                ProgressBar.Value = 0;
                ProgressBar.Visible = false;
                try
                {
                    Save();
                    Trace.TraceInformation("Refresh: Added " + numAdded+" RPC servers.");
                    ProgressLabel.Text = "Added " + numAdded + " RPC servers.";
                    success = true;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Refresh: An error occurred when saving the database: " +
                        ex.Message);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Refresh: An exception occurred while attempting to refresh " +
                    "the database: " + ex.Message);
            }

            return success;
        }

        private List<string> GetLoadedModulesForProcess(int ProcessId)
        {
            Process process;
            try
            {
                process = Process.GetProcessById(ProcessId);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to lookup process by ID: " + ex.Message);
            }

            var modules = new List<string>();
            if (process.Modules != null || process.Modules.Count > 0)
            {
                process.Modules.Cast<System.Diagnostics.ProcessModule>().ToList().ForEach(module =>
                    modules.Add(module.FileName));
                Trace.WriteLine("Process " + ProcessId + " has " + process.Modules.Count + " modules.");
            }
            return modules;
        }
    }
}
