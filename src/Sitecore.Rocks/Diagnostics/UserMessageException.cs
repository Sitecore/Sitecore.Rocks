// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Diagnostics
{
    [UsedImplicitly]
    public class UserMessageException : FriendlyException
    {
        public UserMessageException([CanBeNull] string message) : base(message)
        {
        }

        public override bool Handle()
        {
            AppHost.MessageBox(Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return true;
        }
    }
}
