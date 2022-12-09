//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
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
using NtApiDotNet;

namespace RpcInvestigator
{
    using static TraceLogger;

    public enum RpcLibraryFilterType
    {
        NoFilter,
        FilterByKeyword
    }

    public class RpcLibraryFilter
    {
        public RpcLibraryFilterType FilterType;
        public string Keyword;

        public RpcLibraryFilter()
        {
            FilterType = RpcLibraryFilterType.NoFilter;
        }
    }

    [Serializable]
    public class RpcServerEntry
    {
        public RpcServerEntry()
        {
        }
        public Version InterfaceVersion;
        public int PointerSize;
        public byte[] SerializedServer;
    }

    [Serializable]
    public class RpcServerData
    {
        //
        // Note: this class is NOT thread-safe.
        //

        public int m_Version;
        private Dictionary<Guid, List<RpcServerEntry>> m_Entries;

        public RpcServerData()
        {
            m_Version = 0;
            m_Entries = new Dictionary<Guid, List<RpcServerEntry>>();
        }

        public bool Add(RpcServer Server)
        {
            if (Exists(Server.InterfaceId, Server.InterfaceVersion))
            {
                return false;
            }

            if (!m_Entries.ContainsKey(Server.InterfaceId))
            {
                m_Entries.Add(Server.InterfaceId, new List<RpcServerEntry>());
            }

            m_Entries[Server.InterfaceId].Add(new RpcServerEntry()
            {
                InterfaceVersion = Server.InterfaceVersion,
                SerializedServer = Server.Serialize(),
                PointerSize = IntPtr.Size
            });
            return true;
        }

        public bool Remove(Guid InterfaceId)
        {
            if (!Exists(InterfaceId))
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Warning,
                    "Server " + InterfaceId.ToString() +
                    " does not exist in the data set.");
                return false;
            }

            var num = m_Entries[InterfaceId].Count(
                e => e.PointerSize == IntPtr.Size);
            var removed = m_Entries[InterfaceId].RemoveAll(
                e => e.PointerSize == IntPtr.Size);

