// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Input;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.ContentEditors
{
    public static class AppCommands
    {
        static AppCommands()
        {
            GoBack = new RoutedCommand(@"GoBack", typeof(AppCommands));
            GoForward = new RoutedCommand(@"GoForward", typeof(AppCommands));
        }

        [NotNull]
        public static RoutedCommand GoBack { get; private set; }

        [NotNull]
        public static RoutedCommand GoForward { get; private set; }
    }
}
