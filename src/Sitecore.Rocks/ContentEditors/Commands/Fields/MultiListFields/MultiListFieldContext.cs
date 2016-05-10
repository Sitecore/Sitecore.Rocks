// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.MultiListFields
{
    public class MultiListFieldContext : ICommandContext
    {
        public MultiListFieldContext([NotNull] ContentEditor contentEditor, [NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            ContentEditor = contentEditor;
            ItemUri = itemUri;
        }

        [NotNull]
        public ContentEditor ContentEditor { get; private set; }

        [NotNull]
        public ItemUri ItemUri { get; private set; }
    }
}
