// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command, CommandId(CommandIds.ItemEditor.Reload, typeof(ContentEditorContext), ToolBar = ToolBars.ItemEditor.Id, Group = ToolBars.ItemEditor.Refresh, Icon = "Resources/16x16/refresh.png", Priority = 0x0999), ToolbarElement(typeof(ContentEditorContext), 1990, "Home", "Tasks", Icon = "Resources/32x32/Recurring.png")]
    public class Reload : CommandBase, IToolbarElement
    {
        public Reload()
        {
            Text = Resources.Reload;
            Group = "Reload";
            SortingValue = 9999;
            Icon = new Icon("Resources/16x16/refresh.png");
        }

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

            context.ContentEditor.ContentModel.IsModified = false;
            context.ContentEditor.Pane.SetModifiedFlag(false);

            var options = new LoadItemsOptions(false)
            {
                ForceReload = true
            };

            context.ContentEditor.LoadItems(context.ContentEditor.ContentModel.UriList, options);
        }
    }
}
