// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    public class SetValidationViewerSkin : CommandBase
    {
        public SetValidationViewerSkin()
        {
            Group = "Skins";
        }

        [NotNull]
        public string SkinName { get; set; }

        public override bool CanExecute(object parameter)
        {
            return parameter is ValidationContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            context.ValidationViewer.SkinName = SkinName;
        }
    }
}
