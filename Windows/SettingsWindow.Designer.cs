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
            label1 = new System.Windows.Forms.Label();
            dbghelpPath = new System.Windows.Forms.TextBox();
            browseButton1 = new System.Windows.Forms.Button();
            symbolPath = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            saveButton = new System.Windows.Forms.Button();
            traceLevelComboBox = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            pythonVenv = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            browseButton2 = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            pythonDllLocation = new System.Windows.Forms.ListBox();
            displayLibraryOnStart = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            useGPU = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(16, 32);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(90, 20);
            label1.TabIndex = 0;
            label1.Text = "Dbghelp.dll:";
            // 
            // dbghelpPath
            // 
            dbghelpPath.Location = new System.Drawing.Point(111, 29);
            dbghelpPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            dbghelpPath.Name = "dbghelpPath";
            dbghelpPath.Size = new System.Drawing.Size(386, 27);
            dbghelpPath.TabIndex = 1;
            // 
            // browseButton1
            // 
            browseButton1.Location = new System.Drawing.Point(503, 27);
            browseButton1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            browseButton1.Name = "browseButton1";
            browseButton1.Size = new System.Drawing.Size(75, 29);
            browseButton1.TabIndex = 2;
            browseButton1.Text = "Browse...";
            browseButton1.UseVisualStyleBackColor = true;
            browseButton1.Click += browseButton1_Click;
            // 
            // symbolPath
            // 
            symbolPath.Location = new System.Drawing.Point(111, 89);
            symbolPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            symbolPath.Name = "symbolPath";
            symbolPath.Size = new System.Drawing.Size(386, 27);
            symbolPath.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(16, 92);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(94, 20);
            label2.TabIndex = 3;
            label2.Text = "Symbol Path:";
            // 
            // saveButton
            // 
            saveButton.Location = new System.Drawing.Point(260, 648);
            saveButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(93, 68);
            saveButton.TabIndex = 5;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // traceLevelComboBox
            // 
            traceLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            traceLevelComboBox.FormattingEnabled = true;
            traceLevelComboBox.Items.AddRange(new object[] { "Off", "Error", "Warning", "Information", "Verbose" });
            traceLevelComboBox.Location = new System.Drawing.Point(111, 145);
            traceLevelComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            traceLevelComboBox.Name = "traceLevelComboBox";
            traceLevelComboBox.Size = new System.Drawing.Size(180, 28);
            traceLevelComboBox.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(16, 149);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(85, 20);
            label3.TabIndex = 7;
            label3.Text = "Trace Level:";
            // 
            // pythonVenv
            // 
            pythonVenv.Location = new System.Drawing.Point(111, 206);
            pythonVenv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pythonVenv.Name = "pythonVenv";
            pythonVenv.Size = new System.Drawing.Size(386, 27);
            pythonVenv.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(16, 209);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(91, 20);
            label4.TabIndex = 8;
            label4.Text = "Python venv:";
            // 
            // browseButton2
            // 
            browseButton2.Location = new System.Drawing.Point(503, 206);
            browseButton2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            browseButton2.Name = "browseButton2";
            browseButton2.Size = new System.Drawing.Size(75, 29);
            browseButton2.TabIndex = 10;
            browseButton2.Text = "Browse...";
            browseButton2.UseVisualStyleBackColor = true;
            browseButton2.Click += browseButton2_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(16, 32);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(86, 20);
            label5.TabIndex = 11;
            label5.Text = "Python DLL:";
            // 
            // pythonDllLocation
            // 
            pythonDllLocation.FormattingEnabled = true;
            pythonDllLocation.ItemHeight = 20;
            pythonDllLocation.Location = new System.Drawing.Point(111, 32);
            pythonDllLocation.Name = "pythonDllLocation";
            pythonDllLocation.Size = new System.Drawing.Size(467, 104);
            pythonDllLocation.TabIndex = 12;
            // 
            // displayLibraryOnStart
            // 
            displayLibraryOnStart.AutoSize = true;
            displayLibraryOnStart.Location = new System.Drawing.Point(111, 252);
            displayLibraryOnStart.Name = "displayLibraryOnStart";
            displayLibraryOnStart.Size = new System.Drawing.Size(240, 24);
            displayLibraryOnStart.TabIndex = 13;
            displayLibraryOnStart.Text = "Display library contents on start";
            displayLibraryOnStart.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(useGPU);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(pythonDllLocation);
            groupBox1.Location = new System.Drawing.Point(12, 317);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(589, 299);
            groupBox1.TabIndex = 14;
            groupBox1.TabStop = false;
            groupBox1.Text = "LLM";
            // 
            // useGPU
            // 
            useGPU.AutoSize = true;
            useGPU.Location = new System.Drawing.Point(111, 142);
            useGPU.Name = "useGPU";
            useGPU.Size = new System.Drawing.Size(261, 24);
            useGPU.TabIndex = 14;
            useGPU.Text = "Use GPU for inference (CUDA only)";
            useGPU.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(dbghelpPath);
            groupBox2.Controls.Add(displayLibraryOnStart);
            groupBox2.Controls.Add(browseButton1);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(symbolPath);
            groupBox2.Controls.Add(browseButton2);
            groupBox2.Controls.Add(traceLevelComboBox);
            groupBox2.Controls.Add(pythonVenv);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label4);
            groupBox2.Location = new System.Drawing.Point(12, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(589, 299);
            groupBox2.TabIndex = 15;
            groupBox2.TabStop = false;
            groupBox2.Text = "General";
            // 
            // SettingsWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(613, 730);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(saveButton);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "SettingsWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.TextBox pythonVenv;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button browseButton2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox pythonDllLocation;
        private System.Windows.Forms.CheckBox displayLibraryOnStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox useGPU;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}