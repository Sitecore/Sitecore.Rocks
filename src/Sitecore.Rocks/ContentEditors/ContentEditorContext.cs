// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentEditors
{
    public class ContentEditorContext : ICommandContext, IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext
    {
        public ContentEditorContext([NotNull] ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            ContentEditor = contentEditor;
        }

        [NotNull]
        public ContentEditor ContentEditor { get; }

        [CanBeNull]
        public ContextMenuEventArgs EventArgs { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                var contentModel = ContentEditor.ContentModel;
                if (contentModel.IsEmpty)
                {
                    return DatabaseUri.Empty;
                }

                return contentModel.FirstItem.Uri.DatabaseUri;
            }
        }

        IEnumerable<IItem> IItemSelectionContext.Items
        {
            get { return ContentEditor.ContentModel.Items; }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }
    }
}
