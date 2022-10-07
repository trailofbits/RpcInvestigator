//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using NtApiDotNet;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RpcInvestigator.Util
{
    using static NativeTraceConsumer;
    using static NativeTraceControl;
    using static TraceLogger;

    public class EtwFileTrace : IDisposable
    {
        private bool m_Disposed;
        private readonly string m_EtlFileName;
        private long m_PerfFreq;

        public EtwFileTrace(string EtlFileName)
        {
            m_EtlFileName = EtlFileName;
            m_Disposed = false;
        }

        ~EtwFileTrace()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            Trace(TraceLoggerType.FileTrace,
                TraceEventType.Information,
                "Disposing FileTrace");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Consume(
            EventRecordCallback EventCallback,
            BufferCallback BufferCallback
            )
        {
            var logfile = new EVENT_TRACE_LOGFILE();
            logfile.LogFileName = m_EtlFileName;
            logfile.EventCallback = EventCallback;
            logfile.BufferCallback = BufferCallback;
            logfile.ProcessTraceMode = ProcessTraceMode.EventRecord;

            var logFilePointer = Marshal.AllocHGlobal(Marshal.SizeOf(logfile));
            Trace(TraceLoggerType.FileTrace,
                TraceEventType.Information,
                "Consuming events from ETL file " + m_EtlFileName);
            Marshal.StructureToPtr(logfile, logFilePointer, false);
            var handle = OpenTrace(logFilePointer);
            //
            // Marshal the structure back so we can get the PerfFreq
            //
            logfile = (EVENT_TRACE_LOGFILE)Marshal.PtrToStructure(
                logFilePointer, typeof(EVENT_TRACE_LOGFILE));
            Marshal.FreeHGlobal(logFilePointer);
            if (handle.ToInt64() == -1)
            {
                var error = "OpenTrace() returned an invalid handle:  0x" +
                    Marshal.GetLastWin32Error().ToString("X");
                Trace(TraceLoggerType.FileTrace,
                    TraceEventType.Error,
                    error);
                throw new Exception(error);
            }

            Trace(TraceLoggerType.FileTrace,
                TraceEventType.Information,
                "Trace session successfully opened, processing trace..");

            try
            {
                //
                // Update PerfFreq so event's timestamps can be parsed.
                //
                m_PerfFreq = logfile.LogfileHeader.PerfFreq.QuadPart;

                //
                // Blocking call.  The caller's BufferCallback must return false to
                // unblock this routine.
                //
                var handles = Marshal.AllocHGlobal(IntPtr.Size * 1);
                Marshal.WriteIntPtr(handles, 0, handle);
                var status = ProcessTrace(
                    handles, 1, IntPtr.Zero, IntPtr.Zero).MapDosErrorToStatus();
                Marshal.FreeHGlobal(handles);
                if (status != NtStatus.STATUS_SUCCESS)
                {
                    var error = "ProcessTrace() failed: 0x" + status.ToString("X") +
                        ", GetLastError: " + Marshal.GetLastWin32Error().ToString("X");
                    Trace(TraceLoggerType.FileTrace,
                        TraceEventType.Error,
                        error);
                    throw new Exception(error);
                }
                Trace(TraceLoggerType.FileTrace,
                    TraceEventType.Information,
                    "Trace processing successfully completed.");
            }
            finally
            {
                CloseTrace(handle);
            }
        }

        public
        long
        GetPerfFreq()
        {
            return m_PerfFreq;
        }
    }
}
