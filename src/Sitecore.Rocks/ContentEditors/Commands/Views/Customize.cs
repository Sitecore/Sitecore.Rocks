// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Dialogs;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    [Command(Submenu = ViewSubmenu.Name), Feature(FeatureNames.AdvancedOperations)]
    public class Customize : CommandBase
    {
        public Customize()
        {
            Text = Resources.Customize_Customize_Customize___;
            Group = "Customize";
            SortingValue = 5000;
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

            var d = new AppearanceDialog();

            var contentModel = context.ContentEditor.ContentModel;

            d.Initialize(contentModel);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            context.ContentEditor.Refresh();
        }
    }
}
