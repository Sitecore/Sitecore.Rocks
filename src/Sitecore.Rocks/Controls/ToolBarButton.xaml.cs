// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Controls
{
    public partial class ToolBarButton
    {
        private string _icon;

        private string _text;

        public ToolBarButton()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public string Icon
        {
            get { return _icon; }

            set
            {
                _icon = value;
                ButtonIcon.Icon = value;
                ButtonIcon.Visibility = string.IsNullOrEmpty(_icon) ? Visibility.Collapsed : Visibility.Visible;
                UpdateMargin();
            }
        }

        [CanBeNull]
        public string Text
        {
            get { return _text; }

            set
            {
                _text = value;
                TextBlock.Text = value;
                TextBlock.Visibility = string.IsNullOrEmpty(_text) ? Visibility.Collapsed : Visibility.Visible;
                UpdateMargin();
            }
        }

        private void UpdateMargin()
        {
            if (!string.IsNullOrEmpty(_icon) && !string.IsNullOrEmpty(_text))
            {
                ButtonIcon.Margin = new Thickness(0, 0, 2, 0);
            }
            else
            {
                ButtonIcon.Margin = new Thickness(0, 0, 0, 0);
            }
        }
    }
}
