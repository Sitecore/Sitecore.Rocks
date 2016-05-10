// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Navigating
{
    public class NavigateToBaseTemplate : CommandBase
    {
        public NavigateToBaseTemplate([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            TemplateUri = templateUri;

            Group = "Templates";
            SortingValue = 5000;
        }

        [NotNull]
        public ItemUri TemplateUri { get; }

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

            var uri = new ItemVersionUri(TemplateUri, LanguageManager.CurrentLanguage, Version.Latest);

            AppHost.OpenContentEditor(uri);
        }
    }
}
