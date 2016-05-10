// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectDatabaseDialogs;

namespace Sitecore.Rocks.UI.StartPage
{
    public partial class StartPageViewer
    {
        private readonly List<Tuple<IStartPageCommand, Hyperlink>> hyperlinks = new List<Tuple<IStartPageCommand, Hyperlink>>();

        private DatabaseUri databaseUri;

        public StartPageViewer()
        {
            InitializeComponent();

            DatabaseUri = DatabaseUri.Empty;

            Tabs = new StartPageTabControl(this);
            Tabs.SetValue(Grid.RowProperty, 1);
            Tabs.SetValue(Grid.ColumnProperty, 1);
            Tabs.Margin = new Thickness(4);
            TabsPanel.Children.Add(Tabs);

            ShowOnStartUp.IsChecked = AppHost.Settings.Options.ShowStartPageOnStartup;

            Notifications.RegisterSiteEvents(this, activeDatabaseChanged: HandleActiveDatabaseChanged);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return databaseUri; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                databaseUri = value;

                if (databaseUri == DatabaseUri.Empty)
                {
                    ActiveSiteTextBlock.Text = "<None>";
                }
                else
                {
                    ActiveSiteTextBlock.Text = string.Format("{0} - {1}", value.Site.Name, value.DatabaseName);
                }
            }
        }

        [NotNull]
        public IPane Pane { get; set; }

        [NotNull]
        public StartPageTabControl Tabs { get; set; }

        public static void Open()
        {
            AppHost.Windows.OpenStartPage();
        }

        public void UpdateLinks()
        {
            /*
      var context = new StartPageContext(this);

      foreach (var pair in this.hyperlinks)
      {
        var command = pair.Item1;
        var hyperlink = pair.Item2;

        var canExecute = command.CanExecute(context);

        hyperlink.Foreground = canExecute ? EnabledLink : DisabledLink;
      }
      */
        }

        internal void AddHyperlink([NotNull] IStartPageCommand command, [NotNull] Hyperlink hyperlink)
        {
            Debug.ArgumentNotNull(command, nameof(command));
            Debug.ArgumentNotNull(hyperlink, nameof(hyperlink));

            hyperlinks.Add(new Tuple<IStartPageCommand, Hyperlink>(command, hyperlink));
        }

        private void ChangeDatabase([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectDatabaseDialog
            {
                SelectedDatabaseUri = DatabaseUri
            };

            if (dialog.ShowDialog())
            {
                AppHost.Settings.ActiveDatabaseUri = dialog.SelectedDatabaseUri;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            DatabaseUri = AppHost.Settings.ActiveDatabaseUri;
            UpdateLinks();
        }

        private void HandleActiveDatabaseChanged([NotNull] object sender, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            UpdateLinks();
        }

        private void SetShowOnStartUp([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Options.ShowStartPageOnStartup = ShowOnStartUp.IsChecked == true;
            AppHost.Settings.Options.Save();
        }
    }
}
