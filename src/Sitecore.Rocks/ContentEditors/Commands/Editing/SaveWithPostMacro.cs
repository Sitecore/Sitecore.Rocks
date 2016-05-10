// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentEditors.Commands.Editing
{
    [Command, CommandId(CommandIds.ItemEditor.SaveMacro, typeof(ContentEditorContext)), Feature(FeatureNames.AdvancedOperations)]
    public class SaveWithPostMacro : CommandBase
    {
        public SaveWithPostMacro()
        {
            Text = Resources.SaveWithPostMacro_SaveWithPostMacro_Save__with_Post_Macro_;
            Group = "Navigate";
            SortingValue = 1010;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is ContentEditorContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentEditorContext;
            if (context != null)
            {
                context.ContentEditor.SaveContentModel(true);
            }
        }
    }
}
