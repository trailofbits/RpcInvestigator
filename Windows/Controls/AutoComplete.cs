//
// Copyright (c) 2022-present, Trail of Bits, Inc.
// All rights reserved.
//
// This source code is licensed in accordance with the terms specified in
// the LICENSE file found in the root directory of this source tree.
//
using FastColoredTextBoxNS;

namespace RpcInvestigator.Windows.Controls
{
    internal class AutoCompleteItem : MethodAutocompleteItem
    {
        private string m_Text;

        public AutoCompleteItem(string Text) : base(Text)
        {
            m_Text = Text.ToLower();
        }

        public override CompareResult Compare(string Fragment)
        {
            if (m_Text.Contains(Fragment.ToLower()))
            {
                return CompareResult.Visible;
            }
            return CompareResult.Hidden;
        }
    }
}
