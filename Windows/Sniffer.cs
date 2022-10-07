//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using RpcInvestigator.Util;
using System.Threading;
using RpcInvestigator.Properties;
using System.Collections.Concurrent;
using RpcInvestigator.Windows.Controls;

namespace RpcInvestigator.Windows
{
    using static NativeTraceConsumer;
    using static NativeTraceControl;
    using static TraceLogger;

    public partial class Sniffer : Form
    {
        private readonly Guid s_RpcEtwGuid =
            new Guid("6ad52b32-d609-4be9-ae07-ce8dae937e39");
        private CancellationTokenSource s_CancelSource;
        private ConcurrentQueue<ParsedEtwEvent> m_Queue;
        private System.Windows.Forms.Timer m_RefreshTimer;
        private bool m_EnableDebugEvents;
        private bool m_EnableGroupByActivity;
        private SnifferListview m_Listview;
        private SnifferGraph m_Graph;
        private AutoResetEvent m_TaskCompletedEvent;

        private enum StartButtonState
        {
            None,
            Running,
            Stopped
        }

        public Sniffer(
            RpcLibrary Library,
            SnifferCallbackTable Callbacks,
            ref Settings Settings
            )
        {
            DoubleBuffered = true;
            InitializeComponent();
            m_TaskCompletedEvent = new AutoResetEvent(false);
            m_Listview = new SnifferListview(Callbacks, Settings);
            m_Listview.BuildColumns();
            tableLayoutPanel1.Controls.Add(m_Listview, 0, 1);
            m_Graph = new SnifferGraph(elementHost1, Library, Callbacks);
            startButton.Tag = StartButtonState.None;
            m_RefreshTimer = new System.Windows.Forms.Timer();
            m_RefreshTimer.Tick += RefreshTimer_Tick;
            m_RefreshTimer.Interval = 5000;
        }

