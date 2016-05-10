// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Languages
{
    public class SetLanguage : CommandBase
    {
        public SetLanguage([NotNull] Language language)
        {
            Assert.ArgumentNotNull(language, nameof(language));

            Language = language;
            Group = "Language Names";
        }

        [NotNull]
        public Language Language { get; }

        public override bool CanExecute(object parameter)
        {
            return parameter is ContentEditorContext;
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

            if (contentModel.FirstItem.Uri.Language == Language)
            {
                return;
            }

            LanguageManager.CurrentLanguage = Language;

            var list = new List<ItemVersionUri>
            {
                new ItemVersionUri(contentModel.FirstItem.Uri.ItemUri, Language, contentModel.FirstItem.Uri.Version)
            };

            context.ContentEditor.LoadItems(list, new LoadItemsOptions(true));
        }
    }
}
