namespace RpcInvestigator
{
    partial class Services
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Services));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.servicesListview = new BrightIdeasSoftware.FastObjectListView();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.goButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.servicesListview)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.servicesListview);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.selectAllButton);
            this.splitContainer1.Panel2.Controls.Add(this.goButton);
            this.splitContainer1.Size = new System.Drawing.Size(1371, 815);
            this.splitContainer1.SplitterDistance = 749;
            this.splitContainer1.TabIndex = 0;
            // 
            // servicesListview
            // 
            this.servicesListview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.servicesListview.GridLines = true;
            this.servicesListview.HideSelection = false;
            this.servicesListview.Location = new System.Drawing.Point(0, 0);
            this.servicesListview.Name = "servicesListview";
            this.servicesListview.ShowGroups = false;
            this.servicesListview.Size = new System.Drawing.Size(1371, 749);
            this.servicesListview.TabIndex = 0;
            this.servicesListview.UseCompatibleStateImageBehavior = false;
            this.servicesListview.View = System.Windows.Forms.View.Details;
            this.servicesListview.VirtualMode = true;
            // 
            // selectAllButton
            // 
            this.selectAllButton.Location = new System.Drawing.Point(496, 13);
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.Size = new System.Drawing.Size(154, 37);
            this.selectAllButton.TabIndex = 1;
            this.selectAllButton.Text = "Select All";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(675, 13);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(154, 37);
            this.goButton.TabIndex = 0;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // Services
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1371, 815);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Services";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Services";
            this.Shown += new System.EventHandler(this.Services_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.servicesListview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.FastObjectListView servicesListview;
        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.Button selectAllButton;
    }
}