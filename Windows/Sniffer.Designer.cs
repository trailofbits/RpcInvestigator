namespace RpcInvestigator.Windows
{
    partial class Sniffer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sniffer));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.startButton = new System.Windows.Forms.ToolStripButton();
            this.enableDebugEventsButton = new System.Windows.Forms.ToolStripButton();
            this.chooseColumnsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.groupByActivityButton = new System.Windows.Forms.ToolStripButton();
            this.nodeViewButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startButton,
            this.enableDebugEventsButton,
            this.chooseColumnsButton,
            this.toolStripSeparator1,
            this.groupByActivityButton,
            this.nodeViewButton,
            this.toolStripSeparator2,
            this.saveButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(884, 76);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // startButton
            // 
            this.startButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.startButton.Image = global::RpcInvestigator.Properties.Resources.play;
            this.startButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.startButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(36, 73);
            this.startButton.ToolTipText = "Start or stop the trace";
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // enableDebugEventsButton
            // 
            this.enableDebugEventsButton.CheckOnClick = true;
            this.enableDebugEventsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.enableDebugEventsButton.Image = global::RpcInvestigator.Properties.Resources.gears;
            this.enableDebugEventsButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.enableDebugEventsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.enableDebugEventsButton.Name = "enableDebugEventsButton";
            this.enableDebugEventsButton.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.enableDebugEventsButton.Size = new System.Drawing.Size(51, 73);
            this.enableDebugEventsButton.ToolTipText = "Enable debug events (requires restart)";
            this.enableDebugEventsButton.Click += new System.EventHandler(this.enableDebugEventsButton_Click);
            // 
            // chooseColumnsButton
            // 
            this.chooseColumnsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.chooseColumnsButton.Image = global::RpcInvestigator.Properties.Resources.columns;
            this.chooseColumnsButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.chooseColumnsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chooseColumnsButton.Name = "chooseColumnsButton";
            this.chooseColumnsButton.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.chooseColumnsButton.Size = new System.Drawing.Size(51, 73);
            this.chooseColumnsButton.ToolTipText = "Choose columns...";
            this.chooseColumnsButton.Click += new System.EventHandler(this.chooseColumnsButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 76);
            // 
            // groupByActivityButton
            // 
            this.groupByActivityButton.CheckOnClick = true;
            this.groupByActivityButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.groupByActivityButton.Image = ((System.Drawing.Image)(resources.GetObject("groupByActivityButton.Image")));
            this.groupByActivityButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.groupByActivityButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.groupByActivityButton.Name = "groupByActivityButton";
            this.groupByActivityButton.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.groupByActivityButton.Size = new System.Drawing.Size(51, 73);
            this.groupByActivityButton.ToolTipText = "Group by Activity ID";
            this.groupByActivityButton.Click += new System.EventHandler(this.groupByActivityButton_Click);
            // 
            // nodeViewButton
            // 
            this.nodeViewButton.CheckOnClick = true;
            this.nodeViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nodeViewButton.Image = global::RpcInvestigator.Properties.Resources.nodes;
            this.nodeViewButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.nodeViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nodeViewButton.Name = "nodeViewButton";
            this.nodeViewButton.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.nodeViewButton.Size = new System.Drawing.Size(51, 73);
            this.nodeViewButton.ToolTipText = "Switch to node view";
            this.nodeViewButton.Click += new System.EventHandler(this.nodeViewButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 76);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveButton.Image = global::RpcInvestigator.Properties.Resources.save;
            this.saveButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.saveButton.Size = new System.Drawing.Size(51, 73);
            this.saveButton.ToolTipText = "Save the current view";
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.statusStrip1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(884, 761);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 722);
            this.statusStrip1.MaximumSize = new System.Drawing.Size(0, 35);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 35);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 27);
            this.toolStripProgressBar1.Visible = false;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(5, 29);
            // 
            // Sniffer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 761);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Sniffer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RPC Sniffer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Sniffer_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton startButton;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripButton enableDebugEventsButton;
        private System.Windows.Forms.ToolStripButton chooseColumnsButton;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripButton nodeViewButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton groupByActivityButton;
    }
}