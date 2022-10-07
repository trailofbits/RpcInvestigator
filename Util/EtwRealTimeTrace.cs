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
using System.Text;

namespace RpcInvestigator.Util
{
    using static NativeTraceConsumer;
    using static NativeTraceControl;
    using static TraceLogger;

    public class EtwRealTimeTrace : IDisposable
    {
        private readonly string m_SessionName;
        private readonly Guid m_SessionGuid;
        private bool m_Disposed;
        private IntPtr m_PropertiesBuffer;
        private Guid m_ProviderGuid;
        private IntPtr m_SessionHandle;
        private EventTraceLevel m_TraceLevel;
        private long m_PerfFreq;

        public EtwRealTimeTrace(
            string SessionName,
            Guid Provider,
            bool EnableDebugEvents)
        {
            m_SessionName = SessionName;
            m_SessionGuid = Guid.NewGuid();
            m_Disposed = false;
            m_PropertiesBuffer = IntPtr.Zero;
            m_ProviderGuid = Provider;
            m_SessionHandle = IntPtr.Zero;
            m_TraceLevel = EventTraceLevel.Information;
            if (EnableDebugEvents)
            {
                //
                // We don't need this to be more customizable for
                // how simple RPC provider events are.
                //
                m_TraceLevel = EventTraceLevel.Verbose;
            }
        }

        ~EtwRealTimeTrace()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            Trace(TraceLoggerType.RealTimeTrace,
                TraceEventType.Information,
                "Disposing RealTimeTrace");

            m_Disposed = true;

            if (m_SessionHandle != IntPtr.Zero)
            {
                var result = EnableTraceEx2(m_SessionHandle,
                    ref m_ProviderGuid,
                    EventControlCode.DisableProvider,
                    m_TraceLevel,
                    0, 0, 0,
                    IntPtr.Zero).MapDosErrorToStatus();
                if (result != NtStatus.STATUS_SUCCESS)
                {
                    Trace(TraceLoggerType.RealTimeTrace,
                        TraceEventType.Error,
                        "RealTimeTrace dispose could not disable provider: " +
                        result.ToString("X"));
                }
                result = ControlTrace(
                    m_SessionHandle,
                    m_SessionName,
                    m_PropertiesBuffer,
                    ControlCode.Stop).MapDosErrorToStatus();
                if (result != NtStatus.STATUS_SUCCESS)
                {
                    Trace(TraceLoggerType.RealTimeTrace,
                        TraceEventType.Error,
                        "RealTimeTrace dispose could not stop trace: " +
                        result.ToString("X"));
                }
            }

            if (m_PropertiesBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_PropertiesBuffer);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            NtStatus ntStatus;

            Trace(TraceLoggerType.RealTimeTrace,
                TraceEventType.Information,
                "Starting RealTimeTrace " + m_SessionName + "...");

            m_PropertiesBuffer = GenerateSessionProperties(); // freed in dtor
            for (; ; )
            {
                var status = StartTrace(
                    ref m_SessionHandle, m_SessionName, m_PropertiesBuffer);
                if (status == NtApiDotNet.Win32.Win32Error.ERROR_ALREADY_EXISTS)
                {
                    Trace(TraceLoggerType.RealTimeTrace,
                        TraceEventType.Warning,
                        "A trace is already opened with instance " +
                        "name " + m_SessionName + ", attempting to stop it.");
                    //
                    // Orphaned session, possibly from a crash. Try to stop it.
                    //
                    ntStatus = ControlTrace(
                        IntPtr.Zero,
                        m_SessionName,
                        m_PropertiesBuffer,
                        ControlCode.Stop).MapDosErrorToStatus();
                    if (ntStatus != NtStatus.STATUS_SUCCESS)
                    {
                        var error = "Unable to stop orphaned trace session: " +
                            ntStatus.ToString("X");
                        Trace(TraceLoggerType.RealTimeTrace,
                            TraceEventType.Error,
                            error);
                        throw new Exception(error);
                    }
                    Trace(TraceLoggerType.RealTimeTrace,
                        TraceEventType.Information,
                        "Prior trace session stopped.");
                    continue;
                }
                else if (status != NtApiDotNet.Win32.Win32Error.SUCCESS ||
                         m_SessionHandle.ToInt64() == 0)
                {
                    m_SessionHandle = IntPtr.Zero;
                    var error = "StartTrace() failed: 0x" + status.ToString("X");
                    Trace(TraceLoggerType.RealTimeTrace,
                        TraceEventType.Error,
                        error);
                    throw new Exception(error);
                }
                break;
            }