        private async void Sniffer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_RefreshTimer.Enabled)
            {
                m_RefreshTimer.Stop();
            }
            if (s_CancelSource != null)
            {
                ToggleButtonsNotAllowedDuringTrace(false);
                e.Cancel = true;
                await Task.Run(() =>
                {
                    s_CancelSource.Cancel();
                    m_TaskCompletedEvent.WaitOne();
                    Task.Delay(1000); // extra delay for continuation task to complete
                });
                s_CancelSource = null;
                Close();
            }
        }

        private void enableDebugEventsButton_Click(object sender, EventArgs e)
        {
            m_EnableDebugEvents = enableDebugEventsButton.Checked;
        }

        private void groupByActivityButton_Click(object sender, EventArgs e)
        {
            m_EnableGroupByActivity = groupByActivityButton.Checked;
            m_Listview.SetGroupingStrategy(m_EnableGroupByActivity);
        }

        private void chooseColumnsButton_Click(object sender, EventArgs e)
        {
            m_Listview.ChooseColumns(s_RpcEtwGuid);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (nodeViewButton.Checked)
            {
                m_Graph.SaveAsImage();
            }
            else
            {
                m_Listview.SaveAsText();
            }
        }

        private void nodeViewButton_Click(object sender, EventArgs e)
        {
            ToggleView();
        }

        private async void startButton_Click(object sender, EventArgs e)
        {
            ToggleButtonsNotAllowedDuringTrace(false);
            var currentState = (StartButtonState)startButton.Tag;
            m_TaskCompletedEvent.Reset();

            if (currentState == StartButtonState.Running)
            {
                //
                // A trace is in progress.
                //
                // Request cancellation for the next time a buffer fills.
                // When the continuation task runs, the UI will be
                // reset to allow another start operation.
                //
                s_CancelSource.Cancel();
                m_RefreshTimer.Stop();
                toolStripStatusLabel1.Text = "Cancellation requested...";
            }
            else
            {
                s_CancelSource = new CancellationTokenSource();
                m_Queue = new ConcurrentQueue<ParsedEtwEvent>();

                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Step = 1;
                toolStripProgressBar1.Maximum = 3; // start, consume, done
                toolStripProgressBar1.Value = 0;
                toolStripStatusLabel1.Text = "Starting trace session...";
                startButton.Image = Resources.stop;
                startButton.Enabled = true;
                startButton.Tag = StartButtonState.Running;
                int sizeBuffersConsumed = 0;
                int eventsConsumed = 0;
                m_Listview.Reset();
                m_Graph.Reset();

                m_RefreshTimer.Enabled = true;
                m_RefreshTimer.Start();
                ToggleView();

                //
                // Start the trace in a separate task
                //
                await Task.Run(() =>
                {
                    using (var trace = new EtwRealTimeTrace(
                        "RPC Investigator Tracing",
                        s_RpcEtwGuid,
                        m_EnableDebugEvents))
                    using (var parserBuffers = new EtwEventParserBuffers())
                    {
                        try
                        {
                            trace.Start();
                            statusStrip1.Invoke((MethodInvoker)(() =>
                            {
                                toolStripProgressBar1.PerformStep();
                                toolStripStatusLabel1.Text = "Consuming events...";
                            }));

                            //
                            // Begin consuming events. This is a blocking call.
                            //
                            trace.Consume(
                            //
                            // Callback when a new event arrives
                            //
                            new EventRecordCallback((Event) =>
                            {
                                //
                                // Check for cancellation request. Event processing
                                // continues until the buffer is completely drained.
                                //
                                if (s_CancelSource.IsCancellationRequested)
                                {
                                    statusStrip1.Invoke((MethodInvoker)(() =>
                                    {
                                        toolStripStatusLabel1.Text = "Cancellation requested...";
                                    }));
                                    return;
                                }
                                var evt = (EVENT_RECORD)Marshal.PtrToStructure(
                                    Event, typeof(EVENT_RECORD));
                                var parser = new EtwEventParser(
                                    evt,
                                    parserBuffers,
                                    trace.GetPerfFreq());
                                try
                                {
                                    var result = parser.Parse();
                                    if (result != null)
                                    {
                                        m_Queue.Enqueue(result);
                                    }
                                    else
                                    {
                                        //
                                        // Uncomment to dump binary event and the parsed
                                        // event up until failure. These files can be
                                        // associated with trace logs via the unique ID
                                        // in the file names.
                                        //
                                        //parser.DumpDiagnosticInfo(Settings.m_LogDir);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Trace(TraceLoggerType.RpcSniffer,
                                        TraceEventType.Error,
                                        "Unable to parse event: " + ex.Message);
                                }
                                eventsConsumed++;
                            }),
                            //
                            // Callback when a trace buffer has been processed
                            //
                            new BufferCallback((LogFile) =>
                            {
                                //
                                // Check for cancellation request. Returning false unblocks
                                // ProcessTrace() and causes trace.Consume to return, which
                                // will invoke our trace object dispose routine to disable
                                // the provider and stop our active trace.
                                //
                                if (s_CancelSource.IsCancellationRequested)
                                {
                                    statusStrip1.Invoke((MethodInvoker)(() =>
                                    {
                                        toolStripStatusLabel1.Text = "Cancellation requested...";
                                    }));
                                    return 0;
                                }

                                var logfile = new EVENT_TRACE_LOGFILE();
                                try
                                {
                                    logfile = (EVENT_TRACE_LOGFILE)
                                        Marshal.PtrToStructure(LogFile, typeof(EVENT_TRACE_LOGFILE));
                                }
                                catch (Exception ex)
                                {
                                    //
                                    // Stop processing
                                    //
                                    Trace(TraceLoggerType.RpcSniffer,
                                        TraceEventType.Error,
                                        "Exception in BufferCallback when casting " +
                                        "pointer to EVENT_TRACE_LOGFILE: " + ex.Message);
                                    return 0;
                                }

                                sizeBuffersConsumed += (int)logfile.Filled;
                                statusStrip1.Invoke((MethodInvoker)(() =>
                                {
                                    toolStripStatusLabel1.Text = "Events: " + eventsConsumed + ", Buffers: " +
                                        logfile.BuffersRead + " (" + Formatting.InfoUnit(sizeBuffersConsumed) +
                                        ")";
                                }));
                                return 1;
                            }));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An exception occurred when consuming events: " + ex.Message);
                            return;
                        }
                    }
                }, s_CancelSource.Token).ContinueWith(result =>
                {
                    statusStrip1.Invoke((MethodInvoker)(() =>
                    {
                        toolStripProgressBar1.Visible = false;
                        toolStripStatusLabel1.Text = "Trace session terminated.";
                        startButton.Image = Resources.play;
                        startButton.Tag = StartButtonState.Stopped;
                        ToggleButtonsNotAllowedDuringTrace(true);
                        m_TaskCompletedEvent.Set();
                        //
                        // TODO: show stats for buffers consumed, etc
                        //
                    }));
                });
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            m_RefreshTimer.Stop();
            int num = 0;

            var events = new List<ParsedEtwEvent>();
            while (m_Queue.TryDequeue(out ParsedEtwEvent evt))
            {
                if (++num > 1000)
                {
                    break;
                }
                events.Add(evt);
            }

            m_Listview.Update(events);
            m_Graph.Update(events);
            m_RefreshTimer.Start();
        }

        private
        void
        ToggleButtonsNotAllowedDuringTrace(bool Enable)
        {
            startButton.Enabled = Enable;
            enableDebugEventsButton.Enabled = Enable;
            chooseColumnsButton.Enabled = Enable;
            saveButton.Enabled = Enable;
        }

        private
        void
        ToggleView()
        {
            if (nodeViewButton.Checked)
            {
                m_Listview.Visible = false;
                elementHost1.Visible = true;
                m_Graph.ToggleVisibility(true);
            }
            else
            {
                elementHost1.Visible = false;
                m_Listview.Visible = true;
                m_Graph.ToggleVisibility(false);
            }
        }
    }

    public delegate void ShowRpcServerDetails(string InterfaceId);

    public class SnifferCallbackTable
    {
        public ShowRpcServerDetails ShowRpcServerDetailsCallback;
    }
}
