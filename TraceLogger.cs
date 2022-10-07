//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System.Diagnostics;
using System.IO;

namespace RpcInvestigator
{
    public static class TraceLogger
    {
        private static readonly string m_TraceFileDir = Path.Combine(
            new string[] { Settings.m_WorkingDir, "Logs\\" });
        private static string m_Location = Path.Combine(new string[] { m_TraceFileDir,
                            Path.GetRandomFileName() + ".txt" });
        private static TextWriterTraceListener m_TraceListener =
            new TextWriterTraceListener(m_Location, "RpcInvestigatorListener");
        private static SourceSwitch m_Switch =
            new SourceSwitch("RpcInvestigatorSwitch", "Verbose");
        private static TraceSource[] Sources = {
            new TraceSource("Settings", SourceLevels.Verbose),
            new TraceSource("RpcLibrary", SourceLevels.Verbose),
            new TraceSource("TaskWorker", SourceLevels.Verbose),
            new TraceSource("ServicesWindow", SourceLevels.Verbose),
            new TraceSource("RpcLibraryServerList", SourceLevels.Verbose),
            new TraceSource("RpcLibraryProcedureList", SourceLevels.Verbose),
            new TraceSource("RpcProcedureList", SourceLevels.Verbose),
            new TraceSource("RpcEndpointList", SourceLevels.Verbose),
            new TraceSource("RpcServerList", SourceLevels.Verbose),
            new TraceSource("RpcAlpcServerList", SourceLevels.Verbose),
            new TraceSource("RpcSniffer", SourceLevels.Verbose),
            new TraceSource("RealTimeTrace", SourceLevels.Verbose),
            new TraceSource("FileTrace", SourceLevels.Verbose),
            new TraceSource("EtwEventParser", SourceLevels.Verbose),
            new TraceSource("EtwProviderParser", SourceLevels.Verbose),
        };

        public enum TraceLoggerType
        {
            Settings,
            RpcLibrary,
            TaskWorker,
            ServicesWindow,
            RpcLibraryServerList,
            RpcLibraryProcedureList,
            RpcProcedureList,
            RpcEndpointList,
            RpcServerList,
            RpcAlpcServerList,
            RpcSniffer,
            RealTimeTrace,
            FileTrace,
            EtwEventParser,
            EtwProviderParser,
            Max
        }

        public static void Initialize()
        {
            foreach (var source in Sources)
            {
                source.Listeners.Add(m_TraceListener);
                source.Switch = m_Switch;
            }
        }

        public static void SetLevel(SourceLevels Level)
        {
            m_Switch.Level = Level;
        }

        public static void Trace(TraceLoggerType Type, TraceEventType EventType, string Message)
        {
            if (Type >= TraceLoggerType.Max)
            {
                throw new System.Exception("Invalid logger type");
            }

            //
            // Event ID is not used.
            //
            Sources[(int)Type].TraceEvent(EventType, 1, Message);
        }
    }
}
