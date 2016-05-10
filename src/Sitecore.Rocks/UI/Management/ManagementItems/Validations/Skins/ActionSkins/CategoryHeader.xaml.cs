// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.ActionSkins
{
    public partial class CategoryHeader
    {
        public CategoryHeader([NotNull] string key, [NotNull] string header)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(header, nameof(header));

            InitializeComponent();

            Key = key;
            Header = header;
        }

        [NotNull]
        public string Header
        {
            get { return HeaderField.Text; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                HeaderField.Text = value;
            }
        }

        public bool IsExpanded
        {
            get { return Expander.IsExpanded; }

            set { Expander.IsExpanded = value; }
        }

        [NotNull]
        public string Key { get; }

        private void Collapsed([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set("Management\\Validation\\Categories", Key, "0");
        }

        private void Expanded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set("Management\\Validation\\Categories", Key, "1");
        }
    }
}
