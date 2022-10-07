//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using BrightIdeasSoftware;
using NtApiDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace RpcInvestigator.Util
{
    using static NativeTraceConsumer;
    using static TraceLogger;

    public class ParsedEtwEvent
    {
        //
        // This struct needs to be kept tidy. Some potentially useful fields have
        // been purposefully left out, as ETW data can be a firehose and memory
        // consumed quickly. Instances of these class are kept in memory as long
        // as the window containing the listview displays them.
        //
        // Further, this field list is reduced to only those observed as meaningful
        // for the Microsoft-Windows-RPC provider. Any other provider will probably
        // want other fields added, like EventId, Version, Provider, etc. To save
        // space, they are removed here instead of keeping them without the [OLVColumn]
        // attribute.
        //
        [OLVColumn]
        public uint ProcessId { get; set; }
        [OLVColumn]
        public long ProcessStartKey { get; set; }
        [OLVColumn]
        public uint ThreadId { get; set; }
        [OLVColumn]
        public Sid UserSid { get; set; }
        [OLVColumn]
        public Guid ActivityId { get; set; }
        [OLVColumn]
        public DateTime Timestamp { get; set; }
        [OLVColumn]
        public string Level { get; set; }
        [OLVColumn]
        public string Channel { get; set; }
        [OLVColumn]
        public List<string> Keywords { get; set; }
        [OLVColumn]
        public string Task { get; set; }
        [OLVColumn]
        public string Opcode { get; set; }
        public Dictionary<string, string> UserDataProperties { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("Event ID: " + EventId);
            //sb.AppendLine("Version: " + Version);
            sb.AppendLine("Timestamp: " + Timestamp.ToString());
            sb.AppendLine("Process: " + ProcessId + " (Key=" + ProcessStartKey + ")");
            sb.AppendLine("TID: " + ThreadId);
            if (UserSid != null)
            {
                sb.AppendLine("User SID: " + UserSid.ToString());
            }
            if (ActivityId != null)
            {
                sb.AppendLine("Activity ID: " + ActivityId.ToString());
            }
            sb.AppendLine("Level: " + Level);
            sb.AppendLine("Channel: " + Channel);
            sb.AppendLine("Keywords: ");
            foreach (var k in Keywords)
            {
                sb.AppendLine("   " + k);
            }
            sb.AppendLine("Task: " + Task);
            sb.AppendLine("Opcode: " + Opcode);
            sb.AppendLine("UserData Properties:");
            if (UserDataProperties != null)
            {
                foreach (var kvp in UserDataProperties)
                {
                    sb.AppendLine("   " + kvp.Key + " : " + kvp.Value);
                }
            }
            return sb.ToString();
        }
    }

    public class EtwEventParserBuffers : IDisposable
    {
        public EVENT_RECORD m_Event;
        public TRACE_EVENT_INFO m_TraceEventInfo;
        public EVENT_MAP_INFO m_MapInfo;
        public IntPtr m_TraceEventInfoBuffer;
        public IntPtr m_TdhMapBuffer;
        public IntPtr m_TdhOutputBuffer;
        public IntPtr m_EventBuffer;
        public static int ETW_MAX_EVENT_SIZE = 65536; // 65K
        public static int MAP_SIZE = 1024 * 4000; // 4MB
        public static int TDH_STR_SIZE = 1024 * 4000; // 4MB
        public static int TRACE_EVENT_INFO_SIZE = 1024 * 4000; // 4MB
        private bool m_Disposed;

        public EtwEventParserBuffers()
        {
            //
            // Pre-allocate large buffers to re-use across all events that
            // reuse this parser. This helps performance tremendously.
            //
            // The map buffer holds manifest data that can be ulong.max size.
            // We pick a decently large size here (4MB)
            //
            // The TDH output buffer contains arbitrary unicode string data,
            // and can be as large as the ETW provider wants AFAIK.
            //
            // Event buffer can never exceed 65k.
            //
            // The TRACE_EVENT_INFO structure is variable-length and the total
            // size depends on the ETW provider's manifest.
            //
            m_TdhMapBuffer = Marshal.AllocHGlobal(MAP_SIZE);
            m_TdhOutputBuffer = Marshal.AllocHGlobal(TDH_STR_SIZE);
            m_EventBuffer = Marshal.AllocHGlobal(ETW_MAX_EVENT_SIZE);
            m_TraceEventInfoBuffer = Marshal.AllocHGlobal(TRACE_EVENT_INFO_SIZE);
        }

        ~EtwEventParserBuffers()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Information,
                "Disposing EtwEventParserBuffers");

            m_Disposed = true;

            if (m_TdhMapBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_TdhMapBuffer);
            }

            if (m_TdhOutputBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_TdhOutputBuffer);
            }

            if (m_EventBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_EventBuffer);
            }

            if (m_TraceEventInfoBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_TraceEventInfoBuffer);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetEvent(EVENT_RECORD Event)
        {
            m_Event = Event;
            Marshal.StructureToPtr(Event, m_EventBuffer, false);
        }

        public void SetTraceInfo()
        {
            m_TraceEventInfo = (TRACE_EVENT_INFO)Marshal.PtrToStructure(
                m_TraceEventInfoBuffer, typeof(TRACE_EVENT_INFO));
        }

        public void SetMapInfoBuffer()
        {
            m_MapInfo = (EVENT_MAP_INFO)Marshal.PtrToStructure(
                m_TdhMapBuffer, typeof(EVENT_MAP_INFO));
        }
    }

    public class EtwEventParser
    {
        private EtwEventParserBuffers m_Buffers;
        private long m_UniqueId;
        private ParsedEtwEvent m_ParsedEvent;
        private long m_PerfFreq;

        public EtwEventParser(
            EVENT_RECORD Event,
            EtwEventParserBuffers Buffers,
            long PerfFreq)
        {
            m_Buffers = Buffers;
            m_Buffers.SetEvent(Event);
            m_UniqueId = Event.EventHeader.TimeStamp;
            m_PerfFreq = PerfFreq;
        }

        public
        void
        DumpDiagnosticInfo(string DestinationFolder)
        {
            var size = m_Buffers.m_Event.EventHeader.Size;
            var rawEvent = new byte[size];
            Marshal.Copy(m_Buffers.m_EventBuffer, rawEvent, 0, size);
            string target = Path.Combine(DestinationFolder, "Event-raw-" +
                m_UniqueId.ToString("X") + ".bin");
            File.WriteAllBytes(target, rawEvent);
            target = Path.Combine(DestinationFolder, "Event-text-" +
                m_UniqueId.ToString("X") + ".txt");
            File.WriteAllText(target, m_ParsedEvent.ToString());
        }

        public
        ParsedEtwEvent
        Parse()
        {
            Trace(TraceLoggerType.EtwEventParser,
                 TraceEventType.Verbose,
                 "Parsing event 0x" + m_UniqueId.ToString("X"));

            //
            // Ignore string-only events and those generated by WPP.
            //
            if (m_Buffers.m_Event.EventHeader.Flags.HasFlag(
                    EventHeaderFlags.StringOnly) ||
                m_Buffers.m_Event.EventHeader.Flags.HasFlag(
                    EventHeaderFlags.TraceMessage))
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Warning,
                    "Ignoring string-only or WPP-generated event");
                return null;
            }
            //
            // Ignore events that require legacy WMI MOF manifest to parse user data.
            //
            if (m_Buffers.m_Event.EventHeader.EventProperty.HasFlag(
                EventHeaderPropertyFlags.LegacyEventLog))
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Warning,
                    "Ignoring legacy event log style event");
                return null;
            }

            m_ParsedEvent = new ParsedEtwEvent();

            //
            // Get stuff hanging right off the EVENT_RECORD's header
            //
            m_ParsedEvent.ProcessId = m_Buffers.m_Event.EventHeader.ProcessId;
            m_ParsedEvent.ThreadId = m_Buffers.m_Event.EventHeader.ThreadId;
            m_ParsedEvent.ActivityId = m_Buffers.m_Event.EventHeader.ActivityId;
            //
            // Event timestamp is stored in QPC format, convert to a scaled
            // 100-ns standard system time that DateTime can cope with.
            //
            var scaledTimestamp = (long)
                (m_Buffers.m_Event.EventHeader.TimeStamp * 10000000.0 / m_PerfFreq);
            m_ParsedEvent.Timestamp = DateTime.FromFileTime(scaledTimestamp);

            //parsedEvent.ProviderGuid = m_Buffers.m_Event.EventHeader.ProviderId;

            //
            // The remaining parser steps require TRACE_EVENT_INFO. We cache the
            // resulting buffer internally for the remainder of processing this
            // event.
            //
            if (!GetTraceEventInfo())
            {
                return null;
            }

            //
            // Use TDH to parse meta information from the event descriptor
            // inside the event header (keywords, opcode, task, etc)
            //
            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Information,
                "Parsing trace event meta information");
            if (!ParseMetaInformation())
            {
                return null;
            }

            //
            // Parse "extended data" which is stuff added by the OS to every
            // ETW event - like SID, stack trace, process start key,
            // and so on (some of these must be turned on when you call
            // EnableTraceEx)
            //
            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Information,
                "Parsing trace extended data");
            if (!ParseExtendedData())
            {
                return null;
            }

            Trace(TraceLoggerType.EtwEventParser,
                 TraceEventType.Information,
                 m_ParsedEvent.ToString());

            //
            // Use TDH and the manifest to parse custom "user data" which
            // has information unique to this provider.
            //
            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Information,
                "Parsing trace user data");
            if (m_Buffers.m_Event.UserDataLength <= 0)
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Warning,
                    "Event contained no user data.");
                return m_ParsedEvent;
            }
            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Information,
                "Event has " + m_Buffers.m_Event.UserDataLength +
                " bytes of user data.");

            //
            // Instantiate a PropertyParser to reuse the pre-allocated buffers.
            //
            var propertyParser = new PropertyParser(m_Buffers);
            if (!propertyParser.Initialize())
            {
                return null;
            }

            //
            // Parsing begins at the top level properties. The PropertyParser.Parse
            // method will recurse as needed.
            //
            var properties = new Dictionary<string, string>();
            var success = propertyParser.Parse(
                0,
                m_Buffers.m_TraceEventInfo.TopLevelPropertyCount,
                ref properties,
                new StringBuilder());
            m_ParsedEvent.UserDataProperties = properties;
            if (!success)
            {
                return null;
            }
            return m_ParsedEvent;
        }

        private
        bool
        GetTraceEventInfo()
        {
            uint bufferSize = (uint)EtwEventParserBuffers.TRACE_EVENT_INFO_SIZE;
            var status = TdhGetEventInformation(
                m_Buffers.m_EventBuffer,
                0,
                IntPtr.Zero,
                m_Buffers.m_TraceEventInfoBuffer,
                ref bufferSize);
            if (status != NtApiDotNet.Win32.Win32Error.SUCCESS)
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Error,
                    "TdhGetEventInformation failed: " +
                    status.ToString("X"));
                return false;
            }
            m_Buffers.SetTraceInfo();
            return true;
        }

        private
        bool
        ParseMetaInformation()
        {
            var buffer = m_Buffers.m_TraceEventInfoBuffer;

            try
            {
                /*if (m_Buffers.m_TraceEventInfo.ProviderNameOffset > 0)
                {
                    ParsedEvent.Provider = Marshal.PtrToStringUni(
                        IntPtr.Add(buffer, m_Buffers.m_TraceEventInfo.ProviderNameOffset));
                }*/

                if (m_Buffers.m_TraceEventInfo.LevelNameOffset > 0)
                {
                    m_ParsedEvent.Level = Marshal.PtrToStringUni(
                        IntPtr.Add(buffer, m_Buffers.m_TraceEventInfo.LevelNameOffset));
                }
                else
                {
                    m_ParsedEvent.Level = m_Buffers.m_Event.EventHeader.Level.ToString();
                }

                if (m_Buffers.m_TraceEventInfo.ChannelNameOffset > 0)
                {
                    m_ParsedEvent.Channel = Marshal.PtrToStringUni(
                        IntPtr.Add(buffer, m_Buffers.m_TraceEventInfo.ChannelNameOffset));
                }
                else
                {
                    m_ParsedEvent.Channel =
                        m_Buffers.m_Event.EventHeader.Channel.ToString();
                }

                m_ParsedEvent.Keywords = new List<string>();
                if (m_Buffers.m_TraceEventInfo.KeywordsNameOffset > 0)
                {
                    for (int offset = m_Buffers.m_TraceEventInfo.KeywordsNameOffset; ;)
                    {
                        var str = Marshal.PtrToStringUni(IntPtr.Add(buffer, offset));
                        if (string.IsNullOrEmpty(str))
                        {
                            break;
                        }
                        m_ParsedEvent.Keywords.Add(str);
                        offset += Encoding.Unicode.GetByteCount(str) + 2;
                    }
                }

                if (m_Buffers.m_TraceEventInfo.TaskNameOffset > 0)
                {
                    m_ParsedEvent.Task = Marshal.PtrToStringUni(
                        IntPtr.Add(buffer, m_Buffers.m_TraceEventInfo.TaskNameOffset));
                }
                else
                {
                    m_ParsedEvent.Task = m_Buffers.m_Event.EventHeader.Task.ToString();
                }

                if (m_Buffers.m_TraceEventInfo.OpcodeNameOffset > 0)
                {
                    m_ParsedEvent.Opcode = Marshal.PtrToStringUni(
                        IntPtr.Add(buffer, m_Buffers.m_TraceEventInfo.OpcodeNameOffset));
                }
                else
                {
                    m_ParsedEvent.Opcode =
                        m_Buffers.m_Event.EventHeader.Opcode.ToString();
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Error,
                    "An exception occurred when marshaling the " +
                    "TRACE_EVENT_INFO struct: " + ex.Message);
                return false;
            }
            return true;
        }

        private
        bool
        ParseExtendedData()
        {
            if (!m_Buffers.m_Event.EventHeader.Flags.HasFlag(
                    EventHeaderFlags.ExtendedInfo) ||
                m_Buffers.m_Event.ExtendedDataCount <= 0)
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Warning,
                    "Event contained no extended data.");
                return true;
            }

            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Information,
                "Event has " + m_Buffers.m_Event.ExtendedDataCount +
                " extended data items.");

            IntPtr buffer = m_Buffers.m_Event.ExtendedData;
            for (int i = 0; i < m_Buffers.m_Event.ExtendedDataCount; i++)
            {
                var item = (EVENT_HEADER_EXTENDED_DATA_ITEM)Marshal.PtrToStructure(
                    buffer, typeof(EVENT_HEADER_EXTENDED_DATA_ITEM));
                if (item.DataSize == 0)
                {
                    Trace(TraceLoggerType.EtwEventParser,
                        TraceEventType.Error,
                        "Corrupt extended data, zero-length " +
                        "extended data item.");
                    return false;
                }
                unsafe
                {
                    IntPtr data = new IntPtr((void*)item.DataPtr);
                    try
                    {
                        switch (item.ExtType)
                        {
                            case EventHeaderExtendedDataType.Sid:
                                {
                                    m_ParsedEvent.UserSid = new Sid(data);
                                    break;
                                }
                            case EventHeaderExtendedDataType.RelatedActivityId:
                                {
                                    //
                                    // This is always 0 for RPC events but could
                                    // be useful in the future.
                                    //
                                    //ParsedEvent.ParentActivityId =
                                    //    (Guid)Marshal.PtrToStructure(data, typeof(Guid));
                                    break;
                                }
                            case EventHeaderExtendedDataType.ProcessStartKey:
                                {
                                    m_ParsedEvent.ProcessStartKey = data.ToInt64();
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace(TraceLoggerType.EtwEventParser,
                            TraceEventType.Error,
                            "Failed to cast extended data item #" + i +
                            ", type " + item.ExtType + ", length " + item.DataSize +
                            ", pointer 0x" + item.DataPtr + ": " + ex.Message);
                        return false;
                    }
                    var size = Marshal.SizeOf(typeof(EVENT_HEADER_EXTENDED_DATA_ITEM));
                    buffer = IntPtr.Add(buffer, size);
                }
            }
            return true;
        }
    }

    public class PropertyParser
    {
        private EtwEventParserBuffers m_Buffers;
        private Dictionary<int, ushort> m_PropertyIndexLookup;
        private List<EVENT_PROPERTY_INFO> m_PropertyInfo;
        private IntPtr m_UserDataCurrentPosition;
        private IntPtr m_UserDataEndPosition;
        private ushort m_UserDataRemaining;

        public PropertyParser(EtwEventParserBuffers Buffers)
        {
            m_Buffers = Buffers;
            m_PropertyIndexLookup = new Dictionary<int, ushort>();
            m_UserDataCurrentPosition = m_Buffers.m_Event.UserData;
            m_UserDataEndPosition =
                IntPtr.Add(m_UserDataCurrentPosition, m_Buffers.m_Event.UserDataLength);
            m_UserDataRemaining = m_Buffers.m_Event.UserDataLength;
        }

        public
        bool
        Initialize()
        {
            //
            // Parse the property array from the TRACE_EVENT_INFO for this event.
            //
            try
            {
                int offset = Marshal.OffsetOf(typeof(TRACE_EVENT_INFO),
                    "EventPropertyInfoArray").ToInt32();
                IntPtr arrayStart = IntPtr.Add(m_Buffers.m_TraceEventInfoBuffer, offset);
                m_PropertyInfo = MarshalHelper.MarshalArray<EVENT_PROPERTY_INFO>(arrayStart,
                            (uint)m_Buffers.m_TraceEventInfo.PropertyCount);
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Error,
                    "Failed to retrieve property array: " + ex.Message);
                return false;
            }
            return true;
        }

        public
        bool
        Parse(
            int PropertyIndexStart,
            int PropertyIndexEnd,
            ref Dictionary<string, string> Properties,
            StringBuilder ParentStruct)
        {
            for (int i = PropertyIndexStart; i < PropertyIndexEnd; i++)
            {
                var propertyInfo = m_PropertyInfo[i];
                string propertyName = "(Unnamed)";
                if (propertyInfo.NameOffset != 0)
                {
                    propertyName = Marshal.PtrToStringUni(
                        IntPtr.Add(m_Buffers.m_TraceEventInfoBuffer,
                            propertyInfo.NameOffset));
                }

                bool isArray = false;
                var arrayCount = GetArrayLength(i);

                if (arrayCount > 1 || propertyInfo.Flags.HasFlag(
                    PROPERTY_FLAGS.ParamCount | PROPERTY_FLAGS.ParamFixedCount))
                {
                    isArray = true;
                }

                //
                // If this property is a scalar integer, remember the value
                // in case it is needed for a subsequent property's length
                // or count.
                //
                if (!propertyInfo.Flags.HasFlag(
                        PROPERTY_FLAGS.Struct | PROPERTY_FLAGS.ParamCount) &&
                    propertyInfo.CountOrCountIndex == 1)
                {
                    StorePropertyLookup(i);
                }

                Trace(TraceLoggerType.EtwEventParser,
                    TraceEventType.Verbose,
                    "Parsing property #" + i + " [" + propertyName + "]" +
                    (isArray ? " - Array with " + arrayCount + " elements" : ""));

                //
                // For simplicity, non-array properties are treated like 1-length
                // arrays.
                //
                for (int j = 0; j < arrayCount; j++)
                {
                    var usePropertyName = propertyName;

                    if (isArray)
                    {
                        usePropertyName += "-" + j;
                        Trace(TraceLoggerType.EtwEventParser,
                            TraceEventType.Verbose,
                            usePropertyName);
                    }

                    StringBuilder structValue = new StringBuilder();

                    if (propertyInfo.Flags.HasFlag(PROPERTY_FLAGS.Struct))
                    {
                        Trace(TraceLoggerType.EtwEventParser,
                            TraceEventType.Verbose,
                            "  Property is a struct (start index = " +
                            propertyInfo.StructStartIndex + ", count = " +
                            propertyInfo.NumOfStructMembers + "), recursing...");

                        //
                        // Recurse structs.
                        //
                        structValue.AppendLine(usePropertyName);
                        int startIndex = propertyInfo.StructStartIndex;
                        int endIndex = startIndex + propertyInfo.NumOfStructMembers;
                        if (!Parse(startIndex, endIndex, ref Properties, structValue))
                        {
                            return false;
                        }
                    }

                    if (structValue.Length > 0)
                    {
                        //
                        // Returning from struct recursion? Use that accumulated
                        // value for this property.
                        //
                        AddProperty(usePropertyName,
                            structValue.ToString(),
                            new StringBuilder(),
                            ref Properties);
                    }
                    else if (!GetPropertyValue(i, j, ref Properties, ParentStruct))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private
        bool
        GetPropertyValue(
            int PropertyIndex,
            int ArrayIndex,
            ref Dictionary<string, string> Properties,
            StringBuilder ParentStruct
            )
        {
            var propertyInfo = m_PropertyInfo[PropertyIndex];
            var useMap = false;
            var propertyName = "(Unnamed)";
            var propertyLength = GetPropertyLength(PropertyIndex);
            var traceEventInfoBuffer = m_Buffers.m_TraceEventInfoBuffer;
            var eventBuffer = m_Buffers.m_EventBuffer;

            if (propertyInfo.NameOffset != 0)
            {
                propertyName = Marshal.PtrToStringUni(
                    IntPtr.Add(traceEventInfoBuffer, propertyInfo.NameOffset));
            }

            Trace(TraceLoggerType.EtwEventParser,
                TraceEventType.Verbose,
                "     Property named " + propertyName + " (length=" +
                propertyLength + ")");

            //
            // If the property has an associated map (i.e. an enumerated type),
            // try to look up the map data. (If this is an array, we only need
            // to do the lookup on the first iteration.)
            //
            if (propertyInfo.MapNameOffset != 0 && ArrayIndex == 0)
            {
                switch (propertyInfo.InType)
                {
                    case TdhInputType.UInt8:
                    case TdhInputType.UInt16:
                    case TdhInputType.UInt32:
                    case TdhInputType.HexInt32:
                        {
                            var mapName = Marshal.PtrToStringUni(
                                    IntPtr.Add(traceEventInfoBuffer, propertyInfo.MapNameOffset));
                            uint sizeNeeded = (uint)EtwEventParserBuffers.MAP_SIZE;
                            var status = TdhGetEventMapInformation(
                                eventBuffer,
                                mapName,
                                m_Buffers.m_TdhMapBuffer,
                                ref sizeNeeded).MapDosErrorToStatus();
                            if (status != NtStatus.STATUS_SUCCESS)
                            {
                                //
                                // We could retry, but I want to avoid frequent memory
                                // allocations. If this happens, investigate increasing
                                // the static buffer size.
                                //
                                Trace(TraceLoggerType.EtwEventParser,
                                    TraceEventType.Error,
                                    "TdhGetEventMapInformation() failed: 0x" +
                                    status.ToString("X"));
                                break;
                            }

                            useMap = true;
                            m_Buffers.SetMapInfoBuffer();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            //
            // Loop because we may need to retry the call to TdhFormatProperty.
            //
            for ( ; ; )
            {
                if (IsEmptyProperty(propertyLength, PropertyIndex))
                {
                    Trace(TraceLoggerType.EtwEventParser,
                        TraceEventType.Verbose,
                        "     Property has an empty value.");
                    AddProperty(propertyName,
                        "<empty>",
                        ParentStruct,
                        ref Properties);
                    return true;
                }

                uint outputBufferSize = 65535; // max is 64kb.
                ushort dataConsumed = 0;
                var result = TdhFormatProperty(
                    traceEventInfoBuffer,
                    useMap ? m_Buffers.m_TdhMapBuffer : IntPtr.Zero,
                    (uint)(m_Buffers.m_Event.EventHeader.Flags.HasFlag(
                        EventHeaderFlags.Is32BitHeader) ? 4 : 8),
                    propertyInfo.InType,
                    propertyInfo.OutType == TdhOutputType.NoPrin ?
                        TdhOutputType.Null : propertyInfo.OutType,
                    propertyLength,
                    m_UserDataRemaining,
                    m_UserDataCurrentPosition,
                    ref outputBufferSize,
                    m_Buffers.m_TdhOutputBuffer,
                    ref dataConsumed);
                var status = result.MapDosErrorToStatus();
                if (status == NtStatus.STATUS_BUFFER_TOO_SMALL)
                {
                    //
                    // We could retry, but I want to avoid frequent memory
                    // allocations. However, this should never happen because
                    // we allocate the max ushort size the prototype allows.
                    //
                    Trace(TraceLoggerType.EtwEventParser,
                        TraceEventType.Error,
                        "TdhFormatProperty() failed, buffer too small: 0x" +
                        status.ToString("X"));
                    return false;
                }
                else if (result == NtApiDotNet.Win32.Win32Error.ERROR_EVT_INVALID_EVENT_DATA && useMap)
                {
                    //
                    // If the value isn't in the map, TdhFormatProperty treats it
                    // as an error instead of just putting the number in. We'll
                    // try again with no map.
                    //
                    useMap = false;
                    continue;
                }
                else if (!status.IsSuccess())
                {
                    var error = "TdhFormatProperty() failed: 0x" + status.ToString("X");
                    Trace(TraceLoggerType.EtwEventParser,
                        TraceEventType.Error,
                        error);
                    return false;
                }
                else
                {
                    var propertyValue = Marshal.PtrToStringUni(
                        m_Buffers.m_TdhOutputBuffer);
                    AddProperty(propertyName,
                        propertyValue,
                        ParentStruct,
                        ref Properties);
                    AdvanceBufferPosition(dataConsumed);
                    Trace(TraceLoggerType.EtwEventParser,
                        TraceEventType.Verbose,
                        "     Property value is "+propertyValue);
                    return true;
                }
            }
        }

        private
        bool
        IsEmptyProperty(int PropertyLength, int PropertyIndex)
        {
            var propertyInfo = m_PropertyInfo[PropertyIndex];

            //
            // Null data or null-terminated strings are not supported by TdhFormatProperty
            //
            return PropertyLength == 0 &&
                (propertyInfo.Flags.HasFlag(PROPERTY_FLAGS.ParamLength |
                    PROPERTY_FLAGS.ParamFixedLength)) &&
                (propertyInfo.InType == TdhInputType.UnicodeString ||
                 propertyInfo.InType == TdhInputType.AnsiString);
        }

        private
        void
        StorePropertyLookup(int PropertyIndex)
        {
            var propertyInfo = m_PropertyInfo[PropertyIndex];

            if (m_PropertyIndexLookup.ContainsKey(PropertyIndex))
            {
                //
                // Properties that are repeated in arrays will be revisited for each
                // array element. The index lookup is updated each time.
                //
                m_PropertyIndexLookup.Remove(PropertyIndex);
            }
            //
            // Note: The integer values read here from the Marshaler are
            // read from the current position in the buffer and the buffer
            // should NOT be advanced.
            //
            switch (propertyInfo.InType)
            {
                case TdhInputType.Int8:
                case TdhInputType.UInt8:
                    {
                        if (CanSeek(1))
                        {
                            var value = Marshal.ReadByte(m_UserDataCurrentPosition);
                            m_PropertyIndexLookup.Add(
                                PropertyIndex,
                                value);
                        }
                        else
                        {
                            Trace(TraceLoggerType.EtwEventParser,
                                 TraceEventType.Error,
                                 "  Unable to read 1 byte for PropertyIndex " +
                                 PropertyIndex);
                        }
                        break;
                    }
                case TdhInputType.Int16:
                case TdhInputType.UInt16:
                    {
                        if (CanSeek(2))
                        {
                            var value = (ushort)Marshal.ReadInt16(
                                m_UserDataCurrentPosition);
                            m_PropertyIndexLookup.Add(PropertyIndex, value);
                        }
                        else
                        {
                            Trace(TraceLoggerType.EtwEventParser,
                                 TraceEventType.Error,
                                 "  Unable to read 2 bytes for PropertyIndex " +
                                 PropertyIndex);
                        }
                        break;
                    }
                case TdhInputType.Int32:
                case TdhInputType.UInt32:
                case TdhInputType.HexInt32:
                    {
                        if (CanSeek(4))
                        {
                            var value = Marshal.ReadInt32(m_UserDataCurrentPosition);
                            var asUshort = (ushort)(value > 0xffff ? 0xff : value);
                            m_PropertyIndexLookup.Add(PropertyIndex, asUshort);
                        }
                        else
                        {
                            Trace(TraceLoggerType.EtwEventParser,
                                 TraceEventType.Error,
                                 "  Unable to read 4 bytes for PropertyIndex " +
                                 PropertyIndex);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private
        ushort
        GetPropertyLength(int PropertyIndex)
        {
            var propertyInfo = m_PropertyInfo[PropertyIndex];

            if (propertyInfo.OutType == TdhOutputType.Ipv6 &&
                propertyInfo.InType == TdhInputType.Binary &&
                propertyInfo.LengthOrLengthIndex == 0 &&
                !propertyInfo.Flags.HasFlag(
                    PROPERTY_FLAGS.ParamLength | PROPERTY_FLAGS.ParamFixedLength))
            {
                //
                // special case for incorrectly-defined IPV6 addresses
                //
                return 16;
            }

            if (propertyInfo.Flags.HasFlag(PROPERTY_FLAGS.ParamLength))
            {
                //
                // The length of the property was previously parsed, so
                // look up that value now.
                //
                return m_PropertyIndexLookup[propertyInfo.LengthOrLengthIndex];
            }

            //
            // The length of the property is directly in the field.
            //
            return propertyInfo.LengthOrLengthIndex;
        }

        private
        ushort
        GetArrayLength(int PropertyIndex)
        {
            var propertyInfo = m_PropertyInfo[PropertyIndex];

            if (propertyInfo.Flags.HasFlag(PROPERTY_FLAGS.ParamCount))
            {
                //
                // The length of the array was previously parsed, so
                // look up that value now.
                //
                return m_PropertyIndexLookup[propertyInfo.CountOrCountIndex];
            }

            //
            // The length of the array is directly in the field.
            //
            return propertyInfo.CountOrCountIndex;
        }

        private
        bool
        CanSeek(int NumBytes)
        {
            return m_UserDataEndPosition.ToInt64() -
                m_UserDataCurrentPosition.ToInt64() >= NumBytes;
        }

        private
        void
        AdvanceBufferPosition(ushort NumBytes)
        {
            m_UserDataCurrentPosition = IntPtr.Add(
                m_UserDataCurrentPosition, NumBytes);
            m_UserDataRemaining -= NumBytes;
        }

        private
        void
        AddProperty(
            string PropertyName,
            string PropertyValue,
            StringBuilder ParentStruct,
            ref Dictionary<string, string> Properties)
        {
            if (ParentStruct.Length > 0)
            {
                //
                // We're not directly inserting this property value into the UserData
                // properties because it's actually a field of a struct property we're
                // recursing.  Append it to that value.
                //
                ParentStruct.AppendLine("." + PropertyName + " = " + PropertyValue);
            }
            else
            {
                if (Properties.ContainsKey(PropertyName))
                {
                    //
                    // This is unexpected. ETW event manifests must specify unique
                    // property names IIUC. If this turns out to be untrue, we can
                    // fudge a unique name here. For now, we'll simply drop this
                    // property entirely.
                    //
                    Trace(TraceLoggerType.EtwEventParser,
                        TraceEventType.Error,
                        "The property " + PropertyName + " already exists in the list "+
                        "of UserData properties parsed from this event. Ignoring this "+
                        "discovered property value.");
                    return;
                }
                Properties.Add(PropertyName, PropertyValue);
            }
        }
    }
}
