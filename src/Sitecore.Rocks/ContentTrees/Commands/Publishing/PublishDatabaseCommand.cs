// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.UI.Publishing.Dialogs;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    public abstract class PublishDatabaseCommand : CommandBase, IRuleAction
    {
        public int Mode { get; set; }

        public string PublishingText { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            var databaseUri = context.DatabaseUri;
            if (databaseUri == DatabaseUri.Empty)
            {
                return false;
            }

            if (!databaseUri.Site.DataService.CanExecuteAsync("Publishing.Publish"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            var databaseUri = context.DatabaseUri;

            Publish(databaseUri);
        }

        protected void Publish([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            Execute(databaseUri);

            if (AppHost.Settings.Options.HidePublishingDialog)
            {
                return;
            }

            var d = new PublishDialog
            {
                Caption = Resources.PublishCommand_Execute_Publishing,
                PublishingText = PublishingText
            };

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            if (!AppHost.Settings.Options.ShowJobViewer)
            {
                return;
            }

            AppHost.Windows.OpenJobViewer(databaseUri.Site);
        }

        bool IRuleAction.CanExecute(object parameter)
        {
            return parameter as IDatabaseSelectionContext != null;
        }

        void IRuleAction.Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context != null)
            {
                Execute(context.DatabaseUri);
            }
        }

        private void Execute([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            databaseUri.Site.DataService.Publish(Mode, databaseUri);

            Notifications.RaisePublishing(this, Mode, databaseUri.DatabaseName);

            AppHost.Statusbar.SetText(PublishingText);
        }
    }
}
