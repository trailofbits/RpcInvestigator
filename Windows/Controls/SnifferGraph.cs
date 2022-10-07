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
using RpcInvestigator.Util;
using System.Windows.Forms.Integration;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.OverlapRemoval;
using GraphX.Logic.Models;
using GraphX.Controls;
using GraphX.Controls.Models;
using QuickGraph;
using GraphX.Common.Models;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using Brushes = System.Windows.Media.Brushes;
using Style = System.Windows.Style;
using Binding = System.Windows.Data.Binding;
using Control = System.Windows.Controls.Control;
using GraphX.Common.Interfaces;

namespace RpcInvestigator.Windows.Controls
{
    public class SnifferGraph
    {
        private readonly RpcLibrary m_Library;
        private readonly SnifferCallbackTable m_Callbacks;
        private ZoomControl m_Zoom;
        private SnifferGraphArea m_GraphArea;
        private Dictionary<long, string> m_PidLookupCache;

        public SnifferGraph(
            ElementHost Host,
            RpcLibrary Library,
            SnifferCallbackTable Callbacks
            )
        {
            m_PidLookupCache = new Dictionary<long, string>();
            m_Callbacks = Callbacks;
            m_Library = Library;

            m_Zoom = new ZoomControl();
            Host.Child = m_Zoom;
            m_Zoom.ZoomToFill();
            ZoomControl.SetViewFinderVisibility(m_Zoom, System.Windows.Visibility.Visible);
            var logic = new GXLogicCore<SnifferNode, SnifferEdge, BidirectionalSnifferGraph>();
            m_GraphArea = new SnifferGraphArea
            {
                LogicCore = logic,
                EdgeLabelFactory = new DefaultEdgelabelFactory()
            };
            m_GraphArea.LogicCore.Graph = new BidirectionalSnifferGraph();
            logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            var layoutParams = (LinLogLayoutParameters)
                logic.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.LinLog);
            logic.DefaultLayoutAlgorithmParams = layoutParams;
            logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 20;
            ((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 20;
            logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            logic.AsyncAlgorithmCompute = false;
            m_Zoom.Content = m_GraphArea;
            OverrideTemplateStyles();
            m_GraphArea.RelayoutFinished += SnifferGraphArea_RelayoutFinished;
            m_GraphArea.VertexClicked += NodeClicked;
            ToggleVisibility(false);
        }

        void SnifferGraphArea_RelayoutFinished(object sender, EventArgs e)
        {
            m_Zoom.ZoomToFill();
        }

        private void NodeClicked(object sender, VertexClickedEventArgs e)
        {
            var node = e.Control.Vertex as SnifferNode;
            if (node.UserData == null || node.UserData.GetType() != typeof(SnifferNodeUserData))
            {
                return;
            }
            if (!Guid.TryParse(node.UserData.InterfaceId, out Guid parsed))
            {
                MessageBox.Show("The InterfaceUuid has an invalid format.");
                return;
            }
            m_Callbacks.ShowRpcServerDetailsCallback(parsed.ToString());
        }

        private
        void
        OverrideTemplateStyles()
        {
            //
            // Ellipsis node type
            //
            var ellipsisNodeTrigger = new DataTrigger()
            {
                Binding = new Binding("NodeType"),
                Value = SnifferNodeType.Ellipsis
            };
            ellipsisNodeTrigger.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = Brushes.LightBlue
            });
            //
            // RPC server node type
            //
            var rpcServerNodeTrigger = new DataTrigger()
            {
                Binding = new Binding("NodeType"),
                Value = SnifferNodeType.RpcServer
            };
            rpcServerNodeTrigger.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = Brushes.Red
            });
            rpcServerNodeTrigger.Setters.Add(new Setter()
            {
                Property = VertexControl.VertexShapeProperty,
                Value = VertexShape.Rectangle
            });
            //
            // Process node type
            //
            var processNodeTrigger = new DataTrigger()
            {
                Binding = new Binding("NodeType"),
                Value = SnifferNodeType.Process
            };
            processNodeTrigger.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = Brushes.Blue
            });
            processNodeTrigger.Setters.Add(new Setter()
            {
                Property = Control.ForegroundProperty,
                Value = Brushes.White
            });

            //
            // There's probably a better way, but this works.
            //
            m_Zoom.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Windows/Controls/GraphTemplate.xaml", UriKind.Relative)
            });
            var style = (Style)m_Zoom.Resources.MergedDictionaries[0][typeof(VertexControl)];
            style.Setters.Clear();
            style.Triggers.Clear();
            style.Triggers.Add(ellipsisNodeTrigger);
            style.Triggers.Add(rpcServerNodeTrigger);
            style.Triggers.Add(processNodeTrigger);
        }

        public
        void
        ToggleVisibility(bool Visible)
        {
            m_GraphArea.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
        }

        public void Update(List<ParsedEtwEvent> Events)
        {
            foreach (var evt in Events)
            {
                //
                // Ignore events that don't contain an interface ID for the RPC server
                // involved or activity ID.  This greatly reduces noise and the
                // back-and-forth parts of a typical RPC call which show up here as
                // multiple ETW events but represent one logical call.
                // Also, ignore this tool as well.
                //
                if (!evt.UserDataProperties.ContainsKey("InterfaceUuid") ||
                    evt.ActivityId == null || evt.ActivityId == Guid.Empty ||
                    evt.ProcessId == Process.GetCurrentProcess().Id)
                {
                    continue;
                }

                //
                // Only draw nodes/edge for an activity event group one time.
                //
                var activityId = evt.ActivityId.ToString();
                var edgeId = activityId;
                var existingActivityEdge = m_GraphArea.EdgesList.Where(
                    e => e.Key.SnifferId == activityId);
                if (existingActivityEdge.Count() != 0)
                {
                    continue;
                }

                var rpcServerUuid = evt.UserDataProperties["InterfaceUuid"];
                var existing = m_GraphArea.VertexList.Where(
                    v => v.Key.SnifferId == rpcServerUuid);
                SnifferNode rpcServerNode;

                if (existing.Count() == 0)
                {
                    var node = new SnifferNode()
                    {
                        NodeType = SnifferNodeType.RpcServer,
                        SnifferId = rpcServerUuid,
                        UserData = new SnifferNodeUserData()
                        {
                            InterfaceId = rpcServerUuid,
                            EdgeCount = 0,
                        },
                        Text = rpcServerUuid,
                    };
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
                        node.Text = friendly + rpcServerUuid;
                    }
                    var control = new VertexControl(node);
                    m_GraphArea.AddVertexAndData(node, control);
                    rpcServerNode = node;
                }
                else
                {
                    rpcServerNode = existing.ToList()[0].Key;
                }

                //
                // Avoid edge explosion for chatty RPC servers.
                // TODO: Make this customizable.
                //
                if (rpcServerNode.UserData.EdgeCount > 10)
                {
                    AddOrUpdateEllipsisNode(rpcServerNode);
                    continue;
                }
                ConnectRpcNode(activityId, rpcServerNode, evt);
            }
            m_GraphArea.SetEdgesDrag(false);
            m_GraphArea.SetVerticesDrag(false);
            m_GraphArea.GenerateGraph(true);
            m_GraphArea.ShowAllEdgesLabels(false);
            m_Zoom.ZoomToFill();
        }

        private void ConnectRpcNode(
            string ActivityId,
            SnifferNode RpcServerNode,
            ParsedEtwEvent Event
            )
        {
            //
            // Increment the node's edge count to prevent edge explosion.
            //
            RpcServerNode.UserData.EdgeCount++;

            //
            // TODO: The PID could have been reused. We should probably
            // build and maintain a best-effort mapping.
            //
            string processName = null;
            if (!m_PidLookupCache.ContainsKey(Event.ProcessStartKey))
            {
                try
                {
                    Process p = Process.GetProcessById((int)Event.ProcessId);
                    processName = p.ProcessName + "(" + Event.ProcessId + ")";
                }
                catch (Exception)
                {
                    processName = "[" + Event.ProcessId.ToString() + "]";
                }
                m_PidLookupCache.Add(Event.ProcessStartKey, processName);
            }
            else
            {
                processName = m_PidLookupCache[Event.ProcessStartKey];
            }

            var existingProcessNode = m_GraphArea.VertexList.Where(
                v => v.Key.SnifferId == processName);
            SnifferNode processNode;

            if (existingProcessNode.Count() == 0)
            {
                var node = new SnifferNode()
                {
                    NodeType = SnifferNodeType.Process,
                    SnifferId = processName,
                    Text = processName,
                };
                var control = new VertexControl(node);
                m_GraphArea.AddVertexAndData(node, control);
                processNode = node;
            }
            else
            {
                processNode = existingProcessNode.ToList()[0].Key;
            }

            var processNodeControl = m_GraphArea.VertexList[processNode];

            //
            // RPC etw events have tasks (RpcClientCall, RpcServerCall)
            // and opcodes (Start, Stop) which would result in multiple
            // redundant edges between a process and an RPC server. We
            // have already filtered by ActivityId, so we should never
            // have to check that an edge exists here.
            //
            var rpcServerNodeControl = m_GraphArea.VertexList[RpcServerNode];
            var edgeId = ActivityId;
            var edgeFromRpcServerNodeToProcessNode =
                new SnifferEdge(processNode, RpcServerNode)
            {
                SnifferId = edgeId,
                RpcNodeSnifferId = RpcServerNode.SnifferId,
            };
            var edgeControl = new EdgeControl(processNodeControl,
                    rpcServerNodeControl,
                    edgeFromRpcServerNodeToProcessNode);
            m_GraphArea.AddEdgeAndData(edgeFromRpcServerNodeToProcessNode, edgeControl);
        }

        private void AddOrUpdateEllipsisNode(SnifferNode RpcServerNode)
        {
            if (RpcServerNode.UserData.EllipsisNode != null)
            {
                var count = RpcServerNode.UserData.EdgeCount++;
                RpcServerNode.UserData.EllipsisNode.Text = "+" + count + " more...";
                return;
            }

            //
            // No ellipsis node exists for this RPC server. Create it now.
            //
            var nodeId = RpcServerNode.UserData.InterfaceId + "-ellipsis";
            var node = new SnifferNode()
            {
                NodeType = SnifferNodeType.Ellipsis,
                SnifferId = nodeId,
                UserData = new SnifferNodeUserData(),
                Text = "+1 more..."
            };
            var control = new VertexControl(node);
            m_GraphArea.AddVertexAndData(node, control);
            //
            // Connect it to the RPC server node.
            //
            var rpcServerNodeControl = m_GraphArea.VertexList[RpcServerNode];
            var edgeFromRpcServerNodeToEllipsisNode =
                new SnifferEdge(node, RpcServerNode);
            m_GraphArea.AddEdgeAndData(
                edgeFromRpcServerNodeToEllipsisNode,
                new EdgeControl(
                    control,
                    rpcServerNodeControl,
                    edgeFromRpcServerNodeToEllipsisNode));
            RpcServerNode.UserData.EllipsisNode = node;
        }

        public void Reset()
        {
            m_GraphArea.RemoveAllVertices();
            m_GraphArea.RemoveAllEdges();
        }

        public void SaveAsImage()
        {
            if (m_GraphArea.LogicCore.Graph.IsVerticesEmpty)
            {
                return;
            }
            m_GraphArea.ExportAsImageDialog(ImageType.PNG);
        }
    }

    public class SnifferGraphArea : GraphArea<SnifferNode, SnifferEdge, BidirectionalSnifferGraph> { }

    public class BidirectionalSnifferGraph : BidirectionalGraph<SnifferNode, SnifferEdge> { }

    public enum SnifferNodeType
    {
        Ellipsis,
        Process,
        RpcServer
    }

    public class SnifferNode : VertexBase
    {
        public string SnifferId { get; set; }
        public SnifferNodeUserData UserData { get; set; }
        public string Text { get; set; }
        public SnifferNodeType NodeType { get; set; }

        public SnifferNode() { }

        public override string ToString()
        {
            return Text;
        }
    }

    public class SnifferNodeUserData
    {
        public string InterfaceId;
        public int EdgeCount;
        public SnifferNode EllipsisNode;
    }

    public class SnifferEdge : EdgeBase<SnifferNode>
    {
        public string SnifferId { get; set; }
        public string RpcNodeSnifferId { get; set; }
        public object UserData { get; set; }
        public string Text { get; set; }

        public SnifferEdge(SnifferNode source, SnifferNode target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public SnifferEdge()
            : base(null, null, 1)
        {
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
