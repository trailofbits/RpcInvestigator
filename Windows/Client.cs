//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using NtApiDotNet.Win32;
using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.IO;
using System.Globalization;
using FastColoredTextBoxNS;

namespace RpcInvestigator
{
    public partial class Client : Form
    {
        private readonly RpcServer m_RpcServer;
        private readonly RpcEndpoint m_Endpoint;

        public Client(RpcServer Server, RpcEndpoint Endpoint)
        {
            InitializeComponent();
            HideConnectControls();
            m_RpcServer = Server;
            m_Endpoint = Endpoint;
        }

        private void Client_Shown(object sender, EventArgs e)
        {
            var last = AddLabels(
                new Dictionary<string, string>(){
                    { "UUID", m_RpcServer.InterfaceId.ToString() },
                    { "Version", m_RpcServer.InterfaceVersion.ToString() },
                    { "Name", m_RpcServer.Name.ToString() },
                    { "Transfer Syntax ID", m_RpcServer.TransferSyntaxId.ToString() },
                    { "Transfer Syntax Version", m_RpcServer.TransferSyntaxVersion.ToString() },
                    { "FilePath", m_RpcServer.FilePath.ToString() },
                    { "Endpoint Count", m_RpcServer.EndpointCount.ToString() },
                    { "Procedure Count", m_RpcServer.ProcedureCount.ToString() },
                },
                rpcServerStartLabel.Location.X,
                rpcServerStartLabel.Location.Y);

            if (m_Endpoint != null)
            {
                AddLabels(
                    new Dictionary<string, string>()
                    {
                        { "Endpoint information", "" },
                        { "Endpoint Name", m_Endpoint.Endpoint.ToString() },
                        { "Protocol Sequence", m_Endpoint.ProtocolSequence.ToString() },
                        { "Binding String", m_Endpoint.BindingString.ToString() },
                        { "Endpoint Path", m_Endpoint.EndpointPath.ToString() },
                    },
                rpcServerStartLabel.Location.X,
                last.Item2);
            }

            try
            {
                SetSourceCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem compiling the RPC client " +
                    "code: " + ex.Message);
            }

            RpcProcedureList proceduresTab = new RpcProcedureList();
            workbenchTabControl.TabPages.Add(proceduresTab);
            proceduresTab.Build(m_RpcServer.Name, m_RpcServer.Procedures.ToList());
        }

        private
        void
        HideConnectControls()
        {
            foreach (var control in rpcServerGroupbox.Controls.Cast<Control>().ToList())
            {
                control.Visible = false;
            }
        }

        private
        (int,int)
        AddLabels(
            Dictionary<string, string> Values,
            int StartX,
            int StartY
            )
        {
            int longestNameX = 0;
            int currY = StartY;
            int fixedX = StartX;
            Values.Keys.ToList().ForEach(name =>
            {
                var label = new Label();
                label.AutoSize = true;
                label.Name = name.Replace(" ", "_") + "_Label";
                label.Text = name + ":";
                label.Location = new Point(fixedX, currY);
                label.PerformLayout();
                rpcServerGroupbox.Controls.Add(label);
                currY += label.Height + 3;
                longestNameX = Math.Max(longestNameX, label.Width);
            });
            currY = StartY;
            fixedX = StartX + longestNameX + 5;
            Values.Values.ToList().ForEach(value =>
            {
                Random rand = new Random();
                var label = new Label();
                label.AutoSize = true;
                label.Name = "LabelValue" + rand.Next();
                label.Text = value;
                label.Location = new Point(fixedX, currY);
                rpcServerGroupbox.Controls.Add(label);
                currY += label.Height + 3;
            });

            return (fixedX, currY);
        }

