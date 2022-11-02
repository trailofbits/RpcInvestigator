using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using NtApiDotNet.Win32;
using System.Management;
using System.Globalization;

namespace RpcInvestigator
{
    public enum RpcLibraryFilterType
    {
        NoFilter,
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
            Trace.WriteLine("Library: Added RPC server Id " + id +
                " and version " + version);
            return true;
        }

        public bool Remove(string InterfaceId)
        {
            var id = InterfaceId.ToUpper();
            if (!m_Entries.ContainsKey(id))
            {
                Trace.TraceWarning("Server " + id + " does not exist in the data set.");
                return false;
            }
            if (m_Entries.Remove(id))
            {
                Trace.WriteLine("Library: Removed RPC server ID " + id);
                return true;
            }
            Trace.WriteLine("Library: Failed to remove RPC server ID " + id);
            return false;
        }

        public bool Remove(string InterfaceId, string InterfaceVersion)
        {
            var id = InterfaceId.ToUpper();
            if (!m_Entries.ContainsKey(id))
            {
                Trace.TraceWarning("Server " + id + " does not exist in the data set.");
                return false;
            }
            foreach (var server in m_Entries[id])
            {
                if (server.InterfaceVersion == InterfaceVersion)
                {
                    if (m_Entries[id].Remove(server))
                    {
                        Trace.WriteLine("Library: Removed RPC server ID " + id + ", "+
                            "version "+InterfaceVersion);
                        return true;
                    }
                    Trace.WriteLine("Library: Failed to remove RPC server ID " + id + ", " +
                            "version " + InterfaceVersion);
                    return false;
                }
            }

            Trace.WriteLine("Library: Unable to locate RPC server ID " + id + ", " +
                            "version " + InterfaceVersion);
            return false;
        }

        public RpcServerEntry Get(string InterfaceId, string InterfaceVersion)
        {
            var id = InterfaceId.ToUpper();
            if (!m_Entries.ContainsKey(id))
            {
                return null;
            }
            return m_Entries[id].FirstOrDefault(
                entry => entry.InterfaceVersion == InterfaceVersion);
        }

        public List<RpcServerEntry> Get(string InterfaceId)
        {
            var id = InterfaceId.ToUpper();
            if (!m_Entries.ContainsKey(id))
            {
                return null;
            }
            return m_Entries[id];
        }

