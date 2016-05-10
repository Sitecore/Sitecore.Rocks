// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.StartPage
{
    public interface IStartPageCommand
    {
        bool CanExecute([NotNull] StartPageContext context);

        void Execute([NotNull] StartPageContext context);
    }
}
