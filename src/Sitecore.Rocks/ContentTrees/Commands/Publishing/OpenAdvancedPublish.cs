// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.UI.Publishing.Dialogs;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.StartPage.Tabs.ContentTab.Publishing.Publishing;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), StartPageCommand("Open the Advanced Publishing dialog", StartPagePublishDatabaseGroup.Name, 8000), Feature(FeatureNames.AdvancedPublishing)]
    public class OpenAdvancedPublish : CommandBase, IStartPageCommand
    {
        public OpenAdvancedPublish()
        {
            Text = Resources.OpenAdvancedPublish_OpenAdvancedPublish_Advanced_Publishing___;
            Group = "Advanced";
            SortingValue = 9000;
        }

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

            if (!databaseUri.Site.DataService.CanExecuteAsync("Publishing.AdvancedPublish"))
            {
                return false;
            }

            return databaseUri != DatabaseUri.Empty;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return;
            }

            var databaseUri = context.DatabaseUri;

            Execute(databaseUri, context);
        }

        bool IStartPageCommand.CanExecute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            return this.HasDatabaseUri(context);
        }

        void IStartPageCommand.Execute(StartPageContext context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var databaseUri = this.GetDatabaseUri(context);
            if (databaseUri == DatabaseUri.Empty)
            {
                return;
            }

            Execute(databaseUri, null);
        }

        private void Execute([NotNull] DatabaseUri databaseUri, [CanBeNull] IDatabaseSelectionContext context)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var dialog = new AdvancedPublishDialog(databaseUri);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var profile = dialog.GetCurrentProfile();
            if (profile == null)
            {
                return;
            }

            var itemIds = string.Empty;
            var itemContext = context as IItemSelectionContext;
            if (itemContext != null)
            {
                if (itemContext.Items.Any())
                {
                    itemIds = string.Join("|", itemContext.Items.Select(i => i.ItemUri.ItemId.ToString()));
                }
            }

            var languageList = profile.Languages.Aggregate(string.Empty, (text, part) => text + (text.Length > 0 ? "|" : string.Empty) + part);
            var targetList = profile.Targets.Aggregate(string.Empty, (text, part) => text + (text.Length > 0 ? "|" : string.Empty) + part);

            ExecuteCompleted completed = (response, result) => DataService.HandleExecute(response, result);

            databaseUri.Site.DataService.ExecuteAsync("Publishing.AdvancedPublish", completed, databaseUri.DatabaseName.ToString(), itemIds, profile.Mode, profile.Source, languageList, targetList, profile.RelatedItems);

            if (AppHost.Settings.Options.ShowJobViewer)
            {
                AppHost.Windows.OpenJobViewer(databaseUri.Site);
            }
        }
    }
}
