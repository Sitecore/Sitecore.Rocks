// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins.TreeViewSkins;
using Sitecore.Rocks.UI.SitecoreCop.Dialogs.ValidationProfileDialogs;

namespace Sitecore.Rocks.UI.SitecoreCop
{
    public partial class SitecoreCopTab : IValidationViewer
    {
        private readonly List<ValidationDescriptor> validations = new List<ValidationDescriptor>();

        private Timer countdownTimer;

        public SitecoreCopTab()
        {
            InitializeComponent();

            LoadProfiles();

            var current = AppHost.Settings.GetString("Validation\\Items", "Current", string.Empty);
            if (string.IsNullOrEmpty(current))
            {
                current = "All";
            }

            var item = ProfileComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Content as string == current);
            if (item == null)
            {
                item = ProfileComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Content as string == "All");
            }

            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        public bool Deep { get; private set; }

        [NotNull]
        public string HiddenItems { get; set; }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        [NotNull]
        public TabItem TabItem { get; set; }

        [NotNull]
        protected string FilterText { get; set; }

        [NotNull]
        protected ValidationViewerTreeView TreeView { get; private set; }

        private int CountdownSeconds { get; set; }

        public void Disable(ValidationDescriptor item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var profileName = GetProfileName();
            var key = "[" + item.Name + "]";

            var inactive = AppHost.Settings.GetString("Validation\\Items\\Profiles", profileName, string.Empty);

            if (inactive.Contains(key))
            {
                return;
            }

            inactive += key;

            AppHost.Settings.Set("Validation\\Items\\Profiles", profileName, inactive);
        }

        public void Hide(ValidationDescriptor item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var key = item.GetKey();
            if (HiddenItems.Contains(key))
            {
                return;
            }

            HiddenItems += key;

            var profileName = GetProfileName();
            AppHost.Settings.Set("Validation\\Items\\Hidden", profileName, HiddenItems);
        }

        public void Initialize([NotNull] ItemUri itemUri, bool deep)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            ItemUri = itemUri;
            Deep = deep;

