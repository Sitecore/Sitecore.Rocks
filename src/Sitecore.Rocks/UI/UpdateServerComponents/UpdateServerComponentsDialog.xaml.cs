// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.UpdateServer;
using Sitecore.Rocks.UI.UpdateServerComponents.Updates;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    public partial class UpdateServerComponentsDialog
    {
        public UpdateServerComponentsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Updates = new List<UpdateInfo>();
        }

        [NotNull]
        public string WebRootPath { get; private set; }

        [NotNull]
        protected DataService DataService { get; private set; }

        [NotNull]
        protected IEnumerable<InstalledPluginInfo> InstalledPlugins { get; private set; }

        [NotNull]
        protected List<ServerComponentInfo> ServerComponents { get; private set; }

        [CanBeNull]
        protected Site Site { get; private set; }

        [NotNull]
        protected List<UpdateInfo> Updates { get; }

        public static void AutomaticUpdate([NotNull] DataService dataService, [NotNull] string serverName, [NotNull] string webRootPath, [CanBeNull] Site site)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(serverName, nameof(serverName));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var d = new UpdateServerComponentsDialog();

            Action completed = delegate
            {
                if (!d.Updates.Any(info => info.IsChecked))
                {
                    return;
                }

                Action a = delegate
                {
                    if (d.Updates.Any(info => info.IsChecked))
                    {
                        AppHost.Shell.ShowDialog(d);
                    }
                };

                try
                {
                    d.DoInstall(a);
                }
                catch (Exception ex)
                {
                    AppHost.Shell.HandleException(ex);

                    // AppHost.MessageBox("Failed to update.\n\nMessage: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            d.Initialize(dataService, serverName, webRootPath, site, completed);
        }

        public static bool Execute([NotNull] DataService dataService, [NotNull] string serverName, [NotNull] string webRootPath, [CanBeNull] Site site)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(serverName, nameof(serverName));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            var d = new UpdateServerComponentsDialog();

            d.Initialize(dataService, serverName, webRootPath, site, null);

            return AppHost.Shell.ShowDialog(d) == true;
        }

        public void Initialize([NotNull] DataService dataService, [NotNull] string serverName, [NotNull] string webRootPath, [CanBeNull] Site site, [CanBeNull] Action callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(serverName, nameof(serverName));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            DataService = dataService;
            WebRootPath = webRootPath;
            Site = site;

            ServerName.Text = serverName;

            if ((DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                Install.Content = Rocks.Resources.UpdateServerComponentsDialog_Initialize_Install_and_Close;
                Install.Width = 100;
            }

            InstalledPlugins = UpdateServerComponentsManager.GetInstalledPlugins(webRootPath);
            LoadComponents(callback, true);
        }

        private void CheckedChanged([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }

        private void CloseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private bool CloseIfUpdated()
        {
            if (Updates.Any(info => info.IsChecked))
            {
                return false;
            }

            this.Close(true);
            return true;
        }

        private void DoInstall([NotNull] Action completed)
        {
            Debug.ArgumentNotNull(completed, nameof(completed));

            Install.IsEnabled = false;
            InstallAll.IsEnabled = false;
            CloseButton.IsEnabled = false;

            AppHost.DoEvents();

            try
            {
                try
                {
                    var pipeline = UpdateServerPipeline.Run().WithParameters(Updates, DataService, WebRootPath, Site);

                    WebRootPath = pipeline.WebRootPath;

                    if (pipeline.DataService != DataService)
                    {
                        DataService = pipeline.DataService;
                        Install.Content = Rocks.Resources.Install;
                    }

                    DataService.ResetDataService();
                }
                finally
                {
                    InstallAll.IsEnabled = true;
                    Install.IsEnabled = true;
                    CloseButton.IsEnabled = true;
                }
            }
            finally
            {
                Loading.ShowLoading(Server, CheckAll);
            }

            if ((DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                this.Close(true);
                return;
            }

            InstalledPlugins = UpdateServerComponentsManager.GetInstalledPlugins(WebRootPath);
            LoadComponents(completed, false);
        }

        private void EnableButtons()
        {
            Install.IsEnabled = Updates.Any(updateInfo => updateInfo.IsChecked);
            CheckAll.IsChecked = Updates.All(u => u.IsChecked);
        }

        [CanBeNull]
        private ServerComponentInfo GetComponent([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            foreach (var component in ServerComponents)
            {
                if (component.Name == name)
                {
                    return component;
                }
            }

            return null;
        }

        private void GetComponentStatus()
        {
            foreach (var component in ServerComponents)
            {
                GetComponentStatus(component);
            }
        }

        private void GetComponentStatus([NotNull] ServerComponentInfo serverComponent)
        {
            Debug.ArgumentNotNull(serverComponent, nameof(serverComponent));

            var plugin = GetInstalledPlugin(serverComponent.Name);
            if (plugin == null)
            {
                return;
            }

            System.Version pluginVersion;
            System.Version componentVersion;

            try
            {
                pluginVersion = new System.Version(plugin.Version);
            }
            catch
            {
                pluginVersion = new System.Version(0, 0, 0, 0);
            }

            try
            {
                componentVersion = new System.Version(serverComponent.Version);
            }
            catch
            {
                componentVersion = new System.Version(0, 0, 0, 0);
            }

            var update = new UpdateInfo
            {
                Name = serverComponent.Name,
                Plugin = plugin,
                ServerComponent = serverComponent,
                LocalVersion = plugin.Version,
                RuntimeVersion = plugin.RuntimeVersion.ToString(),
                ServerVersion = serverComponent.Version
            };

            if (pluginVersion == componentVersion)
            {
                update.Action = Rocks.Resources.UpdateServerComponentsDialog_GetComponentStatus_None;
            }
            else if (pluginVersion > componentVersion)
            {
                update.Action = Rocks.Resources.UpdateServerComponentsDialog_GetComponentStatus_Copy_to_Server;
                update.IsChecked = true;
            }
            else if (pluginVersion < componentVersion)
            {
                update.Action = Rocks.Resources.UpdateServerComponentsDialog_GetComponentStatus_None___Newer_Version_on_Server;
            }

            Updates.Add(update);
        }

        [CanBeNull]
        private InstalledPluginInfo GetInstalledPlugin([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            foreach (var plugin in InstalledPlugins)
            {
                if (plugin.Name == name)
                {
                    return plugin;
                }
            }

            return null;
        }

        private void GetPluginStatus()
        {
            foreach (var plugin in InstalledPlugins)
            {
                GetPluginStatus(plugin);
            }
        }

        private void GetPluginStatus([NotNull] InstalledPluginInfo plugin)
        {
            Debug.ArgumentNotNull(plugin, nameof(plugin));

            if (!AppHost.Plugins.IsServerComponent(plugin.Name))
            {
                return;
            }

            var component = GetComponent(plugin.Name);
            if (component != null)
            {
                return;
            }

            var update = new UpdateInfo
            {
                Name = plugin.Name,
                Plugin = plugin,
                LocalVersion = plugin.Version,
                ServerVersion = Rocks.Resources.N_A,
                IsChecked = true,
                Action = Rocks.Resources.UpdateServerComponentsDialog_GetComponentStatus_Copy_to_Server
            };

            Updates.Add(update);
        }

        private void InstallAllClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            foreach (var updateInfo in Updates)
            {
                updateInfo.IsChecked = true;
            }

            EnableButtons();
            InstallClick(sender, e);
        }

        private void InstallClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            DoInstall(() => CloseIfUpdated());
        }

        private void LoadComponents([CanBeNull] Action completed, bool firstTime)
        {
            ServerComponents = new List<ServerComponentInfo>();
            Updates.Clear();

            EnableButtons();

            DataService.DisableLoginErrorMessage = true;
            DataService.ExecuteAsync("ServerComponents", (response, result) => LoadComponents(response, result, firstTime, completed));
        }

        private void LoadComponents([NotNull] string response, [NotNull] ExecuteResult executeResult, bool firstTime, [CanBeNull] Action completed)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));

            DataService.DisableLoginErrorMessage = false;

            if (executeResult.Error != null && executeResult.Error.Message == @"No Service")
            {
                RenderComponents();

                if (!firstTime)
                {
                    AppHost.MessageBox("Got a \"No Service\" error.\n\nPlease check that the connection is correct.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (completed != null)
                {
                    completed();
                }

                return;
            }

            if (!DataService.HandleExecute(response, executeResult, false))
            {
                RenderComponents();
                if (completed != null)
                {
                    completed();
                }

                return;
            }

            if (string.IsNullOrEmpty(response))
            {
                RenderComponents();
                if (completed != null)
                {
                    completed();
                }

                return;
            }

            var doc = XDocument.Parse(response);
            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements(@"component"))
            {
                var info = new ServerComponentInfo
                {
                    Name = element.GetAttributeValue("name"),
                    Version = element.GetAttributeValue("version")
                };

                ServerComponents.Add(info);
            }

            RenderComponents();

            if (completed != null)
            {
                completed();
            }
        }

        private void RenderComponents()
        {
            Updates.Clear();

            GetComponentStatus();
            GetPluginStatus();

            Server.ItemsSource = Updates;

            Loading.HideLoading(Server);
            CheckAll.Visibility = Visibility.Visible;

            EnableButtons();
        }

        private void ToggleChecks([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var isChecked = Updates.All(u => u.IsChecked);

            foreach (var updateInfo in Updates)
            {
                updateInfo.IsChecked = !isChecked;
            }

            EnableButtons();
        }
    }
}
