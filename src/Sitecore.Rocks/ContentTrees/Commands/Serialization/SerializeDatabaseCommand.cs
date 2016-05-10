// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Rules;

namespace Sitecore.Rocks.ContentTrees.Commands.Serialization
{
    public abstract class SerializeDatabaseCommand : CommandBase, IRuleAction
    {
        [NotNull]
        protected string ConfirmationText { get; set; }

        [NotNull]
        protected string SerializationText { get; set; }

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

            if (!databaseUri.Site.DataService.CanExecuteAsync("Serialization.SerializeDatabase"))
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

            Execute(context.DatabaseUri);
        }

        protected void Execute([NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            if (AppHost.MessageBox(ConfirmationText, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                AppHost.Statusbar.SetText(string.Empty);
                DataService.HandleExecute(response, executeResult);
            };

            Execute(databaseUri, callback);

            AppHost.Statusbar.SetText(SerializationText);
        }

        protected abstract void Execute([NotNull] DatabaseUri itemUri, [NotNull] ExecuteCompleted callback);

        bool IRuleAction.CanExecute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.DatabaseUri == DatabaseUri.Empty)
            {
                return false;
            }

            return context.DatabaseUri.Site.CanExecute;
        }

        void IRuleAction.Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            if (context.DatabaseUri != DatabaseUri.Empty)
            {
                Execute(context.DatabaseUri);
            }
        }
    }
}
