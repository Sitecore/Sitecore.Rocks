// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Controls
{
    public partial class DialogHelpText
    {
        public DialogHelpText()
        {
            InitializeComponent();
        }

        [NotNull]
        public string Text
        {
            get { return TextTextBlock.Text ?? string.Empty; }

            set { TextTextBlock.Text = value; }
        }
    }
}
