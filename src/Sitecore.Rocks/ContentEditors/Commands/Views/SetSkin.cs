// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Commands.Views
{
    public class SetSkin : CommandBase
    {
        public SetSkin([NotNull] string skinName)
        {
            Assert.ArgumentNotNull(skinName, nameof(skinName));

            SkinName = skinName;
            Group = "Skins";
        }

        [NotNull]
        public string SkinName { get; }

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

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return;
            }

            var options = AppHost.Settings.Options;
            options.Skin = SkinName;
            options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
