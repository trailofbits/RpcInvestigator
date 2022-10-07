//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using BrightIdeasSoftware;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RpcInvestigator.Util
{
    public class NoEllipsisRenderer : BaseRenderer
    {
        //
        // This renderer overrides cell content truncation/ellipsis.
        //
        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap);
            fmt.LineAlignment = StringAlignment.Center;
            switch (this.Column.TextAlign)
            {
                case HorizontalAlignment.Center: fmt.Alignment = StringAlignment.Center; break;
                case HorizontalAlignment.Left: fmt.Alignment = StringAlignment.Near; break;
                case HorizontalAlignment.Right: fmt.Alignment = StringAlignment.Far; break;
            }
            Column.CellVerticalAlignment = StringAlignment.Near;
            //
            // Use GDI+ to preserve multi-line.
            //
            this.UseGdiTextRendering = false;
            this.DrawAlignedImageAndText(g, r);
        }
    }

    public static class ListviewHelpers
    {
        static
        public
        void
        SetColumnWidth(
            FastObjectListView Listview,
            OLVColumn Column
            )
        {
            //
            // Note:  MeasureText is inaccurate. Google it if you care. It requires
            // some hellish combination of rendering flags that must match the
            // underlying control's use of those flags at the GDI+ level, and there
            // doesn't seem to be a straightforward way to get there.
            //

            //
            // Default the column width to the width of the header text.
            //
            int widestItem = TextRenderer.MeasureText(
                Column.Name,
                Column.HeaderFont,
                new Size(0, 0)).Width;

            //
            // Since the listview is in virtual mode, we have to iterate manually
            // using an indexer.
            //
            var items = Listview.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var subitem = item.SubItems[Column.Index];
                int width = TextRenderer.MeasureText(
                    subitem.Text,
                    item.Font,
                    item.Bounds.Size).Width;
                if (width > widestItem)
                {
                    widestItem = width;
                }
            }

            Column.Width = Math.Min(widestItem, 225);
            Column.Width += 15;
        }

        static
        public
        void
        SetRowHeight(FastObjectListView Listview)
        {
            //
            // Note:  MeasureText is inaccurate. Google it if you care. It requires
            // some hellish combination of rendering flags that must match the
            // underlying control's use of those flags at the GDI+ level, and there
            // doesn't seem to be a straightforward way to get there.
            //

            int tallestItem = 0;

            //
            // Since the listview is in virtual mode, we have to iterate manually
            // using an indexer.
            //
            var items = Listview.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                for (int j = 0; j < item.SubItems.Count; j++)
                {
                    var subitem = item.SubItems[j];
                    int height = TextRenderer.MeasureText(
                        subitem.Text,
                        item.Font,
                        item.Bounds.Size).Height;
                    if (height > tallestItem)
                    {
                        tallestItem = height;
                    }
                }
            }

            //
            // Note: Underlying listview control has limitation of 256 characters.
            //
            Listview.RowHeight = Math.Min(tallestItem, 100);
        }
    }
}
