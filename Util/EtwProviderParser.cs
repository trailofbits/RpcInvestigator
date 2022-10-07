//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using NtApiDotNet.Win32;

namespace RpcInvestigator.Util
{
    using static NativeTraceConsumer;
    using static TraceLogger;

    public static class EtwProviderParser
    {
        public
        static
        Dictionary<string, List<string>>
        GetDistinctProviderEventInfo(Guid ProviderGuid)
        {
            uint bufferSize = 1024;
            var buffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                var status = TdhEnumerateManifestProviderEvents(
                    ref ProviderGuid,
                    buffer,
                    ref bufferSize);
                if (status != Win32Error.SUCCESS)
                {
                    var error = "TdhEnumerateManifestProviderEvents failed: " +
                        status.ToString("X");
                    Trace(TraceLoggerType.EtwProviderParser,
                        TraceEventType.Error,
                        error);
                    throw new Exception(error);
                }
                return ParseProviderEventArray(ProviderGuid, buffer);
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.EtwProviderParser,
                    TraceEventType.Error,
                    "Exception in ParseProviderEventArray(): " +
                    ex.Message);
                throw;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private
        static
        Dictionary<string, List<string>>
        ParseProviderEventArray(Guid ProviderGuid, IntPtr Buffer)
        {
            var array = (PROVIDER_EVENT_INFO)Marshal.PtrToStructure(
                    Buffer, typeof(PROVIDER_EVENT_INFO));
            int offset = Marshal.OffsetOf(typeof(PROVIDER_EVENT_INFO),
                    "EventDescriptorsArray").ToInt32();
            IntPtr arrayStart = IntPtr.Add(Buffer, offset);
            var events = MarshalHelper.MarshalArray<EVENT_DESCRIPTOR>(arrayStart,
                        array.NumberOfEvents);
            var debugStr = new StringBuilder();
            var results = new Dictionary<string, List<string>>()
            {
                //
                // Default system-provided fields. Custom/userData fields
                // added dynamically during parsing.
                //
                {"Task", new List<string>() },
                {"Opcode", new List<string>() },
                {"Level", new List<string>() },
                {"Channel", new List<string>() },
                {"Keywords", new List<string>() },
                {"UserDataProperties", new List<string>()},
            };

            foreach (var evt in events)
            {
                //
                // Important: we exclude events at the Debug level, as these
                // add potentially hundreds of columns to the listview and
                // are not useful anyway.
                //
                if (evt.Level > EventTraceLevel.Information)
                {
                    continue;
                }
                uint descriptorBufferSize = 512;
                uint traceEventInfoBufferSize = 1024 * 4000;
                var eventDescriptorBuffer =
                    Marshal.AllocHGlobal((int)descriptorBufferSize);
                var traceEventInfoBuffer =
                    Marshal.AllocHGlobal((int)traceEventInfoBufferSize);
                try
                {
                    Marshal.StructureToPtr(evt, eventDescriptorBuffer, false);
                    var status = TdhGetManifestEventInformation(
                        ref ProviderGuid,
                        eventDescriptorBuffer,
                        traceEventInfoBuffer,
                        ref traceEventInfoBufferSize);
                    if (status != Win32Error.SUCCESS)
                    {
                        Trace(TraceLoggerType.EtwProviderParser,
                            TraceEventType.Error,
                            "TdhGetManifestEventInformation failed: " +
                            status.ToString("X"));
                        return null;
                    }
                    ParseProviderManifestEvent(
                        traceEventInfoBuffer,
                        evt,
                        ref results,
                        ref debugStr);
                }
                catch (Exception ex)
                {
                    Trace(TraceLoggerType.EtwProviderParser,
                        TraceEventType.Error,
                        "Exception in ParseProviderEventArray(): " +
                        ex.Message);
                    throw;
                }
                finally
                {
                    Marshal.FreeHGlobal(eventDescriptorBuffer);
                    Marshal.FreeHGlobal(traceEventInfoBuffer);
                    //System.IO.File.WriteAllText(
                    //  "EtwProviderDump.txt", debugStr.ToString());
                }
            }

            return results;
        }

