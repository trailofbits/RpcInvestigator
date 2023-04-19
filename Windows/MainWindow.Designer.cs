namespace RpcInvestigator
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            allRpcAlpcServersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadFromBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadFromServiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            libraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            proceduresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            eraseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rPCSnifferToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mainTabControl = new System.Windows.Forms.TabControl();
            imageList1 = new System.Windows.Forms.ImageList(components);
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            progressBar = new System.Windows.Forms.ToolStripProgressBar();
            statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            imageList2 = new System.Windows.Forms.ImageList(components);
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, libraryToolStripMenuItem, toolsToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1084, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { allRpcAlpcServersToolStripMenuItem, loadFromBinaryToolStripMenuItem, loadFromServiceToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // allRpcAlpcServersToolStripMenuItem
            // 
            allRpcAlpcServersToolStripMenuItem.Name = "allRpcAlpcServersToolStripMenuItem";
            allRpcAlpcServersToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            allRpcAlpcServersToolStripMenuItem.Text = "All RPC ALPC servers...";
            allRpcAlpcServersToolStripMenuItem.Click += loadAllRPCALPCServersToolStripMenuItem_Click;
            // 
            // loadFromBinaryToolStripMenuItem
            // 
            loadFromBinaryToolStripMenuItem.Name = "loadFromBinaryToolStripMenuItem";
            loadFromBinaryToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            loadFromBinaryToolStripMenuItem.Text = "Load from binary...";
            loadFromBinaryToolStripMenuItem.Click += loadFromBinaryToolStripMenuItem_Click;
            // 
            // loadFromServiceToolStripMenuItem
            // 
            loadFromServiceToolStripMenuItem.Name = "loadFromServiceToolStripMenuItem";
            loadFromServiceToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            loadFromServiceToolStripMenuItem.Text = "Load from service...";
            loadFromServiceToolStripMenuItem.Click += loadFromServiceToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(233, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(236, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { settingsToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            editToolStripMenuItem.Text = "Edit";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new System.Drawing.Size(154, 26);
            settingsToolStripMenuItem.Text = "Settings...";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { logsToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            viewToolStripMenuItem.Text = "View";
            // 
            // logsToolStripMenuItem
            // 
            logsToolStripMenuItem.Name = "logsToolStripMenuItem";
            logsToolStripMenuItem.Size = new System.Drawing.Size(132, 26);
            logsToolStripMenuItem.Text = "Logs...";
            logsToolStripMenuItem.Click += logsToolStripMenuItem_Click;
            // 
            // libraryToolStripMenuItem
            // 
            libraryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { searchToolStripMenuItem, proceduresToolStripMenuItem, toolStripSeparator1, refreshToolStripMenuItem, exportAsTextToolStripMenuItem, eraseToolStripMenuItem });
            libraryToolStripMenuItem.Name = "libraryToolStripMenuItem";
            libraryToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            libraryToolStripMenuItem.Text = "Library";
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            searchToolStripMenuItem.Text = "Servers";
            searchToolStripMenuItem.Click += libraryServersToolStripMenuItem_Click;
            // 
            // proceduresToolStripMenuItem
            // 
            proceduresToolStripMenuItem.Name = "proceduresToolStripMenuItem";
            proceduresToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            proceduresToolStripMenuItem.Text = "Procedures";
            proceduresToolStripMenuItem.Click += libraryProceduresToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            refreshToolStripMenuItem.Text = "Refresh";
            refreshToolStripMenuItem.Click += refreshLibraryToolStripMenuItem_Click;
            // 
            // exportAsTextToolStripMenuItem
            // 
            exportAsTextToolStripMenuItem.Name = "exportAsTextToolStripMenuItem";
            exportAsTextToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            exportAsTextToolStripMenuItem.Text = "Export as Text";
            exportAsTextToolStripMenuItem.Click += exportLibraryAsTextToolStripMenuItem_Click;
            // 
            // eraseToolStripMenuItem
            // 
            eraseToolStripMenuItem.Name = "eraseToolStripMenuItem";
            eraseToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            eraseToolStripMenuItem.Text = "Erase";
            eraseToolStripMenuItem.Click += eraseToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { rPCSnifferToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // rPCSnifferToolStripMenuItem
            // 
            rPCSnifferToolStripMenuItem.Name = "rPCSnifferToolStripMenuItem";
            rPCSnifferToolStripMenuItem.Size = new System.Drawing.Size(172, 26);
            rPCSnifferToolStripMenuItem.Text = "RPC sniffer...";
            rPCSnifferToolStripMenuItem.Click += rPCSnifferToolStripMenuItem_Click;
            // 
            // mainTabControl
            // 
            mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            mainTabControl.ImageList = imageList1;
            mainTabControl.Location = new System.Drawing.Point(3, 61);
            mainTabControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.Padding = new System.Drawing.Point(9, 3);
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new System.Drawing.Size(1078, 838);
            mainTabControl.TabIndex = 0;
            mainTabControl.DrawItem += mainTabControl_DrawItem;
            mainTabControl.MouseDown += mainTabControl_MouseDown;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "service.png");
            imageList1.Images.SetKeyName(1, "endpoint.jpg");
            imageList1.Images.SetKeyName(2, "binary.png");
            imageList1.Images.SetKeyName(3, "search.jpg");
            imageList1.Images.SetKeyName(4, "greendot.png");
            imageList1.Images.SetKeyName(5, "reddot.png");
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { progressBar, statusLabel });
            statusStrip1.Location = new System.Drawing.Point(0, 903);
            statusStrip1.MaximumSize = new System.Drawing.Size(0, 44);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(22, 22);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(100, 34);
            progressBar.Visible = false;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            statusLabel.Size = new System.Drawing.Size(5, 16);
            // 
            // imageList2
            // 
            imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList2.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList2.ImageStream");
            imageList2.TransparentColor = System.Drawing.Color.Transparent;
            imageList2.Images.SetKeyName(0, "x-icon.png");
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(menuStrip1, 0, 0);
            tableLayoutPanel1.Controls.Add(mainTabControl, 0, 1);
            tableLayoutPanel1.Controls.Add(statusStrip1, 0, 2);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            tableLayoutPanel1.Size = new System.Drawing.Size(1084, 951);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1084, 951);
            Controls.Add(tableLayoutPanel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "MainWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "RPC Investigator";
            FormClosing += MainWindow_FormClosing;
            Shown += MainWindow_Shown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.ToolStripMenuItem loadFromBinaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFromServiceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem allRpcAlpcServersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logsToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem libraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAsTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem proceduresToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rPCSnifferToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripMenuItem eraseToolStripMenuItem;
    }
}

