using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpcInvestigator.Windows
{
    public partial class SecurityDescriptorView : Form
    {
        private DataGridView dataGridView1;
        private RichTextBox richTextBox1;
        private DataGridViewTextBoxColumn Sid;
        private DataGridViewTextBoxColumn Mask;
        private DataGridViewTextBoxColumn Type;
        private DataGridViewTextBoxColumn Flags;
        private CheckedListBox checkedListBox1;
        private Button button1;

        public SecurityDescriptorView(

        )
        {
            InitializeComponent();
        }
        
        public void AddRow(
            string Sid,
            string Mask,
            string Type,
            string Flags
        )
        {
            this.dataGridView1.Rows.Add(Sid, Mask, Type, Flags);
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Sid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Mask = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Flags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Sid,
            this.Mask,
            this.Type,
            this.Flags});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 102;
            this.dataGridView1.Size = new System.Drawing.Size(602, 140);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellClick += DataGridView1_CellClick;
            this.dataGridView1.CellEnter += DataGridView1_CellClick;
            // 
            // Sid
            // 
            this.Sid.HeaderText = "Sid";
            this.Sid.MinimumWidth = 12;
            this.Sid.Name = "Sid";
            this.Sid.ReadOnly = true;
            this.Sid.Width = 250;
            // 
            // Mask
            // 
            this.Mask.HeaderText = "Mask";
            this.Mask.MinimumWidth = 12;
            this.Mask.Name = "Mask";
            this.Mask.ReadOnly = true;
            this.Mask.Width = 50;
            // 
            // Type
            // 
            this.Type.HeaderText = "Type";
            this.Type.MinimumWidth = 12;
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.Width = 70;
            // 
            // Flags
            // 
            this.Flags.HeaderText = "Flags";
            this.Flags.MinimumWidth = 12;
            this.Flags.Name = "Flags";
            this.Flags.ReadOnly = true;
            this.Flags.Width = 50;
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
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.checkedListBox1.Size = new System.Drawing.Size(292, 100);
            this.checkedListBox1.TabIndex = 3;
            // 
            // SecurityDescriptorView
            // 
            this.ClientSize = new System.Drawing.Size(626, 300);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button1);
            this.Name = "SecurityDescriptorView";
            this.Text = "Security Descriptor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SecurityDescriptorView_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        private void DataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SecurityDescriptorView_FormClosing(object sender, EventArgs e)
        {
            //Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            foreach (int i in this.checkedListBox1.CheckedIndices)
            {
                this.checkedListBox1.SetItemCheckState(i, CheckState.Unchecked);
            }
            int rowIndex = e.RowIndex;
            DataGridViewRow row = dataGridView1.Rows[rowIndex];
            foreach (DataGridViewCell cell in row.Cells)
            {  
                if (cell.OwningColumn.Name == "Mask")
                {
                    if (cell.Value == null)
                    {
                        break;
                    }
                    var mask = Convert.ToInt32(cell.Value.ToString(), 16);
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
                        this.checkedListBox1.SetItemCheckState(1, CheckState.Checked);
                    }
                    break;
                }
            }
        }

    }
}
