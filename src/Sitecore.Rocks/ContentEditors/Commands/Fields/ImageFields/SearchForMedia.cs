// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 5220, "Image", "Navigate", Text = "Search", ElementType = RibbonElementType.SmallButton), Command]
    public class SearchForMedia : CommandBase, IToolbarElement
    {
        public SearchForMedia()
        {
            Text = Resources.SearchForMedia_SearchForMedia_Search_for_Recently_Updated_Media;
            Group = "Navigate";
            SortingValue = 200;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (!(field.Control is ImageField))
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

            var mediaViewer = AppHost.Windows.Factory.OpenMediaViewer(context.Field.FieldUris.First().Site);
            if (mediaViewer == null)
            {
                return;
            }

            var to = DateTime.Now.AddDays(1).ToString(@"yyyyMMdd");
            var from = DateTime.Now.AddDays(-7).ToString(@"yyyyMMdd");

            mediaViewer.Search("__updated", string.Format(@"[{0} TO {1}]", from, to));
        }
    }
}
