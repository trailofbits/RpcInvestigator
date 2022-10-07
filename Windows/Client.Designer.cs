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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.leftTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.outputGroupbox = new System.Windows.Forms.GroupBox();
            this.outputRichTextBox = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rpcServerGroupbox = new System.Windows.Forms.GroupBox();
            this.versionValue = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.connectButton = new System.Windows.Forms.Button();
            this.protocolValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.endpointValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.uuidValue = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rpcServerStartLabel = new System.Windows.Forms.Label();
            this.workbenchTabControl = new System.Windows.Forms.TabControl();
            this.codeTabPage = new System.Windows.Forms.TabPage();
            this.rightTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.resetButton = new System.Windows.Forms.Button();
            this.rpcClientSourceCode = new FastColoredTextBoxNS.FastColoredTextBox();
            this.runButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.leftTableLayoutPanel.SuspendLayout();
            this.outputGroupbox.SuspendLayout();
            this.rpcServerGroupbox.SuspendLayout();
            this.workbenchTabControl.SuspendLayout();
            this.codeTabPage.SuspendLayout();
            this.rightTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rpcClientSourceCode)).BeginInit();
            this.SuspendLayout();
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.leftTableLayoutPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.workbenchTabControl);
            this.mainSplitContainer.Size = new System.Drawing.Size(1792, 761);
            this.mainSplitContainer.SplitterDistance = 597;
            this.mainSplitContainer.TabIndex = 0;
            // 
            // leftTableLayoutPanel
            // 
            this.leftTableLayoutPanel.ColumnCount = 1;
            this.leftTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.leftTableLayoutPanel.Controls.Add(this.outputGroupbox, 0, 1);
            this.leftTableLayoutPanel.Controls.Add(this.rpcServerGroupbox, 0, 0);
            this.leftTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.leftTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.leftTableLayoutPanel.Name = "leftTableLayoutPanel";
            this.leftTableLayoutPanel.RowCount = 2;
            this.leftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36.18657F));
            this.leftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 63.81343F));
            this.leftTableLayoutPanel.Size = new System.Drawing.Size(597, 761);
            this.leftTableLayoutPanel.TabIndex = 2;
            // 
            // outputGroupbox
            // 
            this.outputGroupbox.Controls.Add(this.outputRichTextBox);
            this.outputGroupbox.Controls.Add(this.label2);
            this.outputGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputGroupbox.Location = new System.Drawing.Point(3, 278);
            this.outputGroupbox.Name = "outputGroupbox";
            this.outputGroupbox.Size = new System.Drawing.Size(591, 480);
            this.outputGroupbox.TabIndex = 3;
            this.outputGroupbox.TabStop = false;
            this.outputGroupbox.Text = "Output";
            // 
            // outputRichTextBox
            // 
            this.outputRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputRichTextBox.Location = new System.Drawing.Point(3, 18);
            this.outputRichTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.outputRichTextBox.Name = "outputRichTextBox";
            this.outputRichTextBox.Size = new System.Drawing.Size(585, 459);
            this.outputRichTextBox.TabIndex = 8;
            this.outputRichTextBox.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 16);
            this.label2.TabIndex = 7;
            // 
            // rpcServerGroupbox
            // 
            this.rpcServerGroupbox.Controls.Add(this.versionValue);
            this.rpcServerGroupbox.Controls.Add(this.label5);
            this.rpcServerGroupbox.Controls.Add(this.connectButton);
            this.rpcServerGroupbox.Controls.Add(this.protocolValue);
            this.rpcServerGroupbox.Controls.Add(this.label4);
            this.rpcServerGroupbox.Controls.Add(this.endpointValue);
            this.rpcServerGroupbox.Controls.Add(this.label3);
            this.rpcServerGroupbox.Controls.Add(this.uuidValue);
            this.rpcServerGroupbox.Controls.Add(this.label1);
            this.rpcServerGroupbox.Controls.Add(this.rpcServerStartLabel);
            this.rpcServerGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpcServerGroupbox.Location = new System.Drawing.Point(3, 3);
            this.rpcServerGroupbox.Name = "rpcServerGroupbox";
            this.rpcServerGroupbox.Size = new System.Drawing.Size(591, 269);
            this.rpcServerGroupbox.TabIndex = 2;
            this.rpcServerGroupbox.TabStop = false;
            this.rpcServerGroupbox.Text = "RPC Server";
            // 
            // versionValue
            // 
            this.versionValue.Location = new System.Drawing.Point(413, 25);
            this.versionValue.Name = "versionValue";
            this.versionValue.Size = new System.Drawing.Size(46, 22);
            this.versionValue.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(348, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 16);
            this.label5.TabIndex = 15;
            this.label5.Text = "Version:";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(384, 161);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 30);
            this.connectButton.TabIndex = 14;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // protocolValue
            // 
            this.protocolValue.Location = new System.Drawing.Point(85, 113);
            this.protocolValue.Name = "protocolValue";
            this.protocolValue.Size = new System.Drawing.Size(374, 22);
            this.protocolValue.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 16);
            this.label4.TabIndex = 12;
            this.label4.Text = "Protocol:";
            // 
            // endpointValue
            // 
            this.endpointValue.Location = new System.Drawing.Point(85, 66);
            this.endpointValue.Name = "endpointValue";
            this.endpointValue.Size = new System.Drawing.Size(374, 22);
            this.endpointValue.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "Endpoint:";
            // 
            // uuidValue
            // 
            this.uuidValue.Location = new System.Drawing.Point(85, 25);
            this.uuidValue.Name = "uuidValue";
            this.uuidValue.Size = new System.Drawing.Size(243, 22);
            this.uuidValue.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "UUID:";
            // 
            // rpcServerStartLabel
            // 
            this.rpcServerStartLabel.AutoSize = true;
            this.rpcServerStartLabel.Location = new System.Drawing.Point(7, 22);
            this.rpcServerStartLabel.Name = "rpcServerStartLabel";
            this.rpcServerStartLabel.Size = new System.Drawing.Size(0, 16);
            this.rpcServerStartLabel.TabIndex = 7;
            // 
            // workbenchTabControl
            // 
            this.workbenchTabControl.Controls.Add(this.codeTabPage);
            this.workbenchTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workbenchTabControl.Location = new System.Drawing.Point(0, 0);
            this.workbenchTabControl.Name = "workbenchTabControl";
            this.workbenchTabControl.SelectedIndex = 0;
            this.workbenchTabControl.Size = new System.Drawing.Size(1191, 761);
            this.workbenchTabControl.TabIndex = 0;
            // 
            // codeTabPage
            // 
            this.codeTabPage.Controls.Add(this.rightTableLayoutPanel);
            this.codeTabPage.Location = new System.Drawing.Point(4, 25);
            this.codeTabPage.Name = "codeTabPage";
            this.codeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.codeTabPage.Size = new System.Drawing.Size(1183, 732);
            this.codeTabPage.TabIndex = 0;
            this.codeTabPage.Text = "Client Code";
            this.codeTabPage.UseVisualStyleBackColor = true;
            // 
            // rightTableLayoutPanel
            // 
            this.rightTableLayoutPanel.ColumnCount = 3;
            this.rightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.rightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.rightTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.rightTableLayoutPanel.Controls.Add(this.resetButton, 0, 1);
            this.rightTableLayoutPanel.Controls.Add(this.rpcClientSourceCode, 0, 0);
            this.rightTableLayoutPanel.Controls.Add(this.runButton, 0, 1);
            this.rightTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.rightTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.rightTableLayoutPanel.Name = "rightTableLayoutPanel";
            this.rightTableLayoutPanel.RowCount = 2;
            this.rightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92.9267F));
            this.rightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.073298F));
            this.rightTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.rightTableLayoutPanel.Size = new System.Drawing.Size(1177, 726);
            this.rightTableLayoutPanel.TabIndex = 1;
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(123, 677);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(114, 33);
            this.resetButton.TabIndex = 8;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // rpcClientSourceCode
            // 
            this.rpcClientSourceCode.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.rpcClientSourceCode.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;]+);\r\n^\\s*(case|default)\\s*[^:]" +
    "*(?<range>:)\\s*(?<range>[^;]+);\r\n";
            this.rpcClientSourceCode.AutoScrollMinSize = new System.Drawing.Size(31, 18);
            this.rpcClientSourceCode.AutoSize = true;
            this.rpcClientSourceCode.BackBrush = null;
            this.rpcClientSourceCode.BracketsHighlightStrategy = FastColoredTextBoxNS.BracketsHighlightStrategy.Strategy2;
            this.rpcClientSourceCode.CharHeight = 18;
            this.rpcClientSourceCode.CharWidth = 10;
            this.rightTableLayoutPanel.SetColumnSpan(this.rpcClientSourceCode, 3);
            this.rpcClientSourceCode.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.rpcClientSourceCode.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.rpcClientSourceCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpcClientSourceCode.IsReplaceMode = false;
            this.rpcClientSourceCode.Language = FastColoredTextBoxNS.Language.CSharp;
            this.rpcClientSourceCode.LeftBracket = '(';
            this.rpcClientSourceCode.LeftBracket2 = '{';
            this.rpcClientSourceCode.Location = new System.Drawing.Point(3, 3);
            this.rpcClientSourceCode.Name = "rpcClientSourceCode";
            this.rpcClientSourceCode.Paddings = new System.Windows.Forms.Padding(0);
            this.rpcClientSourceCode.RightBracket = ')';
            this.rpcClientSourceCode.RightBracket2 = '}';
            this.rpcClientSourceCode.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.rpcClientSourceCode.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("rpcClientSourceCode.ServiceColors")));
            this.rpcClientSourceCode.Size = new System.Drawing.Size(1171, 668);
            this.rpcClientSourceCode.TabIndex = 1;
            this.rpcClientSourceCode.Zoom = 100;
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(3, 677);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(114, 33);
            this.runButton.TabIndex = 7;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1792, 761);
            this.Controls.Add(this.mainSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Client";
            this.Text = "RPC Client Workbench";
            this.Shown += new System.EventHandler(this.Client_Shown);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.leftTableLayoutPanel.ResumeLayout(false);
            this.outputGroupbox.ResumeLayout(false);
            this.outputGroupbox.PerformLayout();
            this.rpcServerGroupbox.ResumeLayout(false);
            this.rpcServerGroupbox.PerformLayout();
            this.workbenchTabControl.ResumeLayout(false);
            this.codeTabPage.ResumeLayout(false);
            this.rightTableLayoutPanel.ResumeLayout(false);
            this.rightTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rpcClientSourceCode)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.TableLayoutPanel leftTableLayoutPanel;
        private System.Windows.Forms.GroupBox rpcServerGroupbox;
        private System.Windows.Forms.Label rpcServerStartLabel;
        private System.Windows.Forms.GroupBox outputGroupbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox outputRichTextBox;
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
    }
}