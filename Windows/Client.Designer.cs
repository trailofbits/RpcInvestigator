namespace RpcInvestigator
{
    partial class Client
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
            mainSplitContainer = new System.Windows.Forms.SplitContainer();
            leftTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            outputGroupbox = new System.Windows.Forms.GroupBox();
            rpcClientOutput = new FastColoredTextBoxNS.FastColoredTextBox();
            rpcServerGroupbox = new System.Windows.Forms.GroupBox();
            versionValue = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            connectButton = new System.Windows.Forms.Button();
            protocolValue = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            endpointValue = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            uuidValue = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            rpcServerStartLabel = new System.Windows.Forms.Label();
            workbenchTabControl = new System.Windows.Forms.TabControl();
            codeTabPage = new System.Windows.Forms.TabPage();
            rightTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            resetButton = new System.Windows.Forms.Button();
            rpcClientSourceCode = new FastColoredTextBoxNS.FastColoredTextBox();
            runButton = new System.Windows.Forms.Button();
            copyClientCodeButton = new System.Windows.Forms.Button();
            llmTabPage = new System.Windows.Forms.TabPage();
            llmResponse = new FastColoredTextBoxNS.FastColoredTextBox();
            inputJson = new FastColoredTextBoxNS.FastColoredTextBox();
            promptText = new FastColoredTextBoxNS.FastColoredTextBox();
            availableGenerators = new System.Windows.Forms.ComboBox();
            label11 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            availableSkills = new System.Windows.Forms.ComboBox();
            label9 = new System.Windows.Forms.Label();
            modelLocation = new System.Windows.Forms.TextBox();
            copyResponseButton = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            submitButton = new System.Windows.Forms.Button();
            browseButton = new System.Windows.Forms.Button();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            leftTableLayoutPanel.SuspendLayout();
            outputGroupbox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rpcClientOutput).BeginInit();
            rpcServerGroupbox.SuspendLayout();
            workbenchTabControl.SuspendLayout();
            codeTabPage.SuspendLayout();
            rightTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rpcClientSourceCode).BeginInit();
            llmTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)llmResponse).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputJson).BeginInit();
            ((System.ComponentModel.ISupportInitialize)promptText).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(leftTableLayoutPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(workbenchTabControl);
            mainSplitContainer.Size = new System.Drawing.Size(1790, 977);
            mainSplitContainer.SplitterDistance = 595;
            mainSplitContainer.TabIndex = 0;
            // 
            // leftTableLayoutPanel
            // 
            leftTableLayoutPanel.ColumnCount = 1;
            leftTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            leftTableLayoutPanel.Controls.Add(outputGroupbox, 0, 1);
            leftTableLayoutPanel.Controls.Add(rpcServerGroupbox, 0, 0);
            leftTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            leftTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            leftTableLayoutPanel.Name = "leftTableLayoutPanel";
            leftTableLayoutPanel.RowCount = 2;
            leftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36.18657F));
            leftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 63.81343F));
            leftTableLayoutPanel.Size = new System.Drawing.Size(595, 977);
            leftTableLayoutPanel.TabIndex = 2;
            // 
            // outputGroupbox
            // 
            outputGroupbox.Controls.Add(rpcClientOutput);
            outputGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
            outputGroupbox.Location = new System.Drawing.Point(3, 356);
            outputGroupbox.Name = "outputGroupbox";
            outputGroupbox.Size = new System.Drawing.Size(589, 618);
            outputGroupbox.TabIndex = 3;
            outputGroupbox.TabStop = false;
            outputGroupbox.Text = "Output";
            // 
            // rpcClientOutput
            // 
            rpcClientOutput.AutoCompleteBracketsList = (new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' });
            rpcClientOutput.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            rpcClientOutput.AutoScrollMinSize = new System.Drawing.Size(0, 18);
            rpcClientOutput.BackBrush = null;
            rpcClientOutput.CharHeight = 18;
            rpcClientOutput.CharWidth = 10;
            rpcClientOutput.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rpcClientOutput.IsReplaceMode = false;
            rpcClientOutput.Location = new System.Drawing.Point(9, 26);
            rpcClientOutput.Name = "rpcClientOutput";
            rpcClientOutput.Paddings = new System.Windows.Forms.Padding(0);
            rpcClientOutput.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            rpcClientOutput.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("rpcClientOutput.ServiceColors");
            rpcClientOutput.ShowLineNumbers = false;
            rpcClientOutput.Size = new System.Drawing.Size(575, 553);
            rpcClientOutput.TabIndex = 31;
            rpcClientOutput.WordWrap = true;
            rpcClientOutput.Zoom = 100;
            // 
            // rpcServerGroupbox
            // 
            rpcServerGroupbox.Controls.Add(versionValue);
            rpcServerGroupbox.Controls.Add(label5);
            rpcServerGroupbox.Controls.Add(connectButton);
            rpcServerGroupbox.Controls.Add(protocolValue);
            rpcServerGroupbox.Controls.Add(label4);
            rpcServerGroupbox.Controls.Add(endpointValue);
            rpcServerGroupbox.Controls.Add(label3);
            rpcServerGroupbox.Controls.Add(uuidValue);
            rpcServerGroupbox.Controls.Add(label1);
            rpcServerGroupbox.Controls.Add(rpcServerStartLabel);
            rpcServerGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
            rpcServerGroupbox.Location = new System.Drawing.Point(3, 3);
            rpcServerGroupbox.Name = "rpcServerGroupbox";
            rpcServerGroupbox.Size = new System.Drawing.Size(589, 347);
            rpcServerGroupbox.TabIndex = 2;
            rpcServerGroupbox.TabStop = false;
            rpcServerGroupbox.Text = "RPC Server";
            // 
            // versionValue
            // 
            versionValue.Location = new System.Drawing.Point(413, 31);
            versionValue.Name = "versionValue";
            versionValue.Size = new System.Drawing.Size(47, 27);
            versionValue.TabIndex = 16;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(348, 35);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(60, 20);
            label5.TabIndex = 15;
            label5.Text = "Version:";
            // 
            // connectButton
            // 
            connectButton.Location = new System.Drawing.Point(384, 202);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(75, 37);
            connectButton.TabIndex = 14;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += connectButton_Click;
            // 
            // protocolValue
            // 
            protocolValue.Location = new System.Drawing.Point(85, 142);
            protocolValue.Name = "protocolValue";
            protocolValue.Size = new System.Drawing.Size(375, 27);
            protocolValue.TabIndex = 13;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(13, 145);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(68, 20);
            label4.TabIndex = 12;
            label4.Text = "Protocol:";
            // 
            // endpointValue
            // 
            endpointValue.Location = new System.Drawing.Point(85, 83);
            endpointValue.Name = "endpointValue";
            endpointValue.Size = new System.Drawing.Size(375, 27);
            endpointValue.TabIndex = 11;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 86);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(72, 20);
            label3.TabIndex = 10;
            label3.Text = "Endpoint:";
            // 
            // uuidValue
            // 
            uuidValue.Location = new System.Drawing.Point(85, 31);
            uuidValue.Name = "uuidValue";
            uuidValue.Size = new System.Drawing.Size(243, 27);
            uuidValue.TabIndex = 9;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 35);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(47, 20);
            label1.TabIndex = 8;
            label1.Text = "UUID:";
            // 
            // rpcServerStartLabel
            // 
            rpcServerStartLabel.AutoSize = true;
            rpcServerStartLabel.Location = new System.Drawing.Point(7, 28);
            rpcServerStartLabel.Name = "rpcServerStartLabel";
            rpcServerStartLabel.Size = new System.Drawing.Size(0, 20);
            rpcServerStartLabel.TabIndex = 7;
            // 
            // workbenchTabControl
            // 
            workbenchTabControl.Controls.Add(codeTabPage);
            workbenchTabControl.Controls.Add(llmTabPage);
            workbenchTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            workbenchTabControl.Location = new System.Drawing.Point(0, 0);
            workbenchTabControl.Name = "workbenchTabControl";
            workbenchTabControl.SelectedIndex = 0;
            workbenchTabControl.Size = new System.Drawing.Size(1191, 977);
            workbenchTabControl.TabIndex = 0;
            // 
            // codeTabPage
            // 
            codeTabPage.Controls.Add(rightTableLayoutPanel);
            codeTabPage.Location = new System.Drawing.Point(4, 29);
            codeTabPage.Name = "codeTabPage";
            codeTabPage.Padding = new System.Windows.Forms.Padding(3);
            codeTabPage.Size = new System.Drawing.Size(1183, 944);
            codeTabPage.TabIndex = 0;
            codeTabPage.Text = "Client Code";
            codeTabPage.UseVisualStyleBackColor = true;
            // 
            // rightTableLayoutPanel
            // 
            rightTableLayoutPanel.ColumnCount = 3;
            rightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            rightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            rightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            rightTableLayoutPanel.Controls.Add(resetButton, 0, 1);
            rightTableLayoutPanel.Controls.Add(rpcClientSourceCode, 0, 0);
            rightTableLayoutPanel.Controls.Add(runButton, 0, 1);
            rightTableLayoutPanel.Controls.Add(copyClientCodeButton, 2, 1);
            rightTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            rightTableLayoutPanel.Name = "rightTableLayoutPanel";
            rightTableLayoutPanel.RowCount = 2;
            rightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92.9267F));
            rightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.073298F));
            rightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            rightTableLayoutPanel.Size = new System.Drawing.Size(1177, 938);
            rightTableLayoutPanel.TabIndex = 1;
            // 
            // resetButton
            // 
            resetButton.Location = new System.Drawing.Point(124, 874);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(115, 42);
            resetButton.TabIndex = 8;
            resetButton.Text = "Reset";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += resetButton_Click;
            // 
            // rpcClientSourceCode
            // 
            rpcClientSourceCode.AutoCompleteBracketsList = (new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' });
            rpcClientSourceCode.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);\r\n";
            rpcClientSourceCode.AutoScrollMinSize = new System.Drawing.Size(31, 18);
            rpcClientSourceCode.AutoSize = true;
            rpcClientSourceCode.BackBrush = null;
            rpcClientSourceCode.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            rpcClientSourceCode.CharHeight = 18;
            rpcClientSourceCode.CharWidth = 10;
            rightTableLayoutPanel.SetColumnSpan(rpcClientSourceCode, 3);
            rpcClientSourceCode.Cursor = System.Windows.Forms.Cursors.IBeam;
            rpcClientSourceCode.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            rpcClientSourceCode.Dock = System.Windows.Forms.DockStyle.Fill;
            rpcClientSourceCode.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            rpcClientSourceCode.IsReplaceMode = false;
            rpcClientSourceCode.Language = FastColoredTextBoxNS.Language.CSharp;
            rpcClientSourceCode.LeftBracket = '(';
            rpcClientSourceCode.LeftBracket2 = '{';
            rpcClientSourceCode.Location = new System.Drawing.Point(3, 3);
            rpcClientSourceCode.Name = "rpcClientSourceCode";
            rpcClientSourceCode.Paddings = new System.Windows.Forms.Padding(0);
            rpcClientSourceCode.RightBracket = ')';
            rpcClientSourceCode.RightBracket2 = '}';
            rpcClientSourceCode.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            rpcClientSourceCode.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("rpcClientSourceCode.ServiceColors");
            rpcClientSourceCode.Size = new System.Drawing.Size(1171, 865);
            rpcClientSourceCode.TabIndex = 1;
            rpcClientSourceCode.Zoom = 100;
            // 
            // runButton
            // 
            runButton.Location = new System.Drawing.Point(3, 874);
            runButton.Name = "runButton";
            runButton.Size = new System.Drawing.Size(115, 42);
            runButton.TabIndex = 7;
            runButton.Text = "Run";
            runButton.UseVisualStyleBackColor = true;
            runButton.Click += runButton_Click;
            // 
            // copyClientCodeButton
            // 
            copyClientCodeButton.Location = new System.Drawing.Point(245, 874);
            copyClientCodeButton.Name = "copyClientCodeButton";
            copyClientCodeButton.Size = new System.Drawing.Size(111, 42);
            copyClientCodeButton.TabIndex = 9;
            copyClientCodeButton.Text = "Copy";
            copyClientCodeButton.UseVisualStyleBackColor = true;
            copyClientCodeButton.Click += copyClientCodeButton_Click;
            // 
            // llmTabPage
            // 
            llmTabPage.Controls.Add(llmResponse);
            llmTabPage.Controls.Add(inputJson);
            llmTabPage.Controls.Add(promptText);
            llmTabPage.Controls.Add(availableGenerators);
            llmTabPage.Controls.Add(label11);
            llmTabPage.Controls.Add(label10);
            llmTabPage.Controls.Add(availableSkills);
            llmTabPage.Controls.Add(label9);
            llmTabPage.Controls.Add(modelLocation);
            llmTabPage.Controls.Add(copyResponseButton);
            llmTabPage.Controls.Add(label2);
            llmTabPage.Controls.Add(submitButton);
            llmTabPage.Controls.Add(browseButton);
            llmTabPage.Controls.Add(label8);
            llmTabPage.Controls.Add(label7);
            llmTabPage.Location = new System.Drawing.Point(4, 29);
            llmTabPage.Name = "llmTabPage";
            llmTabPage.Size = new System.Drawing.Size(1183, 784);
            llmTabPage.TabIndex = 1;
            llmTabPage.Text = "LLM";
            llmTabPage.UseVisualStyleBackColor = true;
            // 
            // llmResponse
            // 
            llmResponse.AutoCompleteBracketsList = (new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' });
            llmResponse.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            llmResponse.AutoScrollMinSize = new System.Drawing.Size(0, 18);
            llmResponse.BackBrush = null;
            llmResponse.CharHeight = 18;
            llmResponse.CharWidth = 10;
            llmResponse.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            llmResponse.IsReplaceMode = false;
            llmResponse.Location = new System.Drawing.Point(578, 129);
            llmResponse.Name = "llmResponse";
            llmResponse.Paddings = new System.Windows.Forms.Padding(0);
            llmResponse.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            llmResponse.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("llmResponse.ServiceColors");
            llmResponse.ShowLineNumbers = false;
            llmResponse.Size = new System.Drawing.Size(598, 768);
            llmResponse.TabIndex = 32;
            llmResponse.WordWrap = true;
            llmResponse.Zoom = 100;
            // 
            // inputJson
            // 
            inputJson.AutoCompleteBracketsList = (new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' });
            inputJson.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            inputJson.AutoScrollMinSize = new System.Drawing.Size(0, 18);
            inputJson.BackBrush = null;
            inputJson.CharHeight = 18;
            inputJson.CharWidth = 10;
            inputJson.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            inputJson.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            inputJson.IsReplaceMode = false;
            inputJson.Location = new System.Drawing.Point(13, 367);
            inputJson.Name = "inputJson";
            inputJson.Paddings = new System.Windows.Forms.Padding(0);
            inputJson.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            inputJson.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("inputJson.ServiceColors");
            inputJson.ShowLineNumbers = false;
            inputJson.Size = new System.Drawing.Size(559, 530);
            inputJson.TabIndex = 31;
            inputJson.Text = "{}";
            inputJson.WordWrap = true;
            inputJson.Zoom = 100;
            // 
            // promptText
            // 
            promptText.AutoCompleteBracketsList = (new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' });
            promptText.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*(?<range>:)\\s*(?<range>[^;]+);";
            promptText.AutoScrollMinSize = new System.Drawing.Size(0, 18);
            promptText.BackBrush = null;
            promptText.CharHeight = 18;
            promptText.CharWidth = 10;
            promptText.DisabledColor = System.Drawing.Color.FromArgb(100, 180, 180, 180);
            promptText.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            promptText.IsReplaceMode = false;
            promptText.Location = new System.Drawing.Point(13, 126);
            promptText.Name = "promptText";
            promptText.Paddings = new System.Windows.Forms.Padding(0);
            promptText.SelectionColor = System.Drawing.Color.FromArgb(60, 0, 0, 255);
            promptText.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("promptText.ServiceColors");
            promptText.ShowLineNumbers = false;
            promptText.Size = new System.Drawing.Size(559, 215);
            promptText.TabIndex = 30;
            promptText.WordWrap = true;
            promptText.Zoom = 100;
            // 
            // availableGenerators
            // 
            availableGenerators.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            availableGenerators.FormattingEnabled = true;
            availableGenerators.Items.AddRange(new object[] { "dolly", "gpt4all-alpaca-13b-q4", "stable-lm-alpaca" });
            availableGenerators.Location = new System.Drawing.Point(143, 9);
            availableGenerators.Name = "availableGenerators";
            availableGenerators.Size = new System.Drawing.Size(227, 28);
            availableGenerators.TabIndex = 29;
            availableGenerators.SelectedIndexChanged += availableGenerators_SelectedIndexChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(13, 12);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(124, 20);
            label11.TabIndex = 28;
            label11.Text = "Model generator:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(3, 344);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(95, 20);
            label10.TabIndex = 26;
            label10.Text = "Input (JSON):";
            // 
            // availableSkills
            // 
            availableSkills.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            availableSkills.FormattingEnabled = true;
            availableSkills.Location = new System.Drawing.Point(143, 52);
            availableSkills.Name = "availableSkills";
            availableSkills.Size = new System.Drawing.Size(227, 28);
            availableSkills.TabIndex = 25;
            availableSkills.SelectedIndexChanged += availableSkills_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(13, 55);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(126, 20);
            label9.TabIndex = 24;
            label9.Text = "Skill and function:";
            // 
            // modelLocation
            // 
            modelLocation.Enabled = false;
            modelLocation.Location = new System.Drawing.Point(474, 9);
            modelLocation.Name = "modelLocation";
            modelLocation.Size = new System.Drawing.Size(580, 27);
            modelLocation.TabIndex = 14;
            // 
            // copyResponseButton
            // 
            copyResponseButton.Location = new System.Drawing.Point(1117, 92);
            copyResponseButton.Name = "copyResponseButton";
            copyResponseButton.Size = new System.Drawing.Size(59, 31);
            copyResponseButton.TabIndex = 23;
            copyResponseButton.Text = "Copy";
            copyResponseButton.UseVisualStyleBackColor = true;
            copyResponseButton.Click += copyResponseButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(413, 12);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(55, 20);
            label2.TabIndex = 15;
            label2.Text = "Model:";
            // 
            // submitButton
            // 
            submitButton.Location = new System.Drawing.Point(387, 51);
            submitButton.Name = "submitButton";
            submitButton.Size = new System.Drawing.Size(91, 29);
            submitButton.TabIndex = 16;
            submitButton.Text = "Submit";
            submitButton.UseVisualStyleBackColor = true;
            submitButton.Click += submitButton_Click;
            // 
            // browseButton
            // 
            browseButton.Enabled = false;
            browseButton.Location = new System.Drawing.Point(1060, 9);
            browseButton.Name = "browseButton";
            browseButton.Size = new System.Drawing.Size(91, 29);
            browseButton.TabIndex = 17;
            browseButton.Text = "Browse";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(578, 97);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(75, 20);
            label8.TabIndex = 22;
            label8.Text = "Response:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(3, 97);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(125, 20);
            label7.TabIndex = 19;
            label7.Text = "Prompt template:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(10, 21);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(57, 16);
            label6.TabIndex = 1;
            label6.Text = "API Key:";
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripProgressBar1, toolStripStatusLabel1 });
            statusStrip1.Location = new System.Drawing.Point(0, 955);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1790, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new System.Drawing.Size(100, 14);
            toolStripProgressBar1.Visible = false;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(0, 16);
            toolStripStatusLabel1.Visible = false;
            // 
            // Client
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1790, 977);
            Controls.Add(statusStrip1);
            Controls.Add(mainSplitContainer);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Client";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "RPC Client Workbench";
            Shown += Client_Shown;
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            leftTableLayoutPanel.ResumeLayout(false);
            outputGroupbox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rpcClientOutput).EndInit();
            rpcServerGroupbox.ResumeLayout(false);
            rpcServerGroupbox.PerformLayout();
            workbenchTabControl.ResumeLayout(false);
            codeTabPage.ResumeLayout(false);
            rightTableLayoutPanel.ResumeLayout(false);
            rightTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)rpcClientSourceCode).EndInit();
            llmTabPage.ResumeLayout(false);
            llmTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)llmResponse).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputJson).EndInit();
            ((System.ComponentModel.ISupportInitialize)promptText).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.TableLayoutPanel leftTableLayoutPanel;
        private System.Windows.Forms.GroupBox rpcServerGroupbox;
        private System.Windows.Forms.Label rpcServerStartLabel;
        private System.Windows.Forms.GroupBox outputGroupbox;
        private System.Windows.Forms.TabControl workbenchTabControl;
        private System.Windows.Forms.TabPage codeTabPage;
        private System.Windows.Forms.TableLayoutPanel rightTableLayoutPanel;
        private System.Windows.Forms.Button resetButton;
        private FastColoredTextBoxNS.FastColoredTextBox rpcClientSourceCode;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox protocolValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox endpointValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox uuidValue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox versionValue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button copyClientCodeButton;
        private System.Windows.Forms.TabPage llmTabPage;
        private System.Windows.Forms.ComboBox availableSkills;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox modelLocation;
        private System.Windows.Forms.Button copyResponseButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ComboBox availableGenerators;
        private System.Windows.Forms.Label label11;
        private FastColoredTextBoxNS.FastColoredTextBox promptText;
        private FastColoredTextBoxNS.FastColoredTextBox inputJson;
        private FastColoredTextBoxNS.FastColoredTextBox rpcClientOutput;
        private FastColoredTextBoxNS.FastColoredTextBox llmResponse;
    }
}