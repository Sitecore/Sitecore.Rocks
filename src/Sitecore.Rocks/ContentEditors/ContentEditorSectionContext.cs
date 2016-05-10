// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public class ContentEditorSectionContext : ICommandContext, IItemSelectionContext
    {
        public ContentEditorSectionContext([NotNull] ContentEditor contentEditor, [NotNull] ItemUri itemUri, [NotNull] string sectionName, [NotNull] Icon sectionIcon)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(sectionName, nameof(sectionName));
            Assert.ArgumentNotNull(sectionIcon, nameof(sectionIcon));

            ContentEditor = contentEditor;
            SectionItemUri = itemUri;
            SectionName = sectionName;
            SectionIcon = sectionIcon;
        }

        [NotNull]
        public ContentEditor ContentEditor { get; private set; }

        public IEnumerable<IItem> Items
        {
            get
            {
                var itemDescriptor = new ItemDescriptor(SectionItemUri, SectionName, SectionIcon);
                yield return itemDescriptor;
            }
        }

        [NotNull]
        public Icon SectionIcon { get; }

        [NotNull]
        public ItemUri SectionItemUri { get; }

        [NotNull]
        public string SectionName { get; }
    }
}