        private async void runButton_Click(object sender, EventArgs e)
        {
            Assembly assembly;

            //
            // Compile the source
            //
            try
            {
                assembly = ClientCompiler.Compile(rpcClientSourceCode.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            var clientType = assembly.GetType("RpcInvestigator.Client1", true);
            var client = Activator.CreateInstance(clientType);

            try
            {
                //
                // Instantiate the client and connect to the server. If an endpoint was
                // provided to us, use it.
                //
                if (m_Endpoint != null)
                {
                    ((RpcClientBase)client).Connect(m_Endpoint, new RpcTransportSecurity());
                }
                else
                {
                    /*
                    var endpoints = RpcEndpointMapper.FindAlpcEndpointForInterface(
                            m_RpcServer.InterfaceId, m_RpcServer.InterfaceVersion);
                    if (endpoints == null || endpoints.Count() == 0)
                    {
                        MessageBox.Show("Unable to locate an endpoint for server " +
                            m_RpcServer.InterfaceId.ToString() + ", version " +
                            m_RpcServer.InterfaceVersion.ToString() + " - unable to " +
                            "retrieve server definition.");
                    }
                    */
                    ((RpcClientBase)client).Connect();
                }

                //
                // Get the 'Run' method and execute it
                //
                var run = clientType.GetMethod("Run");
                if (run == null)
                {
                    throw new Exception("Method 'Run' must exist in client class.");
                }
                if (run.ReturnType != typeof(Task<bool>))
                {
                    throw new Exception("Method 'Run' must return Task<bool> type.");
                }
                //
                // Capture our own stdout, as the assembly will run in a sub-process of us.
                //
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                Console.SetOut(writer);
                _ = await (Task<bool>)run.Invoke(
                   client,
                   BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                   null,
                   null,
                   CultureInfo.CurrentCulture);
                outputRichTextBox.Text += "> Run() output:" + Environment.NewLine +
                    sb.ToString() +
                    Environment.NewLine;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Run() failed:  " + ex.Message);
                return;
            }
            finally
            {
                ((RpcClientBase)client).Disconnect();
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            try
            {
                SetSourceCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem compiling the RPC client " +
                    "code: " + ex.Message);
            }
        }

        private void SetSourceCode()
        {
            var code = ClientCompiler.GetBaseClientCode(m_RpcServer);
            rpcClientSourceCode.Text = code;
            var lines = rpcClientSourceCode.FindLines(@"public async Task\<bool\> Run",
                System.Text.RegularExpressions.RegexOptions.None);
            if (lines.Count != 1)
            {
                throw new Exception("Unexpected source code layout.");
            }
            var range = new Range(rpcClientSourceCode, lines[0]);
            rpcClientSourceCode.DoCaretVisible();
            var regions = rpcClientSourceCode.FindLines("#region ", System.Text.RegularExpressions.RegexOptions.None);
            foreach (var line in regions)
            {
                if (rpcClientSourceCode.Lines[line].Contains("#region Client Implementation"))
                {
                    continue;
                }
                rpcClientSourceCode.CollapseFoldingBlock(line);
            }
            rpcClientSourceCode.Selection = range;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            var uuid = uuidValue.Text;
            var version = versionValue.Text;
            var endpoint = endpointValue.Text;
            var protocol = protocolValue.Text;
            if (string.IsNullOrEmpty(uuid) || string.IsNullOrEmpty(version) ||
                string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(protocol))
            {
                MessageBox.Show("Please specify UUID, version, endpoint and protocol.");
                return;
            }

            try
            {
                var client = new RpcClient(Guid.Parse(uuid), new Version(version));
                var security = new RpcTransportSecurity();
                security.AuthenticationLevel = RpcAuthenticationLevel.None;
                security.AuthenticationType = RpcAuthenticationType.None;
                security.AuthenticationCapabilities = RpcAuthenticationCapabilities.None;
                client.Connect(protocol, endpoint, security);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect: " + ex.Message);
                return;
            }
            MessageBox.Show("Connected OK");
        }

    }

    public static class ClientCompiler
    {
        public static Assembly Compile(string Source)
        {
            CompilerParameters compile_params = new CompilerParameters();
            using (TempFileCollection temp_files = new TempFileCollection(Path.GetTempPath()))
            {
                compile_params.GenerateExecutable = false;
                compile_params.GenerateInMemory = true;
                compile_params.IncludeDebugInformation = true;
                compile_params.TempFiles = temp_files;
                compile_params.ReferencedAssemblies.Add(typeof(RpcClientBuilder).Assembly.Location);
                compile_params.ReferencedAssemblies.Add("System.dll");
                compile_params.ReferencedAssemblies.Add("System.Core.dll");
                temp_files.KeepFiles = true;
                CompilerResults results = CodeDomProvider.CreateProvider("CSharp").
                    CompileAssemblyFromSource(compile_params, Source);
                if (results.Errors.HasErrors)
                {
                    StringBuilder errors = new StringBuilder();
                    foreach (CompilerError e in results.Errors)
                    {
                        errors.AppendLine(e.ToString());
                    }
                    throw new Exception("Compiler errors:" + errors.ToString());
                }
                return results.CompiledAssembly;
            }
        }

        public static string GetBaseClientCode(RpcServer Server)
        {
            //
            // TODO: It would be nice if RpcClientBuilder exposed its internal
            // routines for different parts of the source code creation process,
            // such that we could add method definitions to the resulting Client
            // class prior to source code generation.
            // As is stands, all those things are marked private and we can only
            // either get back an immutable assembly object or the finalized source
            // code as text, neither of which are ideal for modifying the code.
            // So this becomes a text search problem.
            //
            RpcClientBuilderArguments args = new RpcClientBuilderArguments();
            args.Flags = RpcClientBuilderFlags.GenerateConstructorProperties |
                RpcClientBuilderFlags.StructureReturn |
                RpcClientBuilderFlags.HideWrappedMethods |
                RpcClientBuilderFlags.UnsignedChar |
                RpcClientBuilderFlags.NoNamespace |
                RpcClientBuilderFlags.MarshalPipesAsArrays |
                RpcClientBuilderFlags.ExcludeVariableSourceText;
            args.NamespaceName = "RpcInvestigator";
            args.ClientName = "Client1";
            var src = RpcClientBuilder.BuildSource(Server, args);
            src = "using System;" + Environment.NewLine +
                "using System.Threading.Tasks;" + Environment.NewLine +
                "using NtApiDotNet.Win32;" + Environment.NewLine +
                "using NtApiDotNet;" + Environment.NewLine +
                "using NtApiDotNet.Ndr.Marshal;" + Environment.NewLine +
                src;
            int loc = src.IndexOf("public Client1()");
            src = src.Insert(loc,
                "public async Task<bool> Run()" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            return true;" + Environment.NewLine +
                "        }" + Environment.NewLine+
                "        ");
            return src;

        }
    }
}
