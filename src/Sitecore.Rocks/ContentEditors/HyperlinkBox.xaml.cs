// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public partial class HyperlinkBox
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(@"IsSelected", typeof(bool), typeof(HyperlinkBox));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(@"Text", typeof(string), typeof(HyperlinkBox));

        public HyperlinkBox()
        {
            InitializeComponent();
        }

        public RoutedEventHandler Click
        {
            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Link.Click += value;
            }
        }

        [CanBeNull]
        public ICommand Command
        {
            get { return Link.Command; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Link.Command = value;
            }
        }

        [CanBeNull]
        public object CommandParameter
        {
            get { return Link.CommandParameter; }

            set { Link.CommandParameter = value; }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }

            set { SetValue(IsSelectedProperty, value); }
        }

        [NotNull]
        public string Text
        {
            get { return GetValue(TextProperty) as string ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                SetValue(TextProperty, value);
            }
        }
    }
}
