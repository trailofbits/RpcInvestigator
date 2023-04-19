using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
