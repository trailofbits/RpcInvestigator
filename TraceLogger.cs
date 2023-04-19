﻿//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using static RpcInvestigator.TraceLogger;

namespace RpcInvestigator
{
    public static class TraceLogger
    {
        private static readonly string m_TraceFileDir = Path.Combine(
            new string[] { Settings.m_WorkingDir, "Logs\\" });
        private static string m_Location = Path.Combine(new string[] { m_TraceFileDir,
                            System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") +
                            ".txt" });
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
            new TraceSource("SddlParser", SourceLevels.Verbose),
            new TraceSource("ML", SourceLevels.Verbose),
            new TraceSource("PythonWrapper", SourceLevels.Verbose),
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
            SddlParser,
            ML,
            PythonWrapper,
            Max
        }

        public static void Initialize()
        {
            System.Diagnostics.Trace.AutoFlush = true;
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

    public class MLLogger : ILogger
    {
        public MLLogger()
        {
        }

        public IDisposable BeginScope<TState>(TState state) { return default; }

        public bool IsEnabled(LogLevel logLevel) { return true; }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            TraceEventType level = TraceEventType.Verbose;
            switch (logLevel)
            {
                case LogLevel.Critical:
                    level = TraceEventType.Critical;
                    break;
                case LogLevel.Error:
                    level = TraceEventType.Error;
                    break;
                case LogLevel.Warning:
                    level = TraceEventType.Warning;
                    break;
                case LogLevel.Information:
                    level = TraceEventType.Information;
                    break;
                case LogLevel.Debug:
                    level = TraceEventType.Verbose;
                    break;
            }
            Trace(TraceLoggerType.ML,
                  level,
                  formatter(state, exception));
        }
    }
}
