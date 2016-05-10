// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Searching
{
    [Command(Submenu = "Search"), Feature(FeatureNames.AdvancedNavigation)]
    public class SearchForTemplate : SearchCommand
    {
        public SearchForTemplate()
        {
            Text = Resources.SearchForTemplate_SearchForTemplate_Items_with_Same_Template;
            Group = "Item";
            SortingValue = 6000;

            FieldName = "_template";
        }

        protected override string GetValue(ContentModel contentModel)
        {
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            return contentModel.FirstItem.TemplateId.ToShortId();
        }
    }
}
