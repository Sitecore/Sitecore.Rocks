// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Data.DataServices.Dialogs
{
    public partial class WebServiceSiteEditor : ISiteEditorControl
    {
        public WebServiceSiteEditor()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        public SiteEditor SiteEditor { get; set; }

        // TODO: can move to another helper class

        protected string CurrentUserName
        {
            get
            {
                var currentUser = WindowsIdentity.GetCurrent();
                var username = string.Empty;
                if (currentUser != null)
                {
                    username = currentUser.Name;
                }

                return username;
            }
        }

        public void Apply(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var hostName = Server.Text ?? string.Empty;
            var automaticallyUpdate = AutomaticallyUpdate.IsChecked == true && SiteEditor.DataDriver.SelectedValue as string != @"Good Old Web Service";

            site.ChangeConnection(hostName.Trim(), UserName.Text, Password.Password, SiteEditor.DataDriver.SelectedValue as string ?? string.Empty, WebRootPath.Text, string.Empty, automaticallyUpdate, UseWindowsAuthentication.IsChecked.Value);
        }

        public bool CheckUserName()
        {
            if (UseWindowsAuthentication.IsChecked == true)
            {
                return true;
            }

            var userName = UserName.Text;
            if (userName.Contains(@"\"))
            {
                return true;
            }

            switch (AppHost.MessageBox(Rocks.Resources.ProjectSettingsDialog_CheckUserName_, Rocks.Resources.Site, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning))
            {
                case MessageBoxResult.Yes:
                    UserName.Text = @"sitecore\" + UserName.Text;
                    return true;
                case MessageBoxResult.No:
                    return true;
            }

            return false;
        }

        public void CopyFrom(ISiteEditorControl control)
        {
            Assert.ArgumentNotNull(control, nameof(control));

            var s = control as WebServiceSiteEditor;
            if (s == null)
            {
                return;
            }

            Server.Text = s.Server.Text;
            UserName.Text = s.UserName.Text;
            Password.Password = s.Password.Password;
            WebRootPath.Text = s.WebRootPath.Text;
            AutomaticallyUpdate.IsChecked = s.AutomaticallyUpdate.IsChecked;
        }

        public void Display(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Server.Text = site.HostName;
            UserName.Text = site.Credentials.UserName;
            Password.Password = site.Credentials.Password;
            UseWindowsAuthentication.IsChecked = site.Connection.UseWindowsAuth;
            WebRootPath.Text = site.WebRootPath;
            AutomaticallyUpdate.IsChecked = site.AutomaticallyUpdate;
        }

        public void EnableButtons()
        {
            var wasEnabled = AutomaticUpdate.IsEnabled;

            AutomaticUpdate.IsEnabled = !string.IsNullOrEmpty(WebRootPath.Text) && SiteEditor.DataDriver.SelectedValue as string != @"Good Old Web Service";
            AutomaticallyUpdate.IsEnabled = !string.IsNullOrEmpty(WebRootPath.Text) && SiteEditor.DataDriver.SelectedValue as string != @"Good Old Web Service";

            if (wasEnabled != AutomaticUpdate.IsEnabled)
            {
                AutomaticUpdate.IsChecked = AutomaticUpdate.IsEnabled;
            }
        }

        public void Test()
        {
            if (!CheckHostName())
            {
                return;
            }

            if (!CheckUserName())
            {
                return;
            }

            if (!IsValidWebRootPath())
            {
                return;
            }

            Connection connection;

            if (UseWindowsAuthentication.IsChecked == true)
            {
                connection = new Connection
                {
                    UserName = CurrentUserName,
                    UseWindowsAuth = true
                };
            }
            else
            {
                connection = new Connection
                {
                    UserName = UserName.Text,
                    Password = Password.Password
                };
            }

            connection.HostName = (Server.Text ?? string.Empty).Trim();
            connection.DataServiceName = SiteEditor.DataDriver.SelectedValue as string ?? string.Empty;

            var site = new Site(connection);

            SiteEditor.TestButton.IsEnabled = false;
            SiteEditor.OkButton.IsEnabled = false;
            SiteEditor.CancelButton.IsEnabled = false;

            AppHost.DoEvents();

            try
            {
                try
                {
                    if (site.DataService.TestConnection(string.Empty))
                    {
                        AppHost.MessageBox(Rocks.Resources.ProjectSettingsDialog_TestClick_Yes__it_works_, Rocks.Resources.Test_Connection, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    AppHost.MessageBox(Rocks.Resources.SiteEditor_TestClick_OK__it_does_not_work_ + @"\n\n" + ex.Message, Rocks.Resources.Test_Connection, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            finally
            {
                SiteEditor.TestButton.IsEnabled = true;
                SiteEditor.OkButton.IsEnabled = true;
                SiteEditor.CancelButton.IsEnabled = true;
            }
        }

        public bool Validate()
        {
            if (!CheckHostName())
            {
                return false;
            }

            if (!CheckUserName())
            {
                return false;
            }

            if (!IsValidWebRootPath())
            {
                return false;
            }

            return true;
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedPath = SiteHelper.BrowseWebRootPath(WebRootPath.Text, string.Empty);
            if (selectedPath == null)
            {
                return;
            }

            WebRootPath.Text = selectedPath;
        }

        private bool CheckHostName()
        {
            var hostName = Server.Text;
            if (hostName.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                hostName = "http://" + hostName;
            }

            Uri uri;
            if (Uri.TryCreate(hostName, UriKind.Absolute, out uri))
            {
                return true;
            }

            AppHost.MessageBox("The Host Name is not a valid Uri.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadLocalWebSites();
        }

        private bool IsValidWebRootPath()
        {
            var location = WebRootPath.Text;

            if (string.IsNullOrEmpty(location))
            {
                return true;
            }

            if (location.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }

            if (Directory.Exists(Path.Combine(location, @"bin")) && Directory.Exists(Path.Combine(location, @"sitecore")))
            {
                return true;
            }

            if (AppHost.MessageBox(Rocks.Resources.SiteHelper_BrowseWebRootPath_, Rocks.Resources.Information, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                return true;
            }

            return false;
        }

        private void LoadLocalWebSites()
        {
            WebAdministration.UnloadServerManager();

            if (!WebAdministration.CanAdminister)
            {
                return;
            }

            try
            {
                var serverManager = WebAdministration.ServerManager;
                if (serverManager == null)
                {
                    return;
                }

                foreach (var s in serverManager.Sites)
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = s.Name,
                        Tag = s
                    };

                    Server.Items.Add(comboBoxItem);
                }
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }
        }

        private void SetLocalSite([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var text = Server.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            string webRootPath;
            string hostName;
            try
            {
                dynamic website = null;
                foreach (var item in Server.Items.OfType<ComboBoxItem>())
                {
                    if (item.IsSelected)
                    {
                        website = item.Tag;
                        break;
                    }
                }

                if (website == null)
                {
                    return;
                }

                hostName = WebAdministration.GetWebSiteHostName(website);
                webRootPath = WebAdministration.GetWebRootPath(website);
            }
            catch
            {
                return;
            }

            e.Handled = true;

            Dispatcher.BeginInvoke(new Action(delegate { Server.Text = hostName; }));
            WebRootPath.Text = webRootPath;
        }

        private void UseWindowsAuthentication_OnChecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            UserName.Text = CurrentUserName;
            UserName.IsEnabled = false;
            Password.IsEnabled = false;
        }

        private void UseWindowsAuthentication_OnUnchecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            UserName.IsEnabled = true;
            Password.IsEnabled = true;
        }

        private void WebRootPathChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            SiteEditor.EnableButtons();
        }
    }
}
