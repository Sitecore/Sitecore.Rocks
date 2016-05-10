// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Management
{
    public interface IManagementItem
    {
        bool CanExecute([NotNull] IManagementContext context);

        [NotNull]
        UIElement GetControl([NotNull] IManagementContext context);
    }
}
