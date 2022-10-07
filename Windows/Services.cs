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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NtApiDotNet.Win32.Service;
using BrightIdeasSoftware;
using NtApiDotNet.Win32;
using System.Diagnostics;

namespace RpcInvestigator
{
    using static TraceLogger;

    public partial class Services : Form
    {
        public List<string> m_SelectedDlls = new List<string>();
        private Settings m_Settings;

        public Services(Settings Settings)
        {
            InitializeComponent();
            m_Settings = Settings;
        }

        private void Services_Shown(object sender, EventArgs e)
        {
            servicesListview.BorderStyle = BorderStyle.FixedSingle;
            servicesListview.Cursor = Cursors.Default;
            servicesListview.Dock = DockStyle.Fill;
            servicesListview.FullRowSelect = true;
            servicesListview.GridLines = true;
            servicesListview.MultiSelect = true;
            servicesListview.HideSelection = false;
            servicesListview.Location = new Point(0, 0);
            servicesListview.Margin = new Padding(5);
            servicesListview.Name = "selectedServicesListview";
            servicesListview.UseAlternatingBackColors = true;
            servicesListview.AlternateRowBackColor = Color.LightBlue;
            servicesListview.View = View.Details;
            servicesListview.VirtualMode = true;
            servicesListview.Alignment = ListViewAlignment.Left;
            servicesListview.ShowGroups = false;

            IEnumerable<Win32Service> services;

            try
            {
                var scm = ServiceControlManager.Open(
                    "localhost", NtApiDotNet.Win32.ServiceControlManagerAccessRights.All);
                services = scm.GetServices(
                    NtApiDotNet.Win32.ServiceState.Active, (NtApiDotNet.Win32.ServiceType)0x3ff);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to get service list: " + ex.Message);
                return;
            }

            if (services.Count() == 0)
            {
                MessageBox.Show("No services found.");
                return;
            }

            Generator.GenerateColumns(servicesListview, typeof(Win32Service), true);
            foreach (var column in servicesListview.AllColumns)
            {
                if (column.Name.ToLower() == "triggers" ||
                    column.Name.ToLower() == "requiredprivileges" ||
                    column.Name.ToLower() == "dependencies")
                {
                    column.IsVisible = false;
                }
                column.MaximumWidth = -1;
            }

            servicesListview.SetObjects(services);
            servicesListview.RebuildColumns();
            servicesListview.AutoResizeColumns();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            if (servicesListview.SelectedObjects == null ||
                servicesListview.SelectedObjects.Count == 0)
            {
                return;
            }

            var selectedRows = servicesListview.SelectedObjects.Cast<
                Win32Service>().ToList();
            foreach (var row in selectedRows)
            {
                try
                {
                    if (row.ProcessId == 0 || row.ProcessId == 4)
                    {
                        continue;
                    }

                    var process = Process.GetProcessById(row.ProcessId);
                    if (process.Modules == null || process.Modules.Count == 0)
                    {
                        Trace(TraceLoggerType.ServicesWindow,
                            TraceEventType.Warning,
                            "Process " + process.ProcessName + " has " +
                            "no loaded modules.");
                        continue;
                    }
                    process.Modules.Cast<ProcessModule>().ToList().ForEach(
                        module => m_SelectedDlls.Add(module.FileName));
                    m_SelectedDlls = m_SelectedDlls.Distinct().ToList();
                }
                catch (Exception ex)
                {
                    Trace(TraceLoggerType.ServicesWindow,
                        TraceEventType.Error,
                        "Unable to open process with PID=" +
                        row.ProcessId + ":  " + ex.Message);
                }
            }

            var count = m_SelectedDlls.Count;

            if (count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "RPC Investigator found "+count+" DLLs loaded in the selected service processes. Continue?",
                    "Confirm Action",
                    MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
                m_SelectedDlls.Clear();
            }
            else
            {
                MessageBox.Show("No DLLs were discovered for the selected services.");
            }
        }

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            servicesListview.SelectAll();
        }
    }
}
