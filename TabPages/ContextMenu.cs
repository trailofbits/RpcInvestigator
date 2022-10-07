using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RpcInvestigator.TabPages
{
    public class ContextMenu : ContextMenuStrip
    {
        public ContextMenu()
        {
            Items.AddRange(new ToolStripMenuItem[]
            {
                new ToolStripMenuItem("Copy Row(s)", null, ContextMenuCopyRow),
                new ToolStripMenuItem("Copy Column", null, ContextMenuCopyColumn),
            });
        }

        public
        void
        ContextMenuCopyRow(
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
            var listview = args.ListView;
            StringBuilder text = new StringBuilder();
            int count = 0;

            foreach (int index in listview.SelectedIndices)
            {
                foreach (ListViewItem.ListViewSubItem cell in listview.Items[index].SubItems)
                {
                    text.Append(cell.Text);
                    text.Append(',');
                }
                text.Length--; // remove trailing ','
                text.AppendLine();
                count++;
            }
            text.Length--; // remove trailing new line
            Clipboard.SetText(text.ToString());
        }

        public
        void
        ContextMenuCopyColumn(
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
            var targetCell = args.ColumnIndex;
            var listview = args.ListView as FastObjectListView;
            StringBuilder text = new StringBuilder();
            int count = 0;
            foreach (int index in listview.SelectedIndices)
            {
                var cell = listview.Items[index].SubItems[targetCell];
                text.AppendLine(cell.Text);
                count++;
            }
            text.Length--; // remove trailing new line
            Clipboard.SetText(text.ToString());
        }

        public
        static
        void
        BuildRightClickMenu (
            CellRightClickEventArgs Args,
            List<ToolStripMenuItem> AdditionalMenuItems
            )
        {
            var row = Args.Model;
            var column = Args.Column;
            if (row == null || column == null)
            {
                return;
            }
            Args.MenuStrip = new ContextMenu();
            foreach (var item in AdditionalMenuItems)
            {
                Args.MenuStrip.Items.Add(item);
            }
            foreach (var item in Args.MenuStrip.Items)
            {
                ((ToolStripMenuItem)item).Tag = Args;
            }
        }

        public
        static
        void
        AddSearchElements (
            CellRightClickEventArgs Args
            )
        {
            var listview = Args.ListView;
            var textbox = new ToolStripTextBox("keyword");
            textbox.Text = "<keyword search>";
            textbox.Click += new EventHandler((object o, EventArgs a) =>
            {
                if (textbox.Text == "<keyword search>")
                {
                    textbox.Text = "";
                }
            });
            textbox.LostFocus += new EventHandler((object o, EventArgs a) =>
            {
                if (textbox.Text == "")
                {
                    textbox.Text = "<keyword search>";
                }
            });
            textbox.KeyUp += new KeyEventHandler((object o, KeyEventArgs a) =>
            {
                if (a.KeyCode != Keys.Enter)
                {
                    return;
                }
                var text = textbox.Text;
                if (string.IsNullOrEmpty(text))
                {
                    listview.ModelFilter = null;
                    Args.MenuStrip.Close();
                    return;
                }

                TextMatchFilter searchFilter = new TextMatchFilter(listview, text.ToLower());
                listview.DefaultRenderer = new HighlightTextRenderer(searchFilter);
                listview.ModelFilter = searchFilter;
                Args.MenuStrip.Close();
            });
            textbox.KeyDown += new KeyEventHandler((object o, KeyEventArgs a) =>
            {
                if (a.KeyCode != Keys.Enter)
                {
                    return;
                }
                a.SuppressKeyPress = true;
                a.Handled = true;
            });
            Args.MenuStrip.Items.Add(textbox);
        }
    }
}
