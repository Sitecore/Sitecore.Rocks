// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Rules.Actions
{
    [RuleAction("show the message [message,value,,text]", "System")]
    public class ShowMessageAction : RuleAction
    {
        public ShowMessageAction()
        {
            Text = "Show Message";
        }

        [CanBeNull]
        public string Message { get; set; }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var message = Message;
            if (string.IsNullOrEmpty(message))
            {
                message = "[No text]";
            }

            AppHost.MessageBox(message, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
