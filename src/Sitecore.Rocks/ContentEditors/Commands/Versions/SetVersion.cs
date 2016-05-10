// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Versions
{
    public class SetVersion : CommandBase
    {
        public SetVersion([NotNull] Version version)
        {
            Assert.ArgumentNotNull(version, nameof(version));

            Version = version;
            Group = "Version Numbers";
        }

        [NotNull]
        public Version Version { get; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return;
            }

            if (contentModel.IsMultiple)
            {
                return;
            }

            var item = contentModel.FirstItem;

            if (item.Uri.Version == Version)
            {
                return;
            }

            var list = new List<ItemVersionUri>
            {
                new ItemVersionUri(item.Uri.ItemUri, item.Uri.Language, Version)
            };

            context.ContentEditor.LoadItems(list, new LoadItemsOptions(true));
        }
    }
}
