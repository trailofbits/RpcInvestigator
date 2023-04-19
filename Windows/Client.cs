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
using System.IO;
using System.Globalization;
using Range = FastColoredTextBoxNS.Range;
using FastColoredTextBoxNS;
using RpcInvestigator.Util;
using RpcInvestigator.Util.ML;
using System.Collections;
using RpcInvestigator.Windows.Controls;

namespace RpcInvestigator
{
    public partial class Client : Form
    {
        private readonly RpcServer m_RpcServer;
        private readonly RpcEndpoint m_Endpoint;
        private Settings m_Settings;
        private SkManager m_SkManager;
        private AutocompleteMenu m_AutocompletePopup;

        public Client(RpcServer Server, RpcEndpoint Endpoint, Settings Settings)
        {
            InitializeComponent();
            m_AutocompletePopup = new AutocompleteMenu(inputJson);
            m_AutocompletePopup.MinFragmentLength = 2;
            HideConnectControls();
            m_RpcServer = Server;
            m_Endpoint = Endpoint;
            m_Settings = Settings;

            FormClosed += new FormClosedEventHandler(delegate (object s, FormClosedEventArgs e)
            {
                //
                // Since this form is shown with ShowDialog() instead of Show(),
                // we can't do this during Disposed event, because according to
                // docs, Dispose is never called on the form for ShowDialog().
                //
                m_SkManager.Dispose();
            });
        }

        private void Client_Shown(object sender, EventArgs e)
        {
            //
            // Initialize semantic kernel
            //
            var arguments = new ArrayList {
                m_RpcServer
            };
            m_SkManager = new SkManager(
                ProgressUpdate,
                UpdateStatus,
                arguments);
            m_SkManager.Initialize();

            //
            // Add information about this RPC server and endpoint to the info groupbox.
            //
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

            //
            // Generate and display the c# client source code for this RPC server.
            //
            try
            {
                SetSourceCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem compiling the RPC client " +
                    "code: " + ex.Message);
            }

            //
            // Generate a tab page to display the procedures for this RPC server.
            //
            RpcProcedureList proceduresTab = new RpcProcedureList();
            workbenchTabControl.TabPages.Add(proceduresTab);
            proceduresTab.Build(m_RpcServer.Name, m_RpcServer.Procedures.ToList());

            //
            // Populate available SK skills for the LLM tab.
            //
            var skills = m_SkManager.GetAvailableSkills();
            if (skills.Count > 0)
            {
                availableSkills.Items.AddRange(skills.ToArray());
            }

            //
            // Setup autocomplete source for inputJson textbox on LLM tab.
            //
            var items = new List<AutoCompleteItem>();
            foreach (var item in rpcClientSourceCode.Lines)
            {
                var line = item.Trim();
                if (line == "}" || line == "{" || line.StartsWith("//") ||
                    string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }
                items.Add(new AutoCompleteItem(line));
            }
            m_AutocompletePopup.Items.SetAutocompleteItems(items);
            m_AutocompletePopup.Items.MaximumSize = new System.Drawing.Size(750, 500);
            m_AutocompletePopup.Items.Width = 750;
        }

        #region Rpc client code tab

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
        (int, int)
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
                rpcClientOutput.Text += "> Run() output:" + Environment.NewLine +
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

        private void copyClientCodeButton_Click(object sender, EventArgs e)
        {
            var text = rpcClientSourceCode.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            Clipboard.SetText(text);
        }

        #endregion

        #region LLM tab


        private void copyResponseButton_Click(object sender, EventArgs e)
        {
            var text = llmResponse.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            Clipboard.SetText(text);
        }

