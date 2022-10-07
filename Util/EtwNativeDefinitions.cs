//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using NtApiDotNet.Win32;
using NtApiDotNet;
using System;
using System.Runtime.InteropServices;
using static RpcInvestigator.Util.NativeTraceConsumer;

namespace RpcInvestigator.Util
{
    public static class NativeTraceControl
    {
        #region Enums
        public enum EventTraceLevel : byte
        {
            Critical = 1,
            Error = 2,
            Warning = 3,
            Information = 4,
            Verbose = 5,
        }

        public enum ControlCode : uint
        {
            Query = 0,
            Stop = 1,
            Update = 2,
            Flush = 3
        }

        public enum EventControlCode : uint
        {
            DisableProvider = 0,
            EnableProvider = 1,
            CaptureState = 2,
        }

        public enum EnableTraceProperties : uint
        {
            Sid = 0x1,
            TerminalServicesId = 0x2,
            StackTrace = 0x4,
            PsmKey = 0x8,
            IgnoreKeyword0 = 0x10,
            ProviderGroup = 0x20,
            EnableKeyword0 = 0x40,
            ProcessStartKey = 0x80,
            EventKey = 0x100,
            ExcludeInPrivate = 0x200,
        }

        [Flags]
        public enum LogFileModeFlags : uint
        {
            None = 0,
            Sequential = 0x00000001,
            Circular = 0x00000002,
            Append = 0x00000004,
            NewFile = 0x00000008,
            Preallocate = 0x00000020,
            NonStoppable = 0x00000040,
            Secure = 0x00000080,
            RealTime = 0x00000100,
            DelayOpen = 0x00000200,
            Buffering = 0x00000400,
            PrivateLogger = 0x00000800,
            AddHeader = 0x00001000,
            UseKBytesForSize = 0x00002000,
            UseGlobalSequence = 0x00004000,
            UseLocalSequence = 0x00008000,
            Relog = 0x00010000,
            PrivateInProc = 0x00020000,
            Reserved = 0x00100000,
            UsePagedMember = 0x01000000,
            NoPerProcessorBuffering = 0x10000000,
            SystemLogger = 0x02000000,
            AddToTriageDump = 0x80000000,
            StopOnHybridShutdown = 0x00400000,
            PersistOnHybridShutdown = 0x00800000,
            IndependentSession = 0x08000000,
            Compressed = 0x04000000,
        }

        [Flags]
        public enum WNodeFlags : uint
        {
            None = 0,
            AllData = 0x00000001,
            SingleInstance = 0x00000002,
            SingleItem = 0x00000004,
            EventItem = 0x00000008,
            FixedInstanceSize = 0x00000010,
            TooSmall = 0x00000020,
            InstancesSame = 0x00000040,
            StaticInstanceNames = 0x00000080,
            Internal = 0x00000100,
            UseTimestamp = 0x00000200,
            PersistEvent = 0x00000400,
            Reference = 0x00002000,
            AnsiInstanceNames = 0x00004000,
            MethodItem = 0x00008000,
            PDOInstanceNames = 0x00010000,
            TracedGuid = 0x00020000,
            LogWNode = 0x00040000,
            UseGuidPtr = 0x00080000,
            UseMofPtr = 0x00100000,
            NoHeader = 0x00200000,
            SendDataBlock = 0x00400000,
            VersionedProperties = 0x00800000,
        }

        [Flags]
        public enum ProcessTraceMode : uint
        {
            RealTime = 0x00000100,
            RawTimestamp = 0x00001000,
            EventRecord = 0x10000000
        }

