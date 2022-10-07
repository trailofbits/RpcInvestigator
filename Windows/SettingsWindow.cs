using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace RpcInvestigator
{
    public partial class SettingsWindow : Form
    {
        public Settings m_Settings;

        public SettingsWindow(Settings CurrentSettings)
        {
            InitializeComponent();
            m_Settings = CurrentSettings;
            if (!string.IsNullOrEmpty(m_Settings.m_DbghelpPath))
            {
                dbghelpPath.Text = m_Settings.m_DbghelpPath;
            }
            if (!string.IsNullOrEmpty(m_Settings.m_SymbolPath))
            {
                symbolPath.Text = m_Settings.m_SymbolPath;
            }
            foreach (var item in traceLevelComboBox.Items)
            {
                if (!Enum.TryParse((string)item, out TraceLevel value))
                {
                    continue;
                }
                if (value == m_Settings.m_TraceLevel)
                {
                    traceLevelComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void browseButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            m_Settings.m_DbghelpPath = dialog.FileName;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            m_Settings.m_SymbolPath = symbolPath.Text;
            m_Settings.m_DbghelpPath = dbghelpPath.Text;
            Enum.TryParse((string)traceLevelComboBox.SelectedItem, out m_Settings.m_TraceLevel);
            Settings.Save(m_Settings, null);
            Close();
        }
    }
}
