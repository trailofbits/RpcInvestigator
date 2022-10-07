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
}