            Loaded += ControlLoaded;
        }

        public void RenderValidations()
        {
            TreeView.RenderValidations(validations.Where(IsItemVisible).OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Title).ThenBy(i1 => i1.Problem));
        }

        public void Rerun()
        {
            Loading.HideLoading(Generating, Viewer, NoItems, NotGenerated);

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.HideLoading(NoItems, Viewer, Generating, NotGenerated);
                    return;
                }

                Loading.HideLoading(Generating, NoItems, Viewer, NotGenerated);
                StartReloadCountdown();
            };

            var profileName = GetProfileName();
            var inactiveValidations = AppHost.Settings.Get("Validation\\Items\\Profiles", profileName, string.Empty) as string ?? string.Empty;
            var customValidations = GetCustomValidations(inactiveValidations);

            ItemUri.Site.DataService.ExecuteAsync("Validations.StartItemValidation", completed, "Site", ItemUri.DatabaseName.ToString(), ItemUri.ItemId.ToString(), LanguageManager.CurrentLanguage.ToString(), Data.Version.Latest.ToString(), inactiveValidations, customValidations, Deep);
        }

        public void StopTimer()
        {
            if (countdownTimer == null)
            {
                return;
            }

            countdownTimer.Dispose();
            countdownTimer = null;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            var profileName = GetProfileName();

            HiddenItems = AppHost.Settings.Get("Validation\\Items\\Hidden", profileName, string.Empty) as string ?? string.Empty;

            TreeView = new ValidationViewerTreeView
            {
                ValidationViewer = this
            };

            Viewer.Child = TreeView;

            LoadValidations(true);
        }

        private void Countdown()
        {
            StopTimer();

            CountdownSeconds--;
            if (CountdownSeconds == 0)
            {
                LoadValidations(false);
                return;
            }

            Retrying.Text = string.Format("Reloading in {0} seconds...", CountdownSeconds);

            TimerCallback callback = delegate { Dispatcher.Invoke(new Action(Countdown)); };

            countdownTimer = new Timer(callback, null, 1000, int.MaxValue);
        }

        private void EditProfiles([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new ValidationProfileDialog(ItemUri.Site, "Items");
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            LoadProfiles();

            var current = d.SelectedValue;

            var item = ProfileComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Content as string == current);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderValidations();
        }

        [NotNull]
        private string GetCustomValidations([NotNull] string inactiveValidations)
        {
            Debug.ArgumentNotNull(inactiveValidations, nameof(inactiveValidations));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("validations");

            foreach (var customValidation in CustomValidationManager.CustomValidations.Where(v => inactiveValidations.IndexOf("[" + v.Title + "]", StringComparison.InvariantCultureIgnoreCase) < 0))
            {
                customValidation.Save(output);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [NotNull]
        private string GetProfileName()
        {
            var comboBoxItem = ProfileComboBox.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null)
            {
                return "All";
            }

            var name = comboBoxItem.Content as string ?? string.Empty;
            if (name == "All")
            {
                name = "Custom";
            }

            return name;
        }

        private bool IsItemVisible([NotNull] ValidationDescriptor item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (HiddenItems.Contains(item.GetKey()))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(FilterText))
            {
                var filtered = item.Title.IsFilterMatch(FilterText);
                filtered |= item.Problem.IsFilterMatch(FilterText);
                filtered |= item.Solution.IsFilterMatch(FilterText);

                if (!filtered)
                {
                    return false;
                }
            }

            return true;
        }

        private bool LoadGenerating([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            return root.GetAttributeValue("generating") == "true";
        }

        private void LoadProfiles()
        {
            var keys = Storage.GetKeys("Validation\\Items\\Profiles");

            ProfileComboBox.Items.Clear();

            foreach (var key in keys)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = key,
                    Tag = AppHost.Settings.GetString("Validation\\Items\\Profiles", key, string.Empty)
                };

                ProfileComboBox.Items.Add(comboBoxItem);
            }

            var all = new ComboBoxItem
            {
                Content = "All",
                Tag = string.Empty
            };

            ProfileComboBox.Items.Add(all);
        }

        private void LoadValidations(bool firstRun)
        {
            Loading.ShowLoading(Viewer);

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                Loading.HideLoading(Viewer);

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseValidations(root, firstRun);
            };

            ItemUri.Site.DataService.ExecuteAsync("Validations.GetItemValidation", completed, "Site", ItemUri.DatabaseName.ToString(), ItemUri.ItemId.ToString(), LanguageManager.CurrentLanguage.ToString(), Data.Version.Latest.ToString(), Deep);
        }

        private void ParseValidationElement([NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            validations.Clear();

            foreach (var item in element.Elements())
            {
                var severity = SeverityLevel.None;

                var severityText = item.GetAttributeValue("severity").ToLowerInvariant();
                switch (severityText)
                {
                    case "error":
                        severity = SeverityLevel.Error;
                        break;
                    case "warning":
                        severity = SeverityLevel.Warning;
                        break;
                    case "suggestion":
                        severity = SeverityLevel.Suggestion;
                        break;
                    case "hint":
                        severity = SeverityLevel.Hint;
                        break;
                }

                var record = new ValidationDescriptor(item.GetAttributeValue("name"), severity, item.GetAttributeValue("category"), item.GetElementValue("title"), item.GetElementValue("problem"), item.GetElementValue("solution"))
                {
                    ExternalLink = item.GetAttributeValue("link")
                };

                var id = item.GetAttributeValue("item");
                if (!string.IsNullOrEmpty(id))
                {
                    var parts = id.Split('/');

                    var databaseUri = new DatabaseUri(ItemUri.Site, new DatabaseName(parts[0]));
                    var itemUri = new ItemUri(databaseUri, new ItemId(Guid.Parse(parts[1])));
                    var itemVersionUri = new ItemVersionUri(itemUri, new Language(parts[2]), new Data.Version(int.Parse(parts[3])));

                    record.ItemUri = itemVersionUri;
                    record.ItemPath = item.GetAttributeValue("itempath");
                }

                validations.Add(record);
            }

            RenderValidations();
        }

        private void ParseValidations([NotNull] XElement root, bool firstRun)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            if (LoadGenerating(root))
            {
                Loading.HideLoading(Generating, NoItems, Viewer, NotGenerated);

                StartReloadCountdown();

                return;
            }

            var xml = root.Value;
            if (string.IsNullOrEmpty(xml))
            {
                if (firstRun)
                {
                    Rerun();
                    return;
                }

                Loading.HideLoading(NotGenerated, NoItems, Viewer, Generating);
                return;
            }

            var element = xml.ToXElement();
            if (element == null)
            {
                if (firstRun)
                {
                    Rerun();
                    return;
                }

                Loading.HideLoading(NoItems, Viewer, Generating, NotGenerated);
                return;
            }

            ParseValidationElement(element);

            if (validations.Count == 0)
            {
                Loading.HideLoading(NoItems, Viewer, Generating, NotGenerated);
            }
            else
            {
                Loading.HideLoading(Viewer, NoItems, Generating, NotGenerated);
            }
        }

        private void Rerun([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rerun();
        }

        private void StartReloadCountdown()
        {
            StopTimer();
            CountdownSeconds = 4;

            Countdown();
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }

        private void UpdateProfile([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var profileName = GetProfileName();
            HiddenItems = AppHost.Settings.GetString("Validation\\Items\\Hidden", profileName, string.Empty);

            AppHost.Settings.Set("Validation\\Items", "Current", profileName);
        }

        private void ValidateSite([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Windows.OpenManagementViewer(ItemUri.Site.Name, new SiteManagementContext(ItemUri.Site), ValidationViewer.ItemName);
        }
    }
}