        public void Clear()
        {
            m_Entries.Clear();
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

        public bool Remove(string InterfaceId)
        {
            return m_Data.Remove(InterfaceId);
        }

        public bool Remove(string InterfaceId, string Version)
        {
            return m_Data.Remove(InterfaceId, Version);
        }


        public RpcServer Get(string InterfaceId, string InterfaceVersion)
        {
            var item = m_Data.Get(InterfaceId, InterfaceVersion);
            if (item == null)
            {
                return null;
            }
            return RpcServer.Deserialize(item.SerializedServer);
        }

        public List<RpcServer> Get(string InterfaceId)
        {
            var items = m_Data.Get(InterfaceId);
            if (items == null)
            {
                return null;
            }
            return items.Select(
                entry => RpcServer.Deserialize(entry.SerializedServer)).ToList();
        }

        public List<RpcServer> Get()
        {
            var all = m_Data.Get();
            var servers = new List<RpcServer>();
            foreach (var kvp in all)
            {
                foreach (var serverEntry in kvp.Value)
                {
                    var server = RpcServer.Deserialize(serverEntry.SerializedServer);
                    servers.Add(server);
                }
            }
            return servers;
        }

        public Dictionary<string, List<RpcServer>> GetServersWithMultipleVersions()
        {
            var all = m_Data.Get().Where(s => s.Value.Count > 0);
            var servers = new Dictionary<string, List<RpcServer>>();
            foreach (var kvp in all)
            {
                servers.Add(kvp.Key, new List<RpcServer>());
                foreach (var serverEntry in kvp.Value)
                {
                    servers[kvp.Key].Add(
                        RpcServer.Deserialize(serverEntry.SerializedServer));
                }
            }
            return servers;
        }

        public void Clear()
        {
            m_Data.Clear();
        }

        public List<RpcServer> Find(RpcLibraryFilter Filter)
        {
            var matches = new List<RpcServer>();
            var interfaceAndVersion = Filter.InterfaceIdAndVersion;
            var keyword = Filter.Keyword;

            switch (Filter.FilterType)
            {
                case RpcLibraryFilterType.FilterByKeyword:
                    {
                        var all = Get();
                        foreach (var server in all)
                        {
                            if ((!string.IsNullOrEmpty(server.ServiceDisplayName) &&
                                 server.ServiceDisplayName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(server.ServiceName) &&
                                 server.ServiceName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (!string.IsNullOrEmpty(server.FilePath) &&
                                 server.FilePath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (server.InterfaceId.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                matches.Add(server);
                                continue;
                            }

                            foreach (var endpoint in server.Endpoints)
                            {
                                if ((!string.IsNullOrEmpty(endpoint.EndpointPath) &&
                                    endpoint.EndpointPath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(endpoint.BindingString) &&
                                    endpoint.BindingString.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (endpoint.InterfaceId.ToString().IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                                {
                                    matches.Add(server);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                default:
                    {
                        matches.AddRange(Get());
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
            Action<ArrayList> ProgressInitializeCallback,
            Action<string> ProgressReportCallback,
            Action<string> ProgressSetStatusLabelCallback
            )
        {
            int workSize = 16;
            bool success = false;

            var alpcServers = RpcAlpcServer.GetAlpcServers().ToList();

            if (alpcServers.Count() == 0)
            {
                Trace.TraceWarning("Refresh: No RPC ALPC servers available.");
                return true;
            }

            //
            // Get all loaded modules for all processes, as reported by WMI.
            //
            var modules = new List<string>();
            ProgressSetStatusLabelCallback?.Invoke("Scanning for loaded modules...");
            await Task.Run(() =>
            {
                modules = GetLoadedModules();
            });

            Trace.TraceInformation("Refresh: Discovered " + modules.Count() +
                    " unique modules.");

            //
            // Get RPC servers from unique modules.
            //
            int max = modules.Count() / workSize;
            ProgressInitializeCallback?.Invoke(new ArrayList() {
                workSize, max, "Processing " + max + " unique modules, please wait..." });
            var allTaskResults2 = await TaskWorker.RunSync(
                modules,
                workSize,
                (moduleList) =>
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

                    ProgressReportCallback?.Invoke("Processing module " +
                        "<current> of <total>");
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

            try
            {
                Save();
                Trace.TraceInformation("Refresh: Added " + numAdded + " RPC servers.");
                ProgressSetStatusLabelCallback?.Invoke("Added " + numAdded + " RPC servers.");
                success = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Refresh: An error occurred when saving the database: " +
                    ex.Message);
                ProgressSetStatusLabelCallback?.Invoke("An exception occurred, please see the logs.");
            }

            return success;
        }

        private List<string> GetLoadedModules()
        {
            var modules = new List<string>();
            try
            {
                using (var searcher =
                       new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM CIM_ProcessExecutable"))
                {
                    foreach (ManagementObject queryObject in searcher.Get())
                    {
                        //
                        // Believe it or not, creating a new WMI ManagementObject so the
                        // Name can be cleanly pulled out of the object results in about
                        // a 20x perf hit. So we'll parse it manually.
                        //
                        var antecedent = (string)queryObject["Antecedent"];
                        int start = antecedent.IndexOf(@"CIM_DataFile.Name=""") + 19;
                        int length = antecedent.Length - start;
                        var name = antecedent.Substring(start, length).Replace("\"", "").Replace(@"\\", @"\");
                        if (!modules.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                        {
                            modules.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("WMI exception: " + ex.Message);
            }
            return modules;
        }
    }
}
