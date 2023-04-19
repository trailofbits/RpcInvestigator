//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using RpcInvestigator.Util.ML;

namespace RpcInvestigator
{
    public partial class SettingsWindow : Form
    {
        public Settings m_Settings;

        public SettingsWindow(Settings CurrentSettings)
        {
            InitializeComponent();
            var pythons = PythonEnvironment.GetInstalledPythons();
            if (pythons.Count > 0)
            {
                pythonDllLocation.Items.AddRange(pythons.ToArray());
            }
            m_Settings = CurrentSettings;
            if (!string.IsNullOrEmpty(m_Settings.m_DbghelpPath))
            {
                dbghelpPath.Text = m_Settings.m_DbghelpPath;
            }
            if (!string.IsNullOrEmpty(m_Settings.m_SymbolPath))
            {
                symbolPath.Text = m_Settings.m_SymbolPath;
            }
            if (!string.IsNullOrEmpty(m_Settings.m_PythonVenvPath))
            {
                pythonVenv.Text = m_Settings.m_PythonVenvPath;
            }
            if (!string.IsNullOrEmpty(m_Settings.m_PythonDllLocation))
            {
                for (int i = 0; i < pythonDllLocation.Items.Count; i++)
                {
                    var item = (string)pythonDllLocation.Items[i];
                    if (m_Settings.m_PythonDllLocation == item)
                    {
                        pythonDllLocation.SetSelected(i, true);
                    }
                    else
                    {
                        pythonDllLocation.SetSelected(i, false);
                    }
                }
            }
            foreach (var item in traceLevelComboBox.Items)
            {
                if (!Enum.TryParse((string)item, out SourceLevels value))
                {
                    continue;
                }
                if (value == m_Settings.m_TraceLevel)
                {
                    traceLevelComboBox.SelectedItem = item;
                    break;
                }
            }
            displayLibraryOnStart.Checked = m_Settings.m_DisplayLibraryOnStart;
            useGPU.Checked = m_Settings.m_UseGpuForInference;
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
            m_Settings.m_PythonVenvPath = pythonVenv.Text;
            m_Settings.m_DisplayLibraryOnStart = displayLibraryOnStart.Checked;
            m_Settings.m_UseGpuForInference = useGPU.Checked;
            if (pythonDllLocation.SelectedItem != null)
            {
                m_Settings.m_PythonDllLocation = (string)pythonDllLocation.SelectedItem;
            }
            Enum.TryParse((string)traceLevelComboBox.SelectedItem, out m_Settings.m_TraceLevel);
            Settings.Save(m_Settings, null);
            Close();
        }

        private void browseButton2_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            m_Settings.m_PythonVenvPath = dialog.SelectedPath;
        }
    }
}
