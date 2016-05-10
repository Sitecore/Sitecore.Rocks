// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers.Dialogs;

namespace Sitecore.Rocks.UI.Management.ManagementItems.LanguageViewers.Commands
{
    [Command]
    public class AddNewLanguage : CommandBase
    {
        public AddNewLanguage()
        {
            Text = Resources.AddNewLanguage_AddNewLanguage_Add_New_Language___;
            Group = "Add";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LanguageViewerContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LanguageViewerContext;
            if (context == null)
            {
                return;
            }

            var databaseUri = context.LanguageViewer.Context.DatabaseUri;

            var d = new EditLanguageDialog(databaseUri);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var languageName = d.LanguageName.Text;
            var countryCode = d.CountryCode.Text;
            var customCode = d.CustomCode.Text;
            var codepage = d.CodePage.Text;
            var encoding = d.Encoding.Text;
            var charset = d.Charset.Text;
            var spellchecker = d.SpellcheckerPath.Text;
            var icon = d.IconPath;

            var name = languageName;

            if (!string.IsNullOrEmpty(countryCode))
            {
                name += "-" + countryCode;
            }

            if (!string.IsNullOrEmpty(customCode))
            {
                name += "-" + customCode;
            }

            var fields = string.Format("Regional ISO Code|{0}|ISO|{1}|Code page|{2}|Encoding|{3}|Charset|{4}|Dictionary|{5}|__Icon|{6}", name, languageName, codepage, encoding, charset, spellchecker, icon);

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var parts = response.Split('|');
                if (parts.Length != 2)
                {
                    return;
                }

                Guid itemId;
                if (!Guid.TryParse(parts[0], out itemId))
                {
                    return;
                }

                Guid parentId;
                if (!Guid.TryParse(parts[1], out parentId))
                {
                    return;
                }

                var itemUri = new ItemUri(databaseUri, new ItemId(itemId));
                var parentUri = new ItemUri(itemUri.DatabaseUri, new ItemId(parentId));

                Notifications.RaiseItemAdded(this, new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), parentUri);

                context.LanguageViewer.LoadLanguages();
            };

            databaseUri.Site.DataService.ExecuteAsync("Items.CreateItem", completed, databaseUri.DatabaseName.ToString(), "/sitecore/system/languages", name, "{F68F13A6-3395-426A-B9A1-FA2DC60D94EB}", fields);
        }
    }
}
