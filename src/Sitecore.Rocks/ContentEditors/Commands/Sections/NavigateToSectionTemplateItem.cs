// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Sections
{
    [Command, Feature(FeatureNames.AdvancedNavigation)]
    public class NavigateToSectionTemplateItem : CommandBase
    {
        public NavigateToSectionTemplateItem()
        {
            Text = "Navigate to Section Template Item";
            Group = "NavigateSection";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorSectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.SectionItemUri.ItemId == ItemId.Empty)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorSectionContext;
            if (context == null)
            {
                return;
            }

            var uri = new ItemVersionUri(context.SectionItemUri, Language.Current, Version.Latest);

            AppHost.OpenContentEditor(uri);
        }
    }
}
