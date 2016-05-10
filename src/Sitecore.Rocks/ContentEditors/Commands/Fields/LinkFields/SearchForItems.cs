// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.LinkFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 5200, "Link", "Navigate", ElementType = RibbonElementType.SmallButton), Command]
    public class SearchForItems : CommandBase, IToolbarElement
    {
        public SearchForItems()
        {
            Text = Resources.SearchForItems_SearchForItems_Search_for_Recently_Updated_Items;
            Group = "Navigate";
            SortingValue = 100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (!(field.Control is LinkField))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return;
            }

            var field = context.Field;
            var site = field.FieldUris.First().Site;

            var searchViewer = AppHost.Windows.Factory.OpenSearchViewer(site);
            if (searchViewer == null)
            {
                return;
            }

            var to = DateTime.Now.AddDays(1).ToString(@"yyyyMMdd");
            var from = DateTime.Now.AddDays(-7).ToString(@"yyyyMMdd");

            searchViewer.Search("__updated", string.Format(@"[{0} TO {1}]", from, to));
        }
    }
}
