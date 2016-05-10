// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.IEnumerableExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Publishing.Dialogs
{
    public partial class AdvancedPublishDialog
    {
        private readonly List<PublishProfile> profiles = new List<PublishProfile>();

        public AdvancedPublishDialog([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            InitializeComponent();
            this.InitializeDialog();

            AllLanguages.IsChecked = true;
            AllTargets.IsChecked = true;

            DatabaseUri = databaseUri;

            JobViewer.IsChecked = AppHost.Settings.Options.ShowJobViewer;

            LoadProfiles();

            if (!profiles.Any())
            {
                var profile = new PublishProfile
                {
                    Name = "Default"
                };
                profiles.Add(profile);
            }

            RenderProfiles();

            LoadPublishInformation();

            RelatedItems.Visibility = databaseUri.Site.IsSitecoreVersion(Constants.Versions.Version72) ? Visibility.Visible : Visibility.Collapsed;

            EnableButtons();
        }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        protected bool IsRenderingProfile { get; set; }

        [CanBeNull]
        public PublishProfile GetCurrentProfile()
        {
            var selectedItem = Profiles.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return null;
            }

            return selectedItem.Tag as PublishProfile;
        }

        private void AddProfile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var name = AppHost.Prompt("Add new publish profile:", "Publish Profile");
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var profile = new PublishProfile
            {
                Name = name
            };

            profiles.Add(profile);

            var listBoxItem = new ListBoxItem
            {
                Content = profile.Name,
                Tag = profile
            };

            var index = Profiles.Items.Add(listBoxItem);
            Profiles.SelectedIndex = index;
        }

        private void CheckAllLanguages([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Languages.IsEnabled = false;

            Languages.Children.OfType<CheckBox>().ForEach(c => c.IsChecked = null);

            Commit();
        }

        private void CheckAllTargets([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Targets.IsEnabled = false;

            Targets.Children.OfType<CheckBox>().ForEach(c => c.IsChecked = null);

            Commit();
        }

        private void ClearDetails()
        {
            Details.IsEnabled = false;
        }

        private void Commit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Commit();
        }

        private void Commit()
        {
            if (IsRenderingProfile)
            {
                return;
            }

            var profile = GetCurrentProfile();
            if (profile == null)
            {
                return;
            }

            if (PublishDatabase.IsChecked == true)
            {
                profile.Source = 0;
            }
            else if (PublishItem.IsChecked == true)
            {
                profile.Source = 1;
            }
            else if (PublishItemAndSubItems.IsChecked == true)
            {
                profile.Source = 2;
            }

            if (IncrementalPublish.IsChecked == true)
            {
                profile.Mode = 0;
            }
            else if (SmartPublish.IsChecked == true)
            {
                profile.Mode = 1;
            }
            else if (Republish.IsChecked == true)
            {
                profile.Mode = 2;
            }
            else if (RebuildDatabase.IsChecked == true)
            {
                profile.Mode = 3;
            }

            profile.RelatedItems = RelatedItems.IsChecked == true;

            if (AllLanguages.IsChecked == true)
            {
                profile.Languages.Clear();
            }
            else
            {
                foreach (var checkBox in Languages.Children.OfType<CheckBox>())
                {
                    var language = checkBox.Tag as string ?? string.Empty;

                    if (checkBox.IsChecked == true)
                    {
                        if (!profile.Languages.Contains(language))
                        {
                            profile.Languages.Add(language);
                        }
                    }
                    else
                    {
                        profile.Languages.Remove(language);
                    }
                }
            }

            if (AllTargets.IsChecked == true)
            {
                profile.Targets.Clear();
            }
            else
            {
                foreach (var checkBox in Targets.Children.OfType<CheckBox>())
                {
                    var target = checkBox.Tag as string ?? string.Empty;

                    if (checkBox.IsChecked == true)
                    {
                        if (!profile.Targets.Contains(target))
                        {
                            profile.Targets.Add(target);
                        }
                    }
                    else
                    {
                        profile.Targets.Remove(target);
                    }
                }
            }
        }

        private void EnableButtons()
        {
            RemoveButton.IsEnabled = Profiles.SelectedItem != null;
            RenameButton.IsEnabled = Profiles.SelectedItem != null;
            PublishButton.IsEnabled = Profiles.SelectedItem != null;
        }

        private void LoadProfiles()
        {
            profiles.Clear();

            var value = AppHost.Settings.GetString("Publishing", "Profiles", string.Empty);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var root = value.ToXElement();
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var profile = new PublishProfile();

                profile.Load(element);

                profiles.Add(profile);
            }
        }

        private void LoadPublishInformation()
        {
            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                Profiles.IsEnabled = true;

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                RenderLanguages(root);
                RenderTargets(root);

                if (Profiles.Items.Count > 0)
                {
                    Profiles.SelectedIndex = 0;
                }
            };

            DatabaseUri.Site.DataService.ExecuteAsync("Publishing.GetAdvancedPublishingInformation", completed, DatabaseUri.DatabaseName.ToString());
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Options.ShowJobViewer = JobViewer.IsChecked == true;
            AppHost.Settings.Options.Save();

            SaveProfiles();

            this.Close(true);
        }

        private void RemoveProfile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var profile = GetCurrentProfile();
            if (profile == null)
            {
                return;
            }

            profiles.Remove(profile);
            var selectedIndex = Profiles.SelectedIndex;

            Profiles.Items.Remove(Profiles.SelectedItem);

            if (selectedIndex >= Profiles.Items.Count)
            {
                selectedIndex--;
            }

            Profiles.SelectedIndex = selectedIndex;
        }

        private void RenameProfile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var profile = GetCurrentProfile();
            if (profile == null)
            {
                return;
            }

            var name = AppHost.Prompt("Add new publish profile:", "Rename Publish Profile", profile.Name);
            if (name == null)
            {
                return;
            }

            profile.Name = name;

            var listBoxItem = Profiles.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            listBoxItem.Content = name;
        }

        private void RenderLanguages([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));
            Languages.Children.Clear();

            var languages = root.Element("languages");
            if (languages == null)
            {
                return;
            }

            foreach (var element in languages.Elements())
            {
                var name = element.GetAttributeValue("name");

                var checkBox = new CheckBox
                {
                    Content = name,
                    Tag = name
                };

                checkBox.Checked += Commit;
                checkBox.Unchecked += Commit;

                Languages.Children.Add(checkBox);
            }
        }

        private void RenderProfile([NotNull] PublishProfile profile)
        {
            Debug.ArgumentNotNull(profile, nameof(profile));

            IsRenderingProfile = true;

            switch (profile.Source)
            {
                case 0:
                    PublishDatabase.IsChecked = true;
                    break;
                case 1:
                    PublishItem.IsChecked = true;
                    break;
                case 2:
                    PublishItemAndSubItems.IsChecked = true;
                    break;
            }

            switch (profile.Mode)
            {
                case 0:
                    IncrementalPublish.IsChecked = true;
                    break;
                case 1:
                    SmartPublish.IsChecked = true;
                    break;
                case 2:
                    Republish.IsChecked = true;
                    break;
                case 3:
                    RebuildDatabase.IsChecked = true;
                    break;
            }

            RelatedItems.IsChecked = profile.RelatedItems;

            if (!profile.Languages.Any())
            {
                AllLanguages.IsChecked = true;
            }
            else
            {
                AllLanguages.IsChecked = false;
                foreach (var checkBox in Languages.Children.OfType<CheckBox>())
                {
                    checkBox.IsChecked = profile.Languages.Contains(checkBox.Tag as string);
                }
            }

            if (!profile.Targets.Any())
            {
                AllTargets.IsChecked = true;
            }
            else
            {
                AllTargets.IsChecked = false;
                foreach (var checkBox in Targets.Children.OfType<CheckBox>())
                {
                    checkBox.IsChecked = profile.Targets.Contains(checkBox.Tag as string);
                }
            }

            Details.IsEnabled = true;

            IsRenderingProfile = false;
        }

        private void RenderProfiles()
        {
            foreach (var profile in profiles)
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = profile.Name,
                    Tag = profile
                };

                Profiles.Items.Add(listBoxItem);
            }
        }

        private void RenderTargets([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            Targets.Children.Clear();

            var targets = root.Element("targets");
            if (targets == null)
            {
                return;
            }

            foreach (var element in targets.Elements())
            {
                var name = element.GetAttributeValue("name");

                var checkBox = new CheckBox
                {
                    Content = name,
                    Tag = name
                };

                checkBox.Checked += Commit;
                checkBox.Unchecked += Commit;

                Targets.Children.Add(checkBox);
            }
        }

        private void SaveProfiles()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("profiles");

            foreach (var profile in profiles)
            {
                profile.Save(output);
            }

            output.WriteEndElement();

            AppHost.Settings.Set("Publishing", "Profiles", writer.ToString());
        }

        private void SelectProfile([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ClearDetails();
            EnableButtons();

            var selectedItem = Profiles.SelectedItem as ListBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            var profile = selectedItem.Tag as PublishProfile;
            if (profile == null)
            {
                return;
            }

            RenderProfile(profile);
        }

        private void UncheckAllLanguages([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Languages.IsEnabled = true;
            Languages.Children.OfType<CheckBox>().ForEach(c => c.IsChecked = false);
            Commit();
        }

        private void UncheckAllTargets([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Targets.IsEnabled = true;
            Targets.Children.OfType<CheckBox>().ForEach(c => c.IsChecked = false);
            Commit();
        }
    }
}
