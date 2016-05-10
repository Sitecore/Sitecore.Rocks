// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public class ContentEditorFieldContext : ICommandContext
    {
        public ContentEditorFieldContext([NotNull] ContentEditor contentEditor, [NotNull] Field field, [NotNull] object source)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(source, nameof(source));

            ContentEditor = contentEditor;
            Field = field;
            Source = source;
        }

        [NotNull]
        public ContentEditor ContentEditor { get; private set; }

        [NotNull]
        public Field Field { get; private set; }

        [NotNull]
        public object Source { get; private set; }
    }
}
