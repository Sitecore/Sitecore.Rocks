// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command]
    public class PasteFromClipboard : CommandBase
    {
        public PasteFromClipboard()
        {
            Text = Resources.PasteFromClipboard_PasteFromClipboard_Paste;
            Group = "Clipboard";
            SortingValue = 2710;
            Icon = new Icon("Resources/16x16/paste.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            if (!Clipboard.ContainsText())
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

            string text;
            try
            {
                text = Clipboard.GetText();
            }
            catch
            {
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }

            var fieldControl = context.Field.Control;
            if (fieldControl != null)
            {
                fieldControl.SetValue(text);
            }
        }
    }
}
