// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees.Commands.Tools;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Commands.Commands.Databases
{
    [Command(Submenu = ToolsSubmenu.Name, ExcludeFromSearch = true)]
    public class SwitchLanguage : CommandBase
    {
        public SwitchLanguage()
        {
            Text = Resources.SwitchLanguage_SwitchLanguage_Switch_Language;
            Group = "Languages";
            SortingValue = 9000;

            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected IDatabaseSelectionContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            if (parameter is ContentEditorContext)
            {
                return false;
            }

            var context = parameter as IDatabaseSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.DatabaseUri == DatabaseUri.Empty)
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void ContextMenuClosed()
        {
            base.ContextMenuClosed();

            Context = null;
        }

        public override void Execute(object parameter)
        {
        }

        private void LoadLanguages([NotNull] MenuItem menuItem, [NotNull] XElement root, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(root, nameof(root));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var index = -1;

            var loading = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (loading != null)
            {
                index = menuItem.Items.IndexOf(loading);
                menuItem.Items.RemoveAt(index);
            }

            foreach (var element in root.Elements().Reverse())
            {
                var name = element.GetAttributeValue("name");
                var language = new Language(name);

                var item = new MenuItem
                {
                    Header = name + " - " + element.GetAttributeValue("displayname"),
                    Tag = new Tuple<Language, DatabaseUri>(language, databaseUri),
                    IsChecked = language == AppHost.Globals.CurrentLanguage
                };

                item.Click += SetLanguage;

                if (index >= 0)
                {
                    menuItem.Items.Insert(index, item);
                }
                else
                {
                    menuItem.Items.Add(item);
                    index = 0;
                }
            }
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var databaseUri = context.DatabaseUri;

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var item = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (item == null)
            {
                return;
            }

            var index = menuItem.Items.IndexOf(item);
            menuItem.Items.Insert(index + 1, new Separator());

            Site.RequestCompleted completed = delegate(string response)
            {
                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                LoadLanguages(menuItem, root, databaseUri);
            };

            databaseUri.Site.Execute("Languages.GetLanguages", completed, databaseUri.DatabaseName.ToString());
        }

        private void SetLanguage([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var language = menuItem.Tag as Tuple<Language, DatabaseUri>;
            if (language == null)
            {
                return;
            }

            AppHost.Globals.CurrentLanguage = new Language(language.Item1.Name);

            AppHost.Server.Languages.SetContextLanguage(language.Item2.Site, language.Item1.Name, (response, result) => DataService.HandleExecute(response, result));
        }
    }
}