        public enum WNodeClientContext : uint
        {
            Default = 0,
            QPC = 1,
            SystemTime = 2,
            CpuCycleCounter = 3
        }

        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public class ENABLE_TRACE_PARAMETERS
        {
            public uint Version;
            public EnableTraceProperties EnableProperty;
            public uint ControlFlags;
            public Guid SourceId;
            public IntPtr EnableFilterDesc;
            public uint FilterDescCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WNODE_HEADER
        {
            public uint BufferSize;
            public uint ProviderId;
            public ulong HistoricalContext;
            public LargeIntegerStruct TimeStamp;
            public Guid Guid;
            public WNodeClientContext ClientContext;
            public WNodeFlags Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_TRACE_PROPERTIES
        {
            public WNODE_HEADER Wnode;
            public uint BufferSize;
            public uint MinimumBuffers;
            public uint MaximumBuffers;
            public uint MaximumFileSize;
            public LogFileModeFlags LogFileMode;
            public uint FlushTimer;
            public uint EnableFlags;
            public uint AgeLimit;
            public uint NumberOfBuffers;
            public uint FreeBuffers;
            public uint EventsLost;
            public uint BuffersWritten;
            public uint LogBuffersLost;
            public uint RealTimeBuffersLost;
            public IntPtr LoggerThreadId;
            public uint LogFileNameOffset;
            public uint LoggerNameOffset;
            public uint VersionNumber;
            public uint FilterDescCount;
            public IntPtr FilterDesc;
            public ulong V2Options;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0xac, CharSet = CharSet.Unicode)]
        public struct TIME_ZONE_INFORMATION
        {
            public int bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string standardName;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = 8)]
            public ushort[] standardDate;
            public int standardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string daylightName;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = 8)]
            public ushort[] daylightDate;
            public int daylightBias;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TRACE_LOGFILE_HEADER
        {
            public uint BufferSize;
            public uint Version;
            public uint ProviderVersion;
            public uint NumberOfProcessors;
            public LargeIntegerStruct EndTime;
            public uint TimerResolution;
            public uint MaximumFileSize;
            public uint LogFileMode;
            public uint BuffersWritten;
            public uint StartBuffers;
            public uint PointerSize;
            public uint EventsLost;
            public uint CpuSpeedInMhz;
            public string LoggerName;
            public string LogFileName;
            public TIME_ZONE_INFORMATION TimeZone;
            public LargeIntegerStruct BootTime;
            public LargeIntegerStruct PerfFreq;
            public LargeIntegerStruct StartTime;
            public uint Reserved;
            public uint BuffersLost;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct EVENT_TRACE_LOGFILE
        {
            public string LogFileName;
            public string LoggerName;
            public long CurrentTime;
            public uint BuffersRead;
            public ProcessTraceMode ProcessTraceMode;
            public EVENT_TRACE CurrentEvent;
            public TRACE_LOGFILE_HEADER LogfileHeader;
            public BufferCallback BufferCallback;
            public uint BufferSize;
            public uint Filled;
            public uint EventsLost;
            public EventRecordCallback EventCallback;
            public uint IsKernelTrace;
            public IntPtr Context;
        }
        #endregion

        #region APIs
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Win32Error StartTrace(
            [In, Out] ref long SessionHandle,
            [In] string SessionName,
            [In, Out] IntPtr Properties // EVENT_TRACE_PROPERTIES
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Win32Error ControlTrace(
            [In] long SessionHandle,
            [In] string SessionName,
            [In, Out] IntPtr Properties, // EVENT_TRACE_PROPERTIES
            [In] ControlCode ControlCode
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern long OpenTrace(
            [In, Out] IntPtr LogFile // EVENT_TRACE_LOGFILE*
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Win32Error CloseTrace(
            [In] long SessionHandle
            );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Win32Error ProcessTrace(
            [In] long[] handleArray,
            [In] uint handleCount,
            [In] IntPtr StartTime,
            [In] IntPtr EndTime);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Win32Error EnableTraceEx2(
          [In] long SessionHandle,
          [In] ref Guid ProviderId,
          [In] EventControlCode ControlCode,
          [In] EventTraceLevel Level,
          [In] ulong MatchAnyKeyword,
          [In] ulong MatchAllKeyword,
          [In] uint Timeout,
          [In, Optional] IntPtr EnableParameters // ENABLE_TRACE_PARAMETERS
        );
        #endregion
    }

    public static class NativeTraceConsumer
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void EventRecordCallback(
            [In] IntPtr EventRecord
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate uint BufferCallback(
              [In] IntPtr Logfile // EVENT_TRACE_LOGFILE
            );

        #region Enums

        [Flags]
        public enum EventHeaderFlags : ushort
        {
            ExtendedInfo = 0x01,
            PrivateSession = 0x02,
            StringOnly = 0x04,
            TraceMessage = 0x08,
            NoCpuTime = 0x10,
            Is32BitHeader = 0x20,
            Is64BitHeader = 0x40,
            ClassicHeader = 0x100,
            ProcessorIndex = 0x200
        }

        public enum EventHeaderExtendedDataType : ushort
        {
            RelatedActivityId = 0x0001,
            Sid = 0x0002,
            TerminalServicesId = 0x0003,
            InstanceInfo = 0x0004,
            StackTrace32 = 0x0005,
            StackTrace64 = 0x0006,
            PebsIndex = 0x0007,
            PmcCounters = 0x0008,
            PsmKey = 0x0009,
            EventKey = 0x000A,
            SchemaTl = 0x000B,
            ProvTraits = 0x000C,
            ProcessStartKey = 0x000D,
            Max = 0x000E,
        }

        [Flags]
        public enum EventHeaderPropertyFlags : ushort
        {
            Xml = 1,
            ForwardedXml = 2,
            LegacyEventLog = 3
        }

        [Flags]
        public enum MAP_FLAGS
        {
            ValueMap = 1,
            Bitmap = 2,
            ManifestPatternMap = 4,
            WbemValueMap = 8,
            WbemBitmap = 16,
            WbemFlag = 32,
            WbemNoMap = 64
        };

        [Flags]
        public enum PROPERTY_FLAGS
        {
            None = 0,
            Struct = 0x1,
            ParamLength = 0x2,
            ParamCount = 0x4,
            WbemXmlFragment = 0x8,
            ParamFixedLength = 0x10,
            ParamFixedCount = 0x20
        }

        public enum TdhInputType : ushort
        {
            Null,
            UnicodeString,
            AnsiString,
            Int8,
            UInt8,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
            Float,
            Double,
            Boolean,
            Binary,
            GUID,
            Pointer,
            FILETIME,
            SYSTEMTIME,
            SID,
            HexInt32,
            HexInt64,
            CountedUtf16String = 22,
            CountedMbcsString = 23,
            Struct = 24,
            CountedString = 300,
            CountedAnsiString,
            ReversedCountedString,
            ReversedCountedAnsiString,
            NonNullTerminatedString,
            NonNullTerminatedAnsiString,
            UnicodeChar,
            AnsiChar,
            SizeT,
            HexDump,
            WbemSID
        };

        public enum TdhOutputType : ushort
        {
            Null = 0,  // use TdhInputType
            String = 1,
            DateTime = 2,
            Byte = 3,
            UnsignedByte = 4,
            Short = 5,
            UnsignedShort = 6,
            Integer = 7,
            UnsignedInteger = 8,
            Long = 9,
            UnsignedLong = 10,
            Float = 11,
            Double = 12,
            Boolean = 13,
            Guid = 14,
            HexBinary = 15,
            HexInteger8 = 16,
            HexInteger16 = 17,
            HexInteger32 = 18,
            HexInteger64 = 19,
            Pid = 20,
            Tid = 21,
            Port = 22,
            Ipv4 = 23,
            Ipv6 = 24,
            SocketAddress = 25,
            CimDateTime = 26,
            EtwTime = 27,
            Xml = 28,
            ErrorCode = 29,
            Win32Error = 30,
            Ntstatus = 31,
            Hresult = 32,
            CultureInsensitiveDatetime = 33,
            Json = 34,
            ReducedString = 300,
            NoPrin = 301,
        }

        public enum EVENT_FIELD_TYPE {
            KeywordInformation,
            LevelInformation,
            ChannelInformation,
            TaskInformation,
            OpcodeInformation,
            Max
        }

        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_DESCRIPTOR
        {
            public ushort Id;
            public byte Version;
            public byte Channel;
            public EventTraceLevel Level;
            public byte Opcode;
            public ushort Task;
            public ulong Keyword;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_DATA_DESCRIPTOR
        {
            public long Ptr;
            public int Size;
            public byte Type;
            public byte Reserved1;
            public ushort Reserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_TRACE_HEADER
        {
            public ushort Size;
            public ushort FieldTypeFlags;
            public byte Type;
            public EventTraceLevel Level;
            public ushort Version;
            public uint ThreadId;
            public uint ProcessId;
            public LargeIntegerStruct Timestamp;
            public Guid Guid;
            public ulong ProcessorTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_TRACE
        {
            public EVENT_TRACE_HEADER Header;
            public uint InstanceId;
            public uint ParentInstanceId;
            public Guid ParentGuid;
            public IntPtr MofData;
            public uint MofLength;
            public uint BufferContext;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_HEADER
        {
            public ushort Size;
            public ushort HeaderType;
            public EventHeaderFlags Flags;
            public EventHeaderPropertyFlags EventProperty;
            public uint ThreadId;
            public uint ProcessId;
            public long TimeStamp;
            public Guid ProviderId;
            public ushort Id;
            public byte Version;
            public byte Channel;
            public EventTraceLevel Level;
            public byte Opcode;
            public ushort Task;
            public ulong Keyword;
            public uint KernelTime;
            public uint UserTime;
            public Guid ActivityId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ETW_BUFFER_CONTEXT
        {
            public byte ProcessorNumber;
            public byte Alignment;
            public ushort LoggerId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_HEADER_EXTENDED_DATA_ITEM
        {
            public ushort Reserved1;
            public EventHeaderExtendedDataType ExtType;
            public ushort Reserved2;
            public ushort DataSize;
            public ulong DataPtr;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_RECORD
        {
            public EVENT_HEADER EventHeader;
            public ETW_BUFFER_CONTEXT BufferContext;
            public ushort ExtendedDataCount;
            public ushort UserDataLength;
            public IntPtr ExtendedData; // array of EVENT_HEADER_EXTENDED_DATA_ITEM
            public IntPtr UserData;
            public IntPtr UserContext;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_MAP_ENTRY
        {
            public int NameOffset;
            public int Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_MAP_INFO
        {
            public int NameOffset;
            public MAP_FLAGS Flag;
            public int EntryCount;
            public int ValueType;
            public EVENT_MAP_ENTRY MapEntryArray;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TRACE_EVENT_INFO
        {
            public Guid ProviderGuid;
            public Guid EventGuid;
            public EVENT_DESCRIPTOR EventDescriptor;
            public int DecodingSource;
            public int ProviderNameOffset;
            public int LevelNameOffset;
            public int ChannelNameOffset;
            public int KeywordsNameOffset;
            public int TaskNameOffset;
            public int OpcodeNameOffset;
            public int EventMessageOffset;
            public int ProviderMessageOffset;
            public int BinaryXmlOffset;
            public int BinaryXmlSize;
            public int EventNameOffset;
            public int RelatedActivityIDNameOffset;
            public int PropertyCount;
            public int TopLevelPropertyCount;
            public int Flags;
            public IntPtr EventPropertyInfoArray; // EVENT_PROPERTY_INFO[1]
        }
        public struct EVENT_PROPERTY_INFO
        {
            public PROPERTY_FLAGS Flags;
            public int NameOffset;
            public TdhInputType InType;
            public TdhOutputType OutType;
            public int MapNameOffset;

            public ushort StructStartIndex
            {
                get
                {
                    return (ushort)InType;
                }
            }
            public ushort NumOfStructMembers
            {
                get
                {
                    return (ushort)OutType;
                }
            }
            public ushort CountOrCountIndex;
            public ushort LengthOrLengthIndex;
            public int Reserved;
        }

        public struct PROVIDER_EVENT_INFO
        {
            public uint NumberOfEvents;
            public uint Reserved;
            public EVENT_DESCRIPTOR EventDescriptorsArray; // EVENT_DESCRIPTOR[ANYSIZE_ARRAY]
        }

        #endregion

        #region APIs
        [DllImport("tdh.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Win32Error TdhGetEventInformation(
            [In] IntPtr Event, // EVENT_RECORD*
            [In] uint TdhContextCount,
            [In] IntPtr TdhContext,
            [Out] IntPtr Buffer, // TRACE_EVENT_INFO*
            [In, Out] ref uint BufferSize
        );

        [DllImport("tdh.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Win32Error TdhGetEventMapInformation(
            [In] IntPtr Event, // EVENT_RECORD*
            [In] string MapName,
            [Out] IntPtr Buffer, // EVENT_MAP_INFO*
            [In, Out] ref uint BufferSize
        );

        [DllImport("tdh.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Win32Error TdhFormatProperty(
            [In] IntPtr TraceEventInfo, // TRACE_EVENT_INFO*
            [In, Optional] IntPtr MapInfo,  // EVENT_MAP_INFO*
            [In] uint PointerSize,
            [In] TdhInputType PropertyInType,
            [In] TdhOutputType PropertyOutType,
            [In] ushort PropertyLength,
            [In] ushort UserDataLength,
            [In] IntPtr UserData,           // BYTE*
            [In, Out] ref uint BufferSize,
            [Out, Optional] IntPtr Buffer,  // WCHAR*
            [In, Out] ref ushort UserDataConsumed
        );

        [DllImport("tdh.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Win32Error TdhEnumerateManifestProviderEvents(
            [In] ref Guid ProviderGuid,
            [Out] IntPtr Buffer, // PROVIDER_EVENT_INFO*
            [In, Out] ref uint BufferSize
        );

        [DllImport("tdh.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Win32Error TdhGetManifestEventInformation(
            [In] ref Guid ProviderGuid,
            [In] IntPtr EventDescriptor, // EVENT_DESCRIPTOR*
            [Out] IntPtr Buffer, // TRACE_EVENT_INFO*
            [In, Out] ref uint BufferSize
        );
        #endregion
    }
}
