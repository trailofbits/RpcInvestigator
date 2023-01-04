using BrightIdeasSoftware;
using RpcInvestigator.Util;
using System;
using System.Security.AccessControl;
using System.Windows.Forms;
using NtApiDotNet;

namespace RpcInvestigator.Windows
{
    public partial class SecurityDescriptorView : Form
    {
        private RichTextBox richTextBox1;
        private CheckedListBox checkedListBox1;
        private FastObjectListView fastObjectListView1;
        private OLVColumn olvColumn1;
        private OLVColumn olvColumn2;
        private OLVColumn olvColumn3;
        private OLVColumn olvColumn4;
        private Button button1;

        public SecurityDescriptorView(

        )
        {
            InitializeComponent();
        }

        public void AddRow(
            Ace ace
        )
        {
            AceView aceView = new AceView();
            aceView.Flags = ace.Flags;
            aceView.Type = ace.Type;
            aceView.Mask = ace.Mask;
            aceView.Sid = ace.Sid.ToString() + " (" + ace.Sid.Name + ")";

            fastObjectListView1.AddObject(aceView);
        }

        public void AddOwner(
            string Owner    
        )
        {
            this.richTextBox1.Text += "Owner: " + Owner + "\n";
        }
        public void AddGroup(
            string Group
        )
        {
            this.richTextBox1.Text += "Group: " + Group + "\n";
        }

        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.fastObjectListView1 = new BrightIdeasSoftware.FastObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(543, 262);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(71, 26);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 158);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(304, 100);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "Connect",
            "Delete",
            "Read Control",
            "Write DAC",
            "Write Owner",
            "Synchronize"});
            this.checkedListBox1.Location = new System.Drawing.Point(322, 158);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.checkedListBox1.Size = new System.Drawing.Size(292, 89);
            this.checkedListBox1.TabIndex = 3;
            // 
            // fastObjectListView1
            // 
            this.fastObjectListView1.AllColumns.Add(this.olvColumn1);
            this.fastObjectListView1.AllColumns.Add(this.olvColumn2);
            this.fastObjectListView1.AllColumns.Add(this.olvColumn3);
            this.fastObjectListView1.AllColumns.Add(this.olvColumn4);
            this.fastObjectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fastObjectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumn3,
            this.olvColumn4});
            this.fastObjectListView1.HideSelection = false;
            this.fastObjectListView1.Location = new System.Drawing.Point(12, 12);
            this.fastObjectListView1.Name = "fastObjectListView1";
            this.fastObjectListView1.ShowGroups = false;
            this.fastObjectListView1.Size = new System.Drawing.Size(602, 140);
            this.fastObjectListView1.TabIndex = 4;
            this.fastObjectListView1.UseCompatibleStateImageBehavior = false;
            this.fastObjectListView1.View = System.Windows.Forms.View.Details;
            this.fastObjectListView1.VirtualMode = true;
            this.fastObjectListView1.SelectedIndexChanged += FastObjectListView1_SelectedIndexChanged;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "Sid";
            this.olvColumn1.IsEditable = false;
            this.olvColumn1.Text = "Sid";
            this.olvColumn1.Width = 250;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "Mask";
            this.olvColumn2.IsEditable = false;
            this.olvColumn2.Text = "Mask";
            this.olvColumn2.Width = 50;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "Type";
            this.olvColumn3.IsEditable = false;
            this.olvColumn3.Text = "Type";
            this.olvColumn3.Width = 70;
            // 
            // olvColumn4
            // 
            this.olvColumn4.AspectName = "Flags";
            this.olvColumn4.IsEditable = false;
            this.olvColumn4.Text = "Flags";
            this.olvColumn4.Width = 50;
            // 
            // SecurityDescriptorView
            // 
            this.ClientSize = new System.Drawing.Size(626, 300);
            this.Controls.Add(this.fastObjectListView1);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "SecurityDescriptorView";
            this.Text = "Security Descriptor";
            ((System.ComponentModel.ISupportInitialize)(this.fastObjectListView1)).EndInit();
            this.ResumeLayout(false);

        }

        private void FastObjectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FastObjectListView view = (FastObjectListView)sender;
            if (view.SelectedIndex < 0)
            {
                return;
            }
            var row = this.fastObjectListView1.Items[view.SelectedIndex];
            var mask = Convert.ToInt32(row.SubItems[1].Text.ToString(), 16);

            foreach (int i in this.checkedListBox1.CheckedIndices)
            {
                this.checkedListBox1.SetItemCheckState(i, CheckState.Unchecked);
            }
            if ((mask & 1) == 1)
            {
                this.checkedListBox1.SetItemCheckState(0, CheckState.Checked);
            }
            if ((mask & 0x10000) == 0x10000)
            {
                this.checkedListBox1.SetItemCheckState(1, CheckState.Checked);
            }
            if ((mask & 0x20000) == 0x20000)
            {
                this.checkedListBox1.SetItemCheckState(2, CheckState.Checked);
            }
            if ((mask & 0x40000) == 0x40000)
            {
                this.checkedListBox1.SetItemCheckState(3, CheckState.Checked);
            }
            if ((mask & 0x80000) == 0x80000)
            {
                this.checkedListBox1.SetItemCheckState(4, CheckState.Checked);
            }
            if ((mask & 0x100000) == 0x100000)
            {
                this.checkedListBox1.SetItemCheckState(5, CheckState.Checked);
            }
        }

        public void BuildSdView(
            string SddlString
        )
        {
            RawSecurityDescriptor descriptor;
            try
            {
                descriptor = new RawSecurityDescriptor(SddlString);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create RawSecurityDescriptor from " +
                    "the provided SDDL string '" + SddlString + "':  " + ex.Message);
            }

            AddOwner(SddlParser.SidToString(descriptor.Owner));
            AddGroup(SddlParser.SidToString(descriptor.Group));
            if (descriptor.DiscretionaryAcl == null)
            {
                return;
            }
            foreach (var ace in descriptor.DiscretionaryAcl)
            {
                var ntAce = SddlParser.GetAce(ace);
                if (ntAce != null)
                {
                    AddRow(ntAce);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    class AceView
    {
        public NtApiDotNet.AceType Type { get; set; }
        public NtApiDotNet.AceFlags Flags { get; set; }
        public AccessMask Mask { get; set; }
        public string Sid { get; set; }
    }
}
