//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using RpcInvestigator.Util;
using System.IO;

namespace UnitTests
{
    using static NativeTraceConsumer;
    using static NativeTraceControl;

    [TestClass]
    public class EtwTests
    {
        private readonly Guid s_RpcEtwGuid =
            new Guid("6ad52b32-d609-4be9-ae07-ce8dae937e39");
        private readonly int s_NumEvents = 1000;

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task RealTimeEtwTraceTest(bool EnableDebugEvents)
        {
            var env = new Environment();
            env.Initialize();
            int eventsConsumed = 0;

            await Task.Run(() =>
            {
                //
                // This trace will automatically terminate after a set number
                // of ETW events have been successfully consumed/parsed.
                //
                using (var trace = new EtwRealTimeTrace(
                    "RPC Investigator Unit Test Real-Time Tracing",
                    s_RpcEtwGuid,
                    EnableDebugEvents))
                using (var parserBuffers = new EtwEventParserBuffers())
                {
                    try
                    {
                        trace.Start();

                        //
                        // Begin consuming events. This is a blocking call.
                        //
                        trace.Consume(new EventRecordCallback((Event) =>
                        {
                            var parser = new EtwEventParser(
                                Event,
                                parserBuffers,
                                trace.GetPerfFreq());
                            try
                            {
                                var result = parser.Parse();
                                Assert.IsNotNull(result);
                            }
                            catch (Exception ex)
                            {
                                Assert.Fail("Unable to parse event: " + ex.Message);
                            }
                            eventsConsumed++;
                        }),
                        new BufferCallback((LogFile) =>
                        {
                            var logfile = new EVENT_TRACE_LOGFILE();
                            try
                            {
                                logfile = (EVENT_TRACE_LOGFILE)
                                    Marshal.PtrToStructure(LogFile, typeof(EVENT_TRACE_LOGFILE));
                            }
                            catch (Exception ex)
                            {
                                Assert.Fail("Unable to cast EVENT_TRACE_LOGFILE: " + ex.Message);
                            }
                            if (eventsConsumed >= s_NumEvents)
                            {
                                return false;
                            }
                            return true;
                        }));
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("An exception occurred when consuming events: " + ex.Message);
                    }
                }
            });
        }

        [TestMethod]
        [DeploymentItem(@"..\..\data\trace_files\ms-rpc-capture-arrays.etl")]
        public void FileEtwTraceTest()
        {
            var env = new Environment();
            env.Initialize();
            int eventsConsumed = 0;

            //
            // This trace will automatically terminate after a set number
            // of ETW events have been successfully consumed/parsed.
            //
            var current = Directory.GetCurrentDirectory();
            var target = Path.Combine(current, "ms-rpc-capture-arrays.etl");

            using (var trace = new EtwFileTrace(target))
            using (var parserBuffers = new EtwEventParserBuffers())
            {
                try
                {
                    //
                    // Begin consuming events. This is a blocking call.
                    //
                    trace.Consume(new EventRecordCallback((Event) =>
                    {
                        if (!Event.EventHeader.ProviderId.Equals(s_RpcEtwGuid))
                        {
                            //
                            // Skip events from other providers, because it might not
                            // be a builtin provider, in which case we'd need to go
                            // find the right manifest and that is overly complex for
                            // this unit test.
                            //
                            return;
                        }
                        var parser = new EtwEventParser(
                            Event,
                            parserBuffers,
                            trace.GetPerfFreq());
                        try
                        {
                            var result = parser.Parse();
                            Assert.IsNotNull(result, "Failed to parse the event");
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Unable to parse event: " + ex.Message);
                        }
                        eventsConsumed++;
                    }),
                    new BufferCallback((LogFile) =>
                    {
                        var logfile = new EVENT_TRACE_LOGFILE();
                        try
                        {
                            logfile = (EVENT_TRACE_LOGFILE)
                                Marshal.PtrToStructure(LogFile, typeof(EVENT_TRACE_LOGFILE));
                        }
                        catch (Exception ex)
                        {
                            Assert.Fail("Unable to cast EVENT_TRACE_LOGFILE: " + ex.Message);
                        }
                        if (eventsConsumed >= s_NumEvents)
                        {
                            return false;
                        }
                        return true;
                    }));
                }
                catch (Exception ex)
                {
                    Assert.Fail("An exception occurred when consuming events: " + ex.Message);
                }
            }
        }
    }
}
