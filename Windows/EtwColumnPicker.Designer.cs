namespace RpcInvestigator.Windows
{
    partial class EtwColumnPicker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EtwColumnPicker));
            this.columnsList = new System.Windows.Forms.ListBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // columnsList
            // 
            this.columnsList.FormattingEnabled = true;
            this.columnsList.ItemHeight = 16;
            this.columnsList.Location = new System.Drawing.Point(12, 38);
            this.columnsList.MultiColumn = true;
            this.columnsList.Name = "columnsList";
            this.columnsList.ScrollAlwaysVisible = true;
            this.columnsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.columnsList.Size = new System.Drawing.Size(405, 532);
            this.columnsList.Sorted = true;
            this.columnsList.TabIndex = 2;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(176, 591);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(80, 45);
            this.applyButton.TabIndex = 3;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // EtwColumnPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 648);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.columnsList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EtwColumnPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Columns";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox columnsList;
        private System.Windows.Forms.Button applyButton;
    }
}