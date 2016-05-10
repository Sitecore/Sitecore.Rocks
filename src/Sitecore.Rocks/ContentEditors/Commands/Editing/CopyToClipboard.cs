// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class CopyToClipboard : CommandBase
    {
        public CopyToClipboard()
        {
            Text = Resources.CopyToClipboard_CopyToClipboard_Copy;
            Group = "Clipboard";
            SortingValue = 2700;
            Icon = new Icon("Resources/16x16/copy.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            if (context.Field.Control == null)
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

            if (context.Field.Control == null)
            {
                return;
            }

            AppHost.Clipboard.SetText(context.Field.Control.GetValue());
        }
    }
}
