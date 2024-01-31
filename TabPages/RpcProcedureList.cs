//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using System.Drawing;
using System.Linq;
using NtApiDotNet;
using NtApiDotNet.Ndr;
using System.Diagnostics;
using RpcInvestigator.Util;
using System.Windows.Controls;
using System.IO;

namespace RpcInvestigator
{
    using static TraceLogger;

    public class RpcProcedureList : TabPage
    {
        public FastObjectListView m_Listview;

        public RpcProcedureList()
        {
            m_Listview = new FastObjectListView();
            Random random = new Random();
            string rand = random.Next().ToString();
            m_Listview.OwnerDraw = true;
            m_Listview.BorderStyle = BorderStyle.FixedSingle;
            m_Listview.CellEditUseWholeCell = false;
            m_Listview.Cursor = Cursors.Default;
            m_Listview.Dock = DockStyle.Fill;
            m_Listview.FullRowSelect = true;
            m_Listview.GridLines = true;
            m_Listview.HideSelection = false;
            m_Listview.Location = new Point(0, 0);
            m_Listview.Margin = new Padding(5);
            m_Listview.Name = "RpcProcedureListview" + rand;
            m_Listview.UseAlternatingBackColors = true;
            m_Listview.AlternateRowBackColor = Color.LightBlue;
            m_Listview.UseCompatibleStateImageBehavior = false;
            m_Listview.View = View.Details;
            m_Listview.VirtualMode = true;
            m_Listview.ShowGroups = false;
            m_Listview.Alignment = ListViewAlignment.Left;
            Generator.GenerateColumns(m_Listview, typeof(NdrProcedureDefinition), true);
            foreach (var column in m_Listview.AllColumns)
            {
                column.Renderer = new NoEllipsisRenderer();
                column.MaximumWidth = -1;
            }
            m_Listview.RowHeight = 125;
            m_Listview.AllColumns[1].AspectToStringConverter = delegate (object Params)
            {
                if (Params == null)
                {
                    return "<null>";
                }
                StringBuilder sb = new StringBuilder();
                foreach (var param in (IList<NdrProcedureParameter>)Params)
                {
                    sb.AppendLine(param.ToString());
                }
                return sb.ToString();
            };
            m_Listview.AllColumns[8].AspectToStringConverter = delegate (object DispatchFunction)
            {
                if (DispatchFunction == null)
                {
                    return "<null>";
                }
                return "0x" + ((IntPtr)DispatchFunction).ToString("X");
            };
            Controls.Add(m_Listview);
        }

        public void Build(string Name, List<NdrProcedureDefinition> Procedures)
        {
            this.Name = Name + " Procedures";
            Text = BuildTabTitle(Name);
            ImageIndex = 2;
            try
            {
                using (NtToken token = NtProcess.Current.OpenToken())
                {
                    if (Procedures.Count > 0)
                    {
                        m_Listview.ClearObjects();
                        m_Listview.SetObjects(Procedures);
                        m_Listview.RebuildColumns();
                        m_Listview.AutoResizeColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace(TraceLoggerType.RpcProcedureList,
                    TraceEventType.Error,
                    "Unable to retrieve RPC procedure list: " + ex.Message);
            }
        }

        public int GetCount()
        {
            if (m_Listview.Objects == null)
            {
                return 0;
            }
            return m_Listview.Objects.Cast<NdrProcedureDefinition>().Count();
        }

        public void ExportProceduresToFile(string filePath)
        {
            string procInfoStr = new string("");
            foreach (NdrProcedureDefinition proc in m_Listview.Objects.Cast<NdrProcedureDefinition>())
            {
                procInfoStr += proc.Name + "\n";
                procInfoStr += "\tParams\n";
                
                foreach (var param in proc.Params)
                {
                    procInfoStr += "\t\t" + param.ToString() + "\n";
                }

                procInfoStr += "\tReturn Value: " + proc.ReturnValue.ToString() + "\n";
                procInfoStr += "\tHandle:" + proc.Handle.ToString() + "\n";
                procInfoStr += "\tRpc Flags:" + proc.RpcFlags.ToString() + "\n";
                procInfoStr += "\tProc Num:" + proc.ProcNum.ToString() + "\n";
                procInfoStr += "\tStack Size:" + proc.StackSize.ToString() + "\n";
                procInfoStr += "\tHas Async Handle:" + proc.HasAsyncHandle.ToString() + "\n";
                procInfoStr += "\tDispatch Function:" + proc.DispatchFunction.ToString() + "\n";
                procInfoStr += "\tDispatch Offset:" + proc.DispatchOffset.ToString() + "\n";
                procInfoStr += "\tInterpreter Flags:" + proc.InterpreterFlags.ToString() + "\n";
            }
            procInfoStr += "\n";
            FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None);
            byte[] strBytes = Encoding.UTF8.GetBytes(procInfoStr);
            file.Write(strBytes, 0, strBytes.Length);
            file.Close();
        }

        private static string BuildTabTitle(string Name)
        {
            var name = Name;
            if (name.Length > 30)
            {
                name = name.Substring(0, 15) + "..." + name.Substring(
                    name.Length - 15, 15);
            }
            return "Procedures for " + name;
        }
    }
}