        private
        static
        void
        ParseProviderManifestEvent(
            IntPtr TraceEventInfoBuffer,
            EVENT_DESCRIPTOR Descriptor,
            ref Dictionary<string, List<string>> Results,
            ref StringBuilder DebugStr
            )
        {
            var traceEventInfo = (TRACE_EVENT_INFO)Marshal.PtrToStructure(
                TraceEventInfoBuffer, typeof(TRACE_EVENT_INFO));
            string str;

            DebugStr.AppendLine("Event ID "+traceEventInfo.EventDescriptor.Id);

            try
            {
                if (traceEventInfo.LevelNameOffset > 0)
                {
                    str = Marshal.PtrToStringUni(IntPtr.Add(TraceEventInfoBuffer,
                        traceEventInfo.LevelNameOffset));
                    if (!Results["Level"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Level"].Add(str);
                    }
                    DebugStr.AppendLine("   Level: " + str);
                }
                else
                {
                    str = Descriptor.Level.ToString();
                    if (!Results["Level"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Level"].Add(str);
                    }
                    DebugStr.AppendLine("   Level: " + str);
                }

                if (traceEventInfo.ChannelNameOffset > 0)
                {
                    str = Marshal.PtrToStringUni(IntPtr.Add(TraceEventInfoBuffer,
                        traceEventInfo.ChannelNameOffset));
                    if (!Results["Channel"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Channel"].Add(str);
                    }
                    DebugStr.AppendLine("   Channel: " + str);
                }
                else
                {
                    str = Descriptor.Channel.ToString();
                    if (!Results["Channel"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Channel"].Add(str);
                    }
                    DebugStr.AppendLine("   Channel: " + str);
                }

                if (traceEventInfo.KeywordsNameOffset > 0)
                {
                    for (int offset = traceEventInfo.KeywordsNameOffset; ;)
                    {
                        str = Marshal.PtrToStringUni(
                            IntPtr.Add(TraceEventInfoBuffer, offset));
                        if (string.IsNullOrEmpty(str))
                        {
                            break;
                        }
                        if (!Results["Keywords"].Contains(str,
                            StringComparer.CurrentCultureIgnoreCase))
                        {
                            Results["Keywords"].Add(str);
                        }
                        DebugStr.AppendLine("   Keyword: " + str);
                        offset += Encoding.Unicode.GetByteCount(str) + 2;
                    }
                }

                if (traceEventInfo.TaskNameOffset > 0)
                {
                    str = Marshal.PtrToStringUni(IntPtr.Add(TraceEventInfoBuffer,
                        traceEventInfo.TaskNameOffset));
                    if (!Results["Task"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Task"].Add(str);
                    }
                    DebugStr.AppendLine("   Task: " + str);
                }
                else
                {
                    str = Descriptor.Task.ToString();
                    if (!Results["Task"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Task"].Add(str);
                    }
                    DebugStr.AppendLine("   Task: " + str);
                }

                if (traceEventInfo.OpcodeNameOffset > 0)
                {
                    str = Marshal.PtrToStringUni(IntPtr.Add(TraceEventInfoBuffer,
                        traceEventInfo.OpcodeNameOffset));
                    if (!Results["Opcode"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Opcode"].Add(str);
                    }
                    DebugStr.AppendLine("   Opcode: " + str);
                }
                else
                {
                    str = Descriptor.Opcode.ToString();
                    if (!Results["Opcode"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["Opcode"].Add(str);
                    }
                    DebugStr.AppendLine("   Opcode: " + str);
                }

                ParseEventPropertyInfoArray(
                    TraceEventInfoBuffer,
                    (uint)traceEventInfo.PropertyCount,
                    ref Results,
                    ref DebugStr);
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.EtwProviderParser,
                    TraceEventType.Error,
                    "An exception occurred inside " +
                    "ParseProviderManifestEvent: " + ex.Message);
            }
        }

        private
        static
        void
        ParseEventPropertyInfoArray(
            IntPtr TraceEventInfoBuffer,
            uint NumberOfProperties,
            ref Dictionary<string, List<string>> Results,
            ref StringBuilder DebugStr
            )
        {
            int offset = Marshal.OffsetOf(typeof(TRACE_EVENT_INFO),
                    "EventPropertyInfoArray").ToInt32();
            IntPtr arrayStart = IntPtr.Add(TraceEventInfoBuffer, offset);
            var properties = MarshalHelper.MarshalArray<EVENT_PROPERTY_INFO>(arrayStart,
                        NumberOfProperties);
            int currentPropertyIndex = 0;

            DebugStr.AppendLine("   Properties:");

            foreach (var property in properties)
            {
                if (property.NameOffset != 0)
                {
                    var str = Marshal.PtrToStringUni(
                        IntPtr.Add(TraceEventInfoBuffer, property.NameOffset));
                    if (!Results["UserDataProperties"].Contains(str,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        Results["UserDataProperties"].Add(str);
                    }
                    DebugStr.AppendLine("      " + str + " (" + property.InType + ")");
                }
                else
                {
                    var str = "Unnamed_" + property.InType.ToString() +
                        "_" + currentPropertyIndex;
                    Results["UserDataProperties"].Add(str);
                    DebugStr.AppendLine("      " + str + " (" + property.InType + ")");
                }
                currentPropertyIndex++;
            }
        }
    }
}
