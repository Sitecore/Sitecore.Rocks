// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.Media.Commands
{
    public class SetMediaSkin : CommandBase
    {
        public SetMediaSkin()
        {
            Group = "Skins";
        }

        public string SkinName { get; set; }

        public override bool CanExecute(object parameter)
        {
            return parameter is MediaContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as MediaContext;
            if (context == null)
            {
                return;
            }

            context.MediaViewer.SkinName = SkinName;
        }
    }
}