            if (num != removed)
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Error,
                    "Library: Only removed " + removed + " out of " + num +
                    " for interface " + InterfaceId.ToString());
                return false;
            }

            Trace(TraceLoggerType.RpcLibrary,
                TraceEventType.Verbose,
                "Library: Successfully removed " + removed + " out of " + num +
                " for interface " + InterfaceId.ToString());
            return true;
        }

        public bool Remove(Guid InterfaceId, Version InterfaceVersion)
        {
            if (!Exists(InterfaceId, InterfaceVersion))
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Warning,
                    "Server " + InterfaceId.ToString() +
                    " does not exist in the data set.");
                return false;
            }

            var num = m_Entries[InterfaceId].Count(
                e => e.PointerSize == IntPtr.Size &&
                e.InterfaceVersion.Equals(InterfaceVersion));
            var removed = m_Entries[InterfaceId].RemoveAll(
                e => e.PointerSize == IntPtr.Size &&
                e.InterfaceVersion.Equals(InterfaceVersion));

            if (num != removed)
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Error,
                    "Library: Only removed " + removed + " out of " + num +
                    " for interface " + InterfaceId.ToString() + ", version " +
                    InterfaceVersion.ToString());
                return false;
            }

            Trace(TraceLoggerType.RpcLibrary,
                TraceEventType.Verbose,
                "Library: Successfully removed " + removed + " out of " + num +
                " for interface " + InterfaceId.ToString() + ", version " +
                InterfaceVersion.ToString());
            return true;
        }

        public RpcServerEntry Get(Guid InterfaceId, Version InterfaceVersion)
        {
            if (!Exists(InterfaceId, InterfaceVersion))
            {
                return null;
            }
            return m_Entries[InterfaceId].FirstOrDefault(
                entry => entry.InterfaceVersion.Equals(InterfaceVersion) &&
                    entry.PointerSize == IntPtr.Size);
        }

        public List<RpcServerEntry> Get(Guid InterfaceId)
        {
            if (!Exists(InterfaceId))
            {
                return null;
            }
            return m_Entries[InterfaceId].Where(
                entry => entry.PointerSize == IntPtr.Size).ToList();
        }

        public Dictionary<Guid, List<RpcServer>> Get()
        {
            var all = new Dictionary<Guid, List<RpcServer>>();
            foreach (var kvp in m_Entries)
            {
                foreach (var serverEntry in kvp.Value)
                {
                    if (serverEntry.PointerSize != IntPtr.Size)
                    {
                        continue;
                    }
                    if (!all.ContainsKey(kvp.Key))
                    {
                        all.Add(kvp.Key, new List<RpcServer>());
                    }
                    all[kvp.Key].Add(
                        RpcServer.Deserialize(serverEntry.SerializedServer));
                }
            }
            return all;
        }

        public bool Exists(Guid InterfaceId, Version InterfaceVersion)
        {
            if (!Exists(InterfaceId))
            {
                return false;
            }
            return m_Entries[InterfaceId].Any(
                entry => entry.InterfaceVersion.Equals(InterfaceVersion) &&
                    entry.PointerSize == IntPtr.Size);
        }

        public bool Exists(Guid InterfaceId)
        {
            return m_Entries.ContainsKey(InterfaceId);
        }

        public void Clear()
        {
            m_Entries.Clear();
        }

        public int GetCount()
        {
            return m_Entries.Values.Sum(v => v.Count(s => s.PointerSize == IntPtr.Size));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var all = Get();
            sb.Append("Database for " + (IntPtr.Size == 4 ? "32-bit" : "64-bit") +
                " RPC servers.");
            foreach (var kvp in all)
            {
                sb.AppendLine("Server " + kvp.Key + " has " + kvp.Value.Count + " versions:");
                kvp.Value.ForEach(entry =>
                {
                    sb.AppendLine("   Server " + entry.InterfaceId.ToString() +
                        " / " + entry.InterfaceVersion.ToString());
                    sb.AppendLine("      FilePath        : " + entry.FilePath);
                    sb.AppendLine("      Procedure Count : " + entry.ProcedureCount);
                    sb.AppendLine("      Service Name: " + entry.ServiceName);
                    sb.AppendLine("      Endpoints:");
                    foreach (var endpoint in entry.Endpoints.ToList())
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
                    Trace(TraceLoggerType.RpcLibrary,
                        TraceEventType.Verbose,
                        "Library: successfully created '" + m_Path + "'");
                }
                catch (Exception ex) // swallow
                {
                    Trace(TraceLoggerType.RpcLibrary,
                        TraceEventType.Error,
                        "Failed to create database '" +
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
                        Trace(TraceLoggerType.RpcLibrary,
                            TraceEventType.Verbose,
                            "Library: successfully loaded '" + m_Path + "'");
                        Trace(TraceLoggerType.RpcLibrary,
                            TraceEventType.Verbose,
                            "Library: " + GetServerCount() + " entries.");
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
                using (var stream = File.Open(
                    m_Path,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, m_Data);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save '" + m_Path + "': " + ex.Message);
            }
            Trace(TraceLoggerType.RpcLibrary,
                TraceEventType.Verbose,
                "Library: successfully saved '" + m_Path + "'");
        }

        public bool Add(RpcServer Server)
        {
            return m_Data.Add(Server);
        }

        public bool Remove(Guid InterfaceId)
        {
            return m_Data.Remove(InterfaceId);
        }

        public bool Remove(Guid InterfaceId, Version Version)
        {
            return m_Data.Remove(InterfaceId, Version);
        }

        public RpcServer Get(Guid InterfaceId, Version InterfaceVersion)
        {
            var item = m_Data.Get(InterfaceId, InterfaceVersion);
            if (item == null)
            {
                return null;
            }
            return RpcServer.Deserialize(item.SerializedServer);
        }

        public List<RpcServer> Get(Guid InterfaceId)
        {
            var items = m_Data.Get(InterfaceId);
            if (items == null || items.Count() == 0)
            {
                return null;
            }
            return items.Select(
                entry => RpcServer.Deserialize(entry.SerializedServer)).ToList();
        }

        public List<RpcServer> GetAllServers()
        {
            //
            // Note: Expensive!
            //
            return m_Data.Get().Values.SelectMany(v => v).ToList();
        }

        public Dictionary<Guid, List<RpcServer>> GetServersWithMultipleVersions()
        {
            //
            // Note: expensive!
            //
            return m_Data.Get().Where(
                s => s.Value.Count > 0).ToDictionary(i => i.Key, i => i.Value);
        }

        public void Clear()
        {
            m_Data.Clear();
        }

        public bool Exists(Guid InterfaceId)
        {
            return m_Data.Exists(InterfaceId);
        }

        public bool Exists(Guid InterfaceId, Version InterfaceVersion)
        {
            return m_Data.Exists(InterfaceId, InterfaceVersion);
        }

        public List<RpcServer> Find(RpcLibraryFilter Filter)
        {
            var matches = new List<RpcServer>();
            var keyword = Filter.Keyword;

            switch (Filter.FilterType)
            {
                case RpcLibraryFilterType.FilterByKeyword:
                    {
                        var all = GetAllServers(); // expensive: deserializes all servers!
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
                        var all = GetAllServers(); // expensive: deserializes all servers!
                        matches.AddRange(all);
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
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Information,
                    "Merge: No RPC servers were provided.");
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
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Information,
                    "Merge: Added " + numAdded + " RPC servers.");
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Error,
                    "An error occurred when saving the database: " +
                    ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> Refresh(
            Settings Settings,
            Action<ArrayList> ProgressInitializeCallback,
            Action<string> ProgressReportCallback,
            Action<string> ProgressSetStatusLabelCallback,
            int MaxServers = 0
            )
        {
            bool success = false;

            var alpcServers = RpcAlpcServer.GetAlpcServers().ToList();

            if (alpcServers.Count() == 0)
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Warning,
                    "Refresh: No RPC ALPC servers available.");
                return true;
            }

            //
            // Build a list of process IDs that contain at least one RPC server
            // that's not currently in our library. Because the same DLL/RPC server
            // can be loaded in multiple processes, and we only need to parse it
            // once, we'll pick a representative process.
            //
            // In practice, this optimization doesn't help much, as there always
            // seems to be some prolific DLLs that can't be parsed for whatever
            // reason, meaning we'll always have to re-parse them.
            //
            var distinctInterfaceIds = new Dictionary<string, int>();
            await Task.Run(() =>
            {
                foreach (var s in alpcServers)
                {
                    if (s.EndpointCount == 0 || s.Endpoints == null)
                    {
                        continue;
                    }
                    s.Endpoints.Where(e =>
                        !Exists(e.InterfaceId, e.InterfaceVersion)).ToList().ForEach(e =>
                        {
                            var id = e.InterfaceId.ToString();
                            if (!distinctInterfaceIds.ContainsKey(id))
                            {
                                distinctInterfaceIds.Add(id, s.ProcessId);
                            }
                        });
                }
            });

            var pids = distinctInterfaceIds.Values.ToList();
            if (pids.Count() == 0)
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Information,
                    "Refresh: No new ALPC servers.");
                return true;
            }
            pids = pids.Distinct().ToList();

            //
            // Get all loaded modules for all processes, as reported by WMI.
            //
            var modules = new List<string>();
            ProgressSetStatusLabelCallback?.Invoke("Scanning for loaded modules...");
            await Task.Run(() =>
            {
                modules = GetLoadedModules(pids);
            });

            Trace(TraceLoggerType.RpcLibrary,
                TraceEventType.Information,
                "Refresh: Discovered " + modules.Count() +
                " unique modules.");

            //
            // Get RPC servers from unique modules.
            //
            // IMPORTANT:  ParsePeFile (NtApiDotNet) uses DbgHelp.dll's SymFromAddr
            // function which states on MSDN:
            //   "All DbgHelp functions, such as this one, are single threaded.
            //   Therefore, calls from more than one thread to this function will
            //   likely result in unexpected behavior or memory corruption.
            //   To avoid this, you must synchronize all concurrent calls from
            //   more than one thread to this function.
            // Do NOT use TPL or threading here!
            //
            int numAdded = 0;
            int max = modules.Count();
            ProgressInitializeCallback?.Invoke(new ArrayList() {
                1, max, "Processing " + max + " unique modules, please wait..." });
            var rpcServers = new List<RpcServer>();
            await Task.Run(() =>
            {
                foreach (var module in modules)
                {
                    ProgressReportCallback?.Invoke("Processing module " +
                        "<current> of <total>");
                    var servers = new List<RpcServer>();
                    try
                    {
                        servers = RpcServer.ParsePeFile(
                            module, Settings.m_DbghelpPath, Settings.m_SymbolPath, false).ToList();
                        if (servers.Count() > 0)
                        {
                            rpcServers.AddRange(servers);
                        }
                    }
                    catch (Exception ex) // swallow
                    {
                        Trace(TraceLoggerType.RpcLibrary,
                            TraceEventType.Error,
                            "Refresh: Unable to parse PE file '" +
                            module + "': " + ex.Message);
                    }
                    //
                    // Add to the library
                    //
                    foreach (var s in servers)
                    {
                        //
                        // Because we had to parse all modules in a process that might
                        // only contain one unknown RPC server, we'll end up parsing
                        // RPC servers we already know about. Don't try to re-add them.
                        //
                        if (!distinctInterfaceIds.ContainsKey(s.InterfaceId.ToString()))
                        {
                            continue;
                        }

                        if (Add(s))
                        {
                            numAdded++;
                        }
                        else
                        {
                            //
                            // This shouldn't happen, since we already culled the list
                            // to only servers that don't exist in the library, and we
                            // intentionally skipped servers that were unnecessarily
                            // parsed above.
                            //
                            Trace(TraceLoggerType.RpcLibrary,
                                TraceEventType.Error,
                                "Refresh: failed to add server " +
                                s.InterfaceId.ToString() + " to the library.");
                        }
                    }

                    if (MaxServers > 0 && numAdded >= MaxServers)
                    {
                        //
                        // This path is useful for UnitTests that don't need every
                        // single RPC server on the system.
                        //
                        return;
                    }

                    //
                    // Sometimes symbol resolution fails. And when it does, it
                    // fails silently (see NtApiDotNet's NdrParser.cs). I don't
                    // know why. Maybe it's some sort of rate limiter logic on
                    // the public MS symbol server side.
                    //
                    Task.Delay(50);
                }
            });

            try
            {
                Save();
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Information,
                    "Refresh: Added " + numAdded + " RPC servers.");
                ProgressSetStatusLabelCallback?.Invoke("Added " + numAdded + " RPC servers.");
                success = true;
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcLibrary,
                    TraceEventType.Error,
                    "Refresh: An error occurred when saving the database: " +
                    ex.Message);
                ProgressSetStatusLabelCallback?.Invoke("An exception occurred, please see the logs.");
            }

            return success;
        }

        private List<string> GetLoadedModules(List<int> ProcessIds)
        {
            var modules = new List<string>();
            foreach (var pid in ProcessIds)
            {
                try
                {
                    using (var proc = NtProcess.Open(pid,
                        ProcessAccessRights.QueryLimitedInformation
                        | ProcessAccessRights.DupHandle,
                        false))
                    {
                        if (!proc.IsSuccess)
                            continue;
                        if (proc.Result.Frozen)
                            continue;
                        var p = Process.GetProcessById(pid);
                        var moduleList = p.Modules.Cast<
                            System.Diagnostics.ProcessModule>().ToList().Select(
                                m => m.FileName).ToList();
                        modules = modules.Union(moduleList).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Trace(TraceLoggerType.RpcLibrary,
                        TraceEventType.Error,
                        "GetLoadedModules exception: " + ex.Message);
                }
            }
            return modules;
        }
    }
}