            Trace(TraceLoggerType.RealTimeTrace,
                TraceEventType.Information,
                "Trace started. Enabling provider...");

            var parameters = new ENABLE_TRACE_PARAMETERS
            {
                Version = 2,
                EnableProperty = EnableTraceProperties.ProcessStartKey |
                    EnableTraceProperties.Sid,
            };

            var parametersPtr = Marshal.AllocHGlobal(Marshal.SizeOf(parameters));
            Marshal.StructureToPtr(parameters, parametersPtr, false);
            ntStatus = EnableTraceEx2(
                m_SessionHandle,
                ref m_ProviderGuid,
                EventControlCode.EnableProvider,
                m_TraceLevel,
                0,
                0,
                0xffffffff,
                parametersPtr).MapDosErrorToStatus();
            Marshal.FreeHGlobal(parametersPtr);
            if (ntStatus != NtStatus.STATUS_SUCCESS)
            {
                var error = "EnableTraceEx2() failed: 0x" + ntStatus.ToString("X");
                Trace(TraceLoggerType.RealTimeTrace,
                    TraceEventType.Error,
                    error);
                throw new Exception(error);
            }

            Trace(TraceLoggerType.RealTimeTrace,
                TraceEventType.Information,
                "Provider enabled successfully.");
        }

        public void Consume(
            EventRecordCallback EventCallback,
            BufferCallback BufferCallback
            )
        {
            var logfile = new EVENT_TRACE_LOGFILE();
            logfile.EventCallback = EventCallback;
            logfile.BufferCallback = BufferCallback;
            logfile.LoggerName = m_SessionName;
            logfile.ProcessTraceMode =
                ProcessTraceMode.EventRecord |
                ProcessTraceMode.RealTime;

            var logFilePointer = Marshal.AllocHGlobal(Marshal.SizeOf(logfile));
            Trace(TraceLoggerType.RealTimeTrace,
                TraceEventType.Information,
                "Consuming events...");
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
                Trace(TraceLoggerType.RealTimeTrace,
                    TraceEventType.Error,
                    error);
                throw new Exception(error);
            }

            Trace(TraceLoggerType.RealTimeTrace,
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
                    Trace(TraceLoggerType.RealTimeTrace,
                        TraceEventType.Error,
                        error);
                    throw new Exception(error);
                }
                Trace(TraceLoggerType.RealTimeTrace,
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

        private
        IntPtr
        GenerateSessionProperties()
        {
            var loggerName = Encoding.Unicode.GetBytes(m_SessionName + "\0");
            var loggerNameLocation = Marshal.SizeOf(
                typeof(EVENT_TRACE_PROPERTIES));
            int total = loggerNameLocation + loggerName.Length;
            var buffer = Marshal.AllocHGlobal(total);
            var properties = new EVENT_TRACE_PROPERTIES();
            properties.Wnode.BufferSize = (uint)total;
            properties.Wnode.Flags =
                WNodeFlags.TracedGuid |
                WNodeFlags.VersionedProperties;
            properties.Wnode.ClientContext = WNodeClientContext.QPC;
            properties.Wnode.Guid = m_SessionGuid;
            properties.VersionNumber = 2;
            properties.BufferSize = 64; // high freq should use 64kb - 128kb (this field in KB!)
            properties.LogFileMode =
                LogFileModeFlags.RealTime |
                LogFileModeFlags.Sequential;
            properties.MinimumBuffers = 4;
            properties.MaximumBuffers = 4;
            properties.LogFileNameOffset = 0;
            properties.LoggerNameOffset =
                (uint)Marshal.SizeOf(typeof(EVENT_TRACE_PROPERTIES));
            Marshal.StructureToPtr(properties, buffer, false);
            IntPtr dest = IntPtr.Add(buffer, loggerNameLocation);
            Marshal.Copy(loggerName, 0, dest, loggerName.Length);
            return buffer;
        }
    }
}