        private async void submitButton_Click(object sender, EventArgs e)
        {
            var model = modelLocation.Text;
            if (string.IsNullOrEmpty(model))
            {
                MessageBox.Show("A model location is required.");
                return;
            }
            var prompt = promptText.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                MessageBox.Show("You must provide a prompt.");
                return;
            }
            var input = inputJson.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("You must provide input.");
                return;
            }

            var skill = availableSkills.SelectedItem;
            if (skill == null)
            {
                MessageBox.Show("You must select a skill.");
                return;
            }

            var generator = availableGenerators.SelectedItem;
            if (generator == null)
            {
                MessageBox.Show("You must select a generator for this model.");
                return;
            }

            if (!m_SkManager.IsPythonInitialized())
            {
                ToggleUi(true, SkManager.s_InitializeSteps);
                try
                {
                    await m_SkManager.InitializePython(m_Settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    ToggleUi(false);
                    return;
                }
            }

            ToggleUi(true, SkManager.s_RunSkillSteps);
            try
            {
                var result = await m_SkManager.RunSkill(
                    (string)skill, model, (string)generator, prompt, input);
                llmResponse.Clear();
                llmResponse.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            ToggleUi(false);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (availableGenerators.SelectedItem == null)
            {
                return;
            }

            //
            // Depending on the model, a file or folder might be required.
            //
            if ((string)availableGenerators.SelectedItem == "dolly")
            {
                var dialog = new FolderBrowserDialog();

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var modelFile = dialog.SelectedPath;
                modelLocation.Text = modelFile;
            }
            else if ((string)availableGenerators.SelectedItem == "gpt4all-alpaca-13b-q4")
            {
                var dialog = new OpenFileDialog();

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var modelFile = dialog.FileName;
                modelLocation.Text = modelFile;
            }
            else
            {
                MessageBox.Show("Support has not been added for that generator.");
                return;
            }
        }

        private void availableSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = availableSkills.SelectedItem;
            if (item == null)
            {
                return;
            }
            else if ((string)item == "Custom")
            {
                //
                // The user will define the prompt and input.
                //
                promptText.Enabled = true;
                return;
            }

            //
            // Pre-defined semantic skills have readonly prompt text
            //
            promptText.Enabled = false;
            promptText.Text = m_SkManager.GetPromptTemplate((string)item);

            //
            // Supply editable skill input.
            //
            inputJson.Text = m_SkManager.GetDefaultInput((string)item);
        }

        #endregion

        private void ToggleUi(bool Initialize, int Steps = 0)
        {
            ToggleMenu(!Initialize);
            ToggleStatusStrip(Initialize, Steps);
        }

        private void ToggleMenu(bool Enable)
        {
            rpcServerGroupbox.Enabled = Enable;
            workbenchTabControl.Enabled = Enable;
        }

        private void ToggleStatusStrip(bool Initialize, int Steps)
        {
            if (Initialize)
            {
                toolStripProgressBar1.Enabled = true;
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Step = 1;
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = Steps;
                return;
            }
            toolStripProgressBar1.Enabled = false;
            toolStripProgressBar1.Visible = false;
            toolStripProgressBar1.Step = 0;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = 0;
            toolStripStatusLabel1.Text = "";
        }

        private
        void
        UpdateStatus(string Message)
        {
            //
            // Thread context can be arbitrary. Invoke on main UI thread.
            //
            statusStrip1.Invoke((MethodInvoker)(() =>
            {
                toolStripStatusLabel1.Visible = true;
                toolStripStatusLabel1.Enabled = true;
                toolStripStatusLabel1.Text = Message;
            }));
        }

        private
        void
        ProgressUpdate()
        {
            //
            // Thread context can be arbitrary. Invoke on main UI thread.
            //
            statusStrip1.Invoke((MethodInvoker)(() =>
            {
                toolStripProgressBar1.PerformStep();
            }));
        }

        private void availableGenerators_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (availableGenerators.SelectedItem == null ||
                string.IsNullOrEmpty((string)availableGenerators.SelectedItem))
            {
                modelLocation.Enabled = false;
                browseButton.Enabled = false;
                return;
            }
            modelLocation.Enabled = true;
            browseButton.Enabled = true;
        }
    }
}
