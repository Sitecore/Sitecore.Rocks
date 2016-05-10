// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public partial class RuleListBoxItem
    {
        private XElement element;

        private int index;

        public RuleListBoxItem()
        {
            InitializeComponent();
        }

        [NotNull]
        public XElement Element
        {
            get { return element; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                element = value;

                Refresh();
            }
        }

        public int Index
        {
            get { return index; }

            set
            {
                index = value;
                Refresh();
            }
        }

        private void Refresh()
        {
            Text.Text = Rocks.Resources.RuleListBoxItem_Refresh_Rule + @" " + Index;
        }
    }
}
