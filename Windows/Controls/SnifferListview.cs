//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using RpcInvestigator.Util;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace RpcInvestigator.Windows.Controls
{

    internal class SnifferListview : FastObjectListView
    {
        private List<string> m_ChosenColumns;
        private readonly SnifferCallbackTable m_Callbacks;

        public SnifferListview(
            SnifferCallbackTable Callbacks,
            List<string> SavedColumns
            )
        {
            m_Callbacks = Callbacks;
            DoubleBuffered = true;
            Visible = false;
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            CellEditUseWholeCell = false;
            Dock = System.Windows.Forms.DockStyle.Fill;
            FullRowSelect = true;
            GridLines = true;
            HideSelection = false;
            Location = new System.Drawing.Point(6, 130);
            Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            Name = "snifferListview";
            ShowGroups = false;
            Size = new System.Drawing.Size(2940, 1336);
            TabIndex = 0;
            UseAlternatingBackColors = true;
            View = View.Details;
            AlternateRowBackColor = Color.LightBlue;
            UseCompatibleStateImageBehavior = false;
            VirtualMode = true;
            if (SavedColumns.Count() == 0)
            {
                m_ChosenColumns = new List<string>() {
                    "Timestamp",
                    "ProcessId",
                    "ThreadId",
                    "UserSid",
                    "Task",
                    "Opcode",
                    "InterfaceUuid",
                    "ProcNum",
                    "Protocol",
                    "NetworkAddress",
                    "Endpoint",
                    "Params",
                    "AuthenticationLevel",
                    "ImpersonationLevel"
                };
            }
            else
            {
                m_ChosenColumns = SavedColumns;
            }

            DoubleClick += new EventHandler((object obj, EventArgs e2) =>
            {
                if (SelectedObjects == null || SelectedObjects.Count == 0)
                {
                    return;
                }
                var selectedRow = SelectedObjects.Cast<ParsedEtwEvent>().ToList()[0];
            });
            CellRightClick += RightClickHandler;
        }

        public
        bool
        ChooseColumns(Guid EtwProviderGuid)
        {
            //
            // Build a list of all possible columns using reflection.
            //
            var allColumns = new List<string>();
            Generator.GenerateColumns(this, typeof(ParsedEtwEvent), false);
            foreach (var col in AllColumns)
            {
                allColumns.Add(col.Name);
            }
            AllColumns.Clear();

            try
            {
                //
                // Add columns for custom fields inside UserData.
                //
                var info = EtwProviderParser.GetDistinctProviderEventInfo(
                    EtwProviderGuid);
                if (info.ContainsKey("UserDataProperties"))
                {
                    allColumns.AddRange(info["UserDataProperties"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load columns: " +
                    ex.Message);
                return false;
            }

            using (var chooser = new EtwColumnPicker(allColumns, m_ChosenColumns))
            {
                chooser.ShowDialog();
                if (chooser.DialogResult == DialogResult.OK)
                {
                    m_ChosenColumns = chooser.m_SelectedColumns;
                    BuildColumns();
                    return true;
                }
            }
            return false;
        }

        public
        void
        SetGroupingStrategy(bool EnableGroupingByActivity)
        {
            if (!EnableGroupingByActivity)
            {
                ShowGroups = false;
                return;
            }
            ShowGroups = true;
            AlwaysGroupByColumn = AllColumns[4]; // ActivityID
            BuildGroups();
        }

        public
        void
        BuildColumns()
        {
            //
            // Add UserData columns that were chosen by the user (or the default ones)
            //
            var cols = new List<OLVColumn>();
            foreach (var columnName in m_ChosenColumns)
            {
                OLVColumn col;

                if (columnName == "Params")
                {
                    //
                    // This is a special-case hack for Params, which is an
                    // array of structs. This particular property is useful
                    // for RPC, so we'll manually handle it. Other arrays
                    // are not generally handled, because we have no idea
                    // how many elements are there. Columns cannot be created
                    // randomly on the fly as data comes in, so we have to
                    // choose a random number of array elements to support now.
                    // The column names produced below are aligned with how
                    // EtwEventParser handles storing array elements in the
                    // UserDataProperties array.
                    //
                    for (int i = 0; i < 4; i++)
                    {
                        col = new OLVColumn("Params-" + i, null);
                        col.Renderer = new NoEllipsisRenderer();
                        col.Name = "Params-" + i;
                        col.AspectGetter = delegate (object Row)
                        {
                            if (Row == null)
                            {
                                return "";
                            }
                            var row = (ParsedEtwEvent)Row;
                            if (!row.UserDataProperties.ContainsKey(col.Name))
                            {
                                return "";
                            }
                            return row.UserDataProperties[col.Name];
                        };
                        col.IsVisible = true;
                        cols.Add(col);
                    }
                    continue;
                }

                col = new OLVColumn(columnName, null);
                col.Renderer = new NoEllipsisRenderer();
                col.Name = columnName;
                col.AspectGetter = delegate (object Row)
                {
                    if (Row == null)
                    {
                        return "";
                    }
                    var row = (ParsedEtwEvent)Row;
                    //
                    // Use reflection to try to pull the value directly from a field
                    // in the structure that matches this column name.
                    //
                    object value = null;
                    try
                    {
                        var properties = Row.GetType().GetProperties(
                            BindingFlags.Public | BindingFlags.Instance);
                        properties.ToList().ForEach(property =>
                        {
                            if (property.Name.ToLower() == col.Name.ToLower())
                            {
                                value = property.GetValue(Row, null);
                            }
                        });
                    }
                    catch (Exception) { }

                    //
                    // Next, try the UserDataProperties array.
                    //
                    if (value == null)
                    {
                        if (!row.UserDataProperties.ContainsKey(col.Name))
                        {
                            return "";
                        }
                        return row.UserDataProperties[col.Name];
                    }
                    return value;
                };
                col.AspectToStringConverter = delegate (object Item)
                {
                    if (Item == null)
                    {
                        return "";
                    }
                    if (!long.TryParse(Item.ToString(), out long value))
                    {
                        return Item.ToString();
                    }
                    return "0x" + value.ToString("X");
                };
                col.IsVisible = true;
                cols.Add(col);
            }
            AllColumns.AddRange(cols.ToArray());
            foreach (var column in AllColumns)
            {
                column.MaximumWidth = -1;
            }
            RebuildColumns();
        }

        public
        void
        Update(List<ParsedEtwEvent> Events)
        {
            SuspendLayout();
            AddObjects(Events);
            AllColumns.ForEach(col =>
            {
                ListviewHelpers.SetColumnWidth(this, col);
            });
            ListviewHelpers.SetRowHeight(this);
            ResumeLayout();
        }

        public
        new
        void
        Reset()
        {
            ClearObjects();
            Groups.Clear();
        }

        public
        void
        SaveAsText()
        {
            if (Objects == null)
            {
                return;
            }

            var data = Objects.Cast<ParsedEtwEvent>().ToList();
            if (data.Count == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            data.ForEach(ev =>
            {
                sb.AppendLine(ev.ToString());
            });
            var location = Path.GetRandomFileName() + ".txt";
            File.WriteAllText(location, sb.ToString());
            Process.Start(location);
        }

        private
        void
        RightClickHandler(
            object Obj,
            CellRightClickEventArgs Args
            )
        {
            TabPages.ContextMenu.BuildRightClickMenu(Args, new List<ToolStripMenuItem>{
                new ToolStripMenuItem("Open in Library", null, ContextMenuOpenInLibrary),
            });
        }

        private
        void
        ContextMenuOpenInLibrary(
            object Sender,
            EventArgs Args
            )
        {
            object tag = ((ToolStripMenuItem)Sender).Tag;
            if (tag == null)
            {
                return;
            }
            var args = (CellRightClickEventArgs)tag;
            var evt = args.Model as ParsedEtwEvent;
            if (!evt.UserDataProperties.ContainsKey("InterfaceUuid"))
            {
                MessageBox.Show("Unable to show RPC server details because " +
                    "there is no interface UUID present in the ETW event.");
                return;
            }
            var interfaceId = evt.UserDataProperties["InterfaceUuid"].Replace(
                "{", "").Replace("}", "");
            m_Callbacks.ShowRpcServerDetailsCallback(interfaceId);
        }
    }
}
