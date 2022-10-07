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
using System.Windows.Forms;

namespace RpcInvestigator.Windows
{
    public partial class EtwColumnPicker : Form
    {
        public List<string> m_SelectedColumns;

        public EtwColumnPicker(List<string> AllColumns, List<string> DefaultSelectedColumns)
        {
            InitializeComponent();
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
            m_SelectedColumns = columnsList.SelectedItems.Cast<string>().ToList();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
