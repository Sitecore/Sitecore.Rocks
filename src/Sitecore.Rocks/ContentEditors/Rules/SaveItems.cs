// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.ContentEditors.Rules
{
    [RuleAction("save items", "Item Editor")]
    public class SaveItems : RuleAction
    {
        public override bool CanExecute(object parameter)
        {
            return parameter as ContentEditorContext != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context != null)
            {
                context.ContentEditor.Save();
            }
        }
    }
}
