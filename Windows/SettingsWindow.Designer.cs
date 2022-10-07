namespace RpcInvestigator
{
    partial class SettingsWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.dbghelpPath = new System.Windows.Forms.TextBox();
            this.browseButton1 = new System.Windows.Forms.Button();
            this.symbolPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.traceLevelComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Dbghelp.dll:";
            // 
            // dbghelpPath
            // 
            this.dbghelpPath.Location = new System.Drawing.Point(107, 22);
            this.dbghelpPath.Name = "dbghelpPath";
            this.dbghelpPath.Size = new System.Drawing.Size(386, 22);
            this.dbghelpPath.TabIndex = 1;
            // 
            // browseButton1
            // 
            this.browseButton1.Location = new System.Drawing.Point(499, 21);
            this.browseButton1.Name = "browseButton1";
            this.browseButton1.Size = new System.Drawing.Size(75, 23);
            this.browseButton1.TabIndex = 2;
            this.browseButton1.Text = "Browse...";
            this.browseButton1.UseVisualStyleBackColor = true;
            this.browseButton1.Click += new System.EventHandler(this.browseButton1_Click);
            // 
            // symbolPath
            // 
            this.symbolPath.Location = new System.Drawing.Point(107, 70);
            this.symbolPath.Name = "symbolPath";
            this.symbolPath.Size = new System.Drawing.Size(386, 22);
            this.symbolPath.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Symbol Path:";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(260, 518);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(93, 54);
            this.saveButton.TabIndex = 5;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // traceLevelComboBox
            // 
            this.traceLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.traceLevelComboBox.FormattingEnabled = true;
            this.traceLevelComboBox.Items.AddRange(new object[] {
            "Off",
            "Error",
            "Info",
            "Warning",
            "Verbose"});
            this.traceLevelComboBox.Location = new System.Drawing.Point(107, 115);
            this.traceLevelComboBox.Name = "traceLevelComboBox";
            this.traceLevelComboBox.Size = new System.Drawing.Size(180, 24);
            this.traceLevelComboBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 16);
            this.label3.TabIndex = 7;
            this.label3.Text = "Trace Level:";
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 584);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.traceLevelComboBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.symbolPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.browseButton1);
            this.Controls.Add(this.dbghelpPath);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsWindow";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox dbghelpPath;
        private System.Windows.Forms.Button browseButton1;
        private System.Windows.Forms.TextBox symbolPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.ComboBox traceLevelComboBox;
        private System.Windows.Forms.Label label3;
    }
}