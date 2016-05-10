// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels.Docks.ContentEditorWarnings
{
    public partial class ContentEditorWarnings
    {
        public ContentEditorWarnings()
        {
            InitializeComponent();
        }

        public void Load([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            Panel.Children.Clear();

            if (contentModel.IsMultiple || contentModel.IsEmpty)
            {
                return;
            }

            var item = contentModel.FirstItem;

            foreach (var warning in item.Warnings)
            {
                var contentEditorWarning = new ContentEditorWarning();
                contentEditorWarning.Load(warning);
                Panel.Children.Add(contentEditorWarning);
            }
        }
    }
}
