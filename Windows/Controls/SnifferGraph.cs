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
using System.Linq;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using RpcInvestigator.Util;

namespace RpcInvestigator.Windows.Controls
{
    internal class SnifferGraph
    {
        public GViewer m_Viewer;
        private readonly RpcLibrary m_Library;
        private readonly SnifferCallbackTable m_Callbacks;

        public SnifferGraph(RpcLibrary Library, SnifferCallbackTable Callbacks)
        {
            m_Callbacks = Callbacks;
            m_Library = Library;
            m_Viewer = new GViewer();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sniffer));
            m_Viewer.Transform = ((Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation)(resources.GetObject("graphViewer.Transform")));
            m_Viewer.Graph = new Graph("SnifferGraph");
            m_Viewer.Dock = DockStyle.Fill;
            m_Viewer.CurrentLayoutMethod = LayoutMethod.IcrementalLayout;
            m_Viewer.Click += GraphClicked;
            ToggleVisibility(false);
        }

        private void GraphClicked(object sender, EventArgs e)
        {
            GViewer viewer = sender as GViewer;
            if (!(viewer.SelectedObject is Node))
            {
                return;
            }
            Node node = viewer.SelectedObject as Node;
            //
            // Bug in GViewer: undo the node stroke style
            //
            node.Attr.LineWidth = 1;
            if (node.UserData == null || node.UserData.GetType() != typeof(ParsedEtwEvent))
            {
                return;
            }
            var evt = (ParsedEtwEvent)node.UserData;
            if (!evt.UserDataProperties.ContainsKey("InterfaceUuid"))
            {
                MessageBox.Show("Unable to show RPC server details because " +
                    "there is no interface UUID present in the ETW event.");
                return;
            }
            var interfaces = evt.UserDataProperties["InterfaceUuid"];
            if (interfaces.Count() != 1)
            {
                MessageBox.Show("The InterfaceUuid has an invalid format.");
                return;
            }
            var interfaceId = evt.UserDataProperties["InterfaceUuid"].Replace(
                "{", "").Replace("}", "");
            m_Callbacks.ShowRpcServerDetailsCallback(interfaceId);
        }

        public
        void
        ToggleVisibility(bool Visible)
        {
            m_Viewer.Visible = Visible;
        }

        public
        void
        Update(List<ParsedEtwEvent> Events)
        {
            m_Viewer.NeedToCalculateLayout = false;
            if (m_Viewer.Graph == null)
            {
                m_Viewer.Graph = new Graph("SnifferGraph");
            }

            foreach (var evt in Events)
            {
                if (!evt.UserDataProperties.ContainsKey("InterfaceUuid"))
                {
                    return;
                }
                var graph = m_Viewer.Graph;
                var rpcServerUuid = evt.UserDataProperties["InterfaceUuid"];
                var rpcServerNode = graph.FindNode(rpcServerUuid);
                if (rpcServerNode == null)
                {
                    var node = new Node(rpcServerUuid);
                    node.Attr.FillColor = Color.Red;
                    node.Attr.Shape = Shape.Box;
                    node.Attr.LabelMargin = 5;
                    node.Id = rpcServerUuid;
                    node.LabelText = rpcServerUuid;
                    node.UserData = evt;
                    var servers = m_Library.Get(new Guid(rpcServerUuid));
                    if (servers != null)
                    {
                        //
                        // TODO: Icon to show this info came from our RPC library.
                        // Also, we should add ALPC port information if it's available.
                        //
                        var friendly = "";
                        if (!string.IsNullOrEmpty(servers[0].ServiceName))
                        {
                            friendly = servers[0].ServiceName + Environment.NewLine;
                        }
                        else if (!string.IsNullOrEmpty(servers[0].FilePath))
                        {
                            friendly = servers[0].FilePath + Environment.NewLine;
                        }
                        node.LabelText = friendly + rpcServerUuid;
                    }
                    graph.AddNode(node);
                    rpcServerNode = node;
                }
                //
                // Avoid graph explosion for very chatty RPC servers.
                //
                if (rpcServerNode.InEdges.Count() > 10)
                {
                    //
                    // Add an "..." ellipsis node with count.
                    //
                    var eId = rpcServerUuid + "-ellipsis";
                    var ellipsisNode = graph.FindNode(eId);
                    int count = 1;
                    if (ellipsisNode == null)
                    {
                        var node = new Node(eId);
                        node.Attr.FillColor = Color.LightBlue;
                        node.Attr.Shape = Shape.Circle;
                        node.Attr.AddStyle(Style.Dashed);
                        node.Attr.LabelMargin = 5;
                        node.Id = eId;
                        //
                        // TODO: Make node clickable via URI
                        //
                        graph.AddNode(node);
                        ellipsisNode = node;
                        //
                        // Connect it to the RPC server node.
                        //
                        graph.AddPrecalculatedEdge(new Edge(
                                ellipsisNode, rpcServerNode, ConnectionToGraph.Connected));
                    }
                    else
                    {
                        count = (int)ellipsisNode.UserData + 1;
                    }
                    ellipsisNode.UserData = count;
                    ellipsisNode.LabelText = "+" + count + " more...";
                    RefreshGraph(graph);
                    return;
                }
                //
                // TODO: The PID could have been reused. We should probably
                // build and maintain a best-effort mapping.
                //
                string processName = "[" + evt.ProcessId.ToString() + "]";
                try
                {
                    Process p = Process.GetProcessById((int)evt.ProcessId);
                    processName = p.ProcessName + "(" + evt.ProcessId + ")";
                }
                catch (Exception) { } // swallow
                var processKey = evt.ProcessStartKey.ToString("X");
                var processNode = graph.FindNode(processKey);
                if (processNode == null)
                {
                    var node = new Node(processKey);
                    node.Id = processKey;
                    node.LabelText = processName;
                    node.Attr.FillColor = Color.Blue;
                    node.Attr.Shape = Shape.Circle;
                    node.Attr.LabelMargin = 5;
                    node.Label.FontColor = Color.White;
                    //
                    // TODO: Make node clickable via URI
                    //
                    graph.AddNode(node);
                    processNode = node;
                }
                //
                // RPC etw events have tasks (RpcClientCall, RpcServerCall)
                // and opcodes (Start, Stop) which would result in multiple
                // redundant edges between a process and an RPC server. We
                // treat them all as a single edge.
                //
                var edgeId = rpcServerUuid + "-" + processKey;
                var edge = graph.EdgeById(edgeId);
                if (edge == null)
                {
                    edge = new Edge(
                        processNode, rpcServerNode, ConnectionToGraph.Connected);
                    edge.Attr.Color = Color.Black;
                    edge.Attr.Id = edgeId;
                    //
                    // TODO: Make edge clickable via URI
                    //
                    graph.AddPrecalculatedEdge(edge);
                }
                RefreshGraph(graph);
            }
        }

        public void Reset()
        {
            m_Viewer.Graph = null;
        }

        private void RefreshGraph(Graph NewGraph)
        {
            m_Viewer.NeedToCalculateLayout = true;
            m_Viewer.Graph = NewGraph;
            m_Viewer.NeedToCalculateLayout = false;
            m_Viewer.Graph = NewGraph;
        }
    }
}
