//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RpcInvestigator.Windows
{
    public partial class EtwColumnPicker : Form
    {
        public List<string> m_SelectedColumns;
        private Settings m_Settings;

        public EtwColumnPicker(
            List<string> AllColumns,
            List<string> DefaultSelectedColumns,
            Settings Settings
            )
        {
            InitializeComponent();
            m_Settings = Settings;
            columnsList.DataSource = AllColumns;
            for (int i = 0; i < columnsList.Items.Count; i++)
            {
                var item = (string)columnsList.Items[i];
                if (DefaultSelectedColumns.Contains(item))
                {
                    columnsList.SetSelected(i, true);
                }
                else
                {
                    columnsList.SetSelected(i, false);
                }
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (columnsList.SelectedItems == null ||
                columnsList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one column.");
                return;
            }
            m_SelectedColumns = columnsList.SelectedItems.Cast<string>().ToList();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (columnsList.SelectedItems == null ||
                columnsList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one column.");
                return;
            }
            m_SelectedColumns = columnsList.SelectedItems.Cast<string>().ToList();
            m_Settings.m_SnifferColumns = m_SelectedColumns;
            Settings.Save(m_Settings, null);
        }
    }
}
