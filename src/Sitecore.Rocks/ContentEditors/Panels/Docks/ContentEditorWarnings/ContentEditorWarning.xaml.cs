// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ImageExtensions;

namespace Sitecore.Rocks.ContentEditors.Panels.Docks.ContentEditorWarnings
{
    public partial class ContentEditorWarning
    {
        public ContentEditorWarning()
        {
            InitializeComponent();
        }

        public void Load([NotNull] Warning warning)
        {
            Assert.ArgumentNotNull(warning, nameof(warning));

            Title.Text = warning.Title;
            Text.Text = warning.Text;
            Icon.SetImage(warning.Icon);

            if (string.IsNullOrEmpty(warning.Text))
            {
                Text.Visibility = Visibility.Collapsed;
            }
        }
    }
}
