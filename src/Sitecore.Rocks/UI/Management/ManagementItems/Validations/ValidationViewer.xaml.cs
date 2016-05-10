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
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs.BuildTasks;
using Sitecore.Rocks.UI.Management.ManagementItems.Validations.Skins;
using Sitecore.Rocks.UI.SitecoreCop.Dialogs.ValidationProfileDialogs;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations
{
    [Management(ItemName, 9000)]
    public partial class ValidationViewer : IValidationViewer, IManagementItem, IContextProvider
    {
        public const string ItemName = "Validation";

        public const string ValidationSiteDatabasesAndLanguages = "Validation\\Site\\DatabasesAndLanguages";

        public const string ValidationSiteProfiles = "Validation\\Site\\Profiles";

        private readonly List<ValidationDescriptor> validations = new List<ValidationDescriptor>();

        private Timer countdownTimer;

        private string skinName;

        public ValidationViewer()
        {
            InitializeComponent();

            Loaded += ControlLoaded;

            // TODO: this looks a bug - unsubscribe from Unloaded
            Notifications.Unloaded += delegate(object sender, object window)
            {
                if (this.IsContainedIn(window))
                {
                    StopTimer();
                }
            };

            SkinName = AppHost.Settings.Get("Validation\\Site", "Skin", "Action Center") as string ?? string.Empty;

            FilterText = string.Empty;
            ShowErrors = (AppHost.Settings.Get("Validation\\Site\\View", "Errors", "1") as string ?? "1") == "1";
            ShowWarnings = (AppHost.Settings.Get("Validation\\Site\\View", "Warnings", "1") as string ?? "1") == "1";
            ShowSuggestions = (AppHost.Settings.Get("Validation\\Site\\View", "Suggestions", "1") as string ?? "1") == "1";
            ShowHints = (AppHost.Settings.Get("Validation\\Site\\View", "Hints", "1") as string ?? "1") == "1";

            LoadProfiles();
            var current = AppHost.Settings.GetString("Validation\\Site", "Current", string.Empty);
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

        [NotNull]
        public SiteManagementContext Context { get; set; }

        [NotNull]
        public string HiddenItems { get; set; }

        public bool ShowErrors { get; set; }

        public bool ShowHints { get; set; }

        public bool ShowSuggestions { get; set; }

        public bool ShowWarnings { get; set; }

        [NotNull]
        public string SkinName
        {
            get { return skinName ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                if (skinName == value)
                {
                    return;
                }

                skinName = value;
                AppHost.Settings.Set("Validation\\Site", "Skin", value);

                RenderValidations();
            }
        }

        [NotNull]
        public IEnumerable<ValidationDescriptor> Validations
        {
            get { return validations; }
        }

        [NotNull]
        protected string FilterText { get; set; }

        private int CountdownSeconds { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var site = context as SiteManagementContext;
            if (site == null)
            {
                return false;
            }

            return site.Site.DataService.CanExecuteAsync("Validations.GetValidationReport");
        }

        public void Disable(ValidationDescriptor item)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            var profileName = GetProfileName();
            var key = "[" + item.Name + "]";

            var inactive = AppHost.Settings.GetString(ValidationSiteProfiles, profileName, string.Empty);

            if (inactive.Contains(key))
            {
                return;
            }

            inactive += key;

            AppHost.Settings.Set(ValidationSiteProfiles, profileName, inactive);
        }

        [NotNull]
        public object GetContext()
        {
            return new ValidationContext(this, Validations);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
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
            AppHost.Settings.Set("Validation\\Site\\Hidden", profileName, HiddenItems);
        }

        public void LoadValidationReport()
        {
            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.HideLoading(NoItems, ItemsHolder, Generating, NotGenerated);
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    Loading.HideLoading(NoItems, ItemsHolder, Generating, NotGenerated);
                    return;
                }

                ParseValidations(root);
            };

            Loading.ShowLoading(ItemsHolder, NoItems, Generating, NotGenerated);

            Context.Site.DataService.ExecuteAsync("Validations.GetValidationReport", c, "Site");
        }

        public void RenderValidations()
        {
            var skin = ValidationViewerSkinManager.GetInstance(SkinName) ?? ValidationViewerSkinManager.GetDefaultInstance();

            skin.ValidationViewer = this;
            Viewer.Child = skin.GetControl();

            skin.RenderValidations(validations.Where(IsItemVisible).OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Title).ThenBy(i1 => i1.Problem));
        }

        public void Rerun()
        {
            Loading.HideLoading(Generating, ItemsHolder, NoItems, NotGenerated);

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    Loading.HideLoading(NoItems, ItemsHolder, Generating, NotGenerated);
                    return;
                }

                Loading.HideLoading(Generating, NoItems, ItemsHolder, NotGenerated);
                StartReloadCountdown();
            };

            var profileName = GetProfileName();

            var inactiveValidations = AppHost.Settings.Get(ValidationSiteProfiles, profileName, string.Empty) as string ?? string.Empty;
            var selected = AppHost.Settings.GetString(ValidationSiteDatabasesAndLanguages, Context.Site.Name, "master^en");
            var customValidations = GetCustomValidations(inactiveValidations);

            Context.Site.DataService.ExecuteAsync("Validations.StartValidationAssessment", c, "Site", selected, inactiveValidations, customValidations);
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

            HiddenItems = AppHost.Settings.Get("Validation\\Site\\Hidden", profileName, string.Empty) as string ?? string.Empty;

            if (AppHost.Shell.ShellIdentifier != Constants.SitecoreRocksVisualStudio)
            {
                IncludeInBuildButton.Visibility = Visibility.Collapsed;
            }

            LoadValidationReport();
        }

        private void Countdown()
        {
            StopTimer();

            CountdownSeconds--;
            if (CountdownSeconds == 0)
            {
                LoadValidationReport();
                return;
            }

            Retrying.Text = string.Format("Reloading in {0} seconds...", CountdownSeconds);

            TimerCallback callback = delegate { Dispatcher.Invoke(new Action(Countdown)); };
            countdownTimer = new Timer(callback, null, 1000, int.MaxValue);
        }

        private void EditCustomValidations([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new ManageCustomValidationsDialog();
            AppHost.Shell.ShowDialog(dialog);
        }

        private void EditProfiles([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new ValidationProfileDialog(Context.Site, "Site");
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

        private void IncludeInBuild([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var profileName = GetProfileName();

            var dialog = new BuildTaskDialog(Context.Site, profileName);
            AppHost.Shell.ShowDialog(dialog);
        }

        private bool IsItemVisible([NotNull] ValidationDescriptor item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (HiddenItems.Contains(item.GetKey()))
            {
                return false;
            }

            switch (item.Severity)
            {
                case SeverityLevel.Error:
                    if (!ShowErrors)
                    {
                        return false;
                    }

                    break;

                case SeverityLevel.Warning:
                    if (!ShowWarnings)
                    {
                        return false;
                    }

                    break;

                case SeverityLevel.Suggestion:
                    if (!ShowSuggestions)
                    {
                        return false;
                    }

                    break;

                case SeverityLevel.Hint:
                    if (!ShowHints)
                    {
                        return false;
                    }

                    break;
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

        private void LoadLastUpdate([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            LastUpdate.Visibility = Visibility.Collapsed;

            var timestamp = root.GetAttributeValue("timestamp");
            if (string.IsNullOrEmpty(timestamp))
            {
                return;
            }

            var lastUpdate = DateTimeExtensions.FromIso(timestamp);
            if (lastUpdate == DateTime.MinValue)
            {
                return;
            }

            LastUpdate.Text = "Last Updated: " + lastUpdate;
            LastUpdate.Visibility = Visibility.Visible;
        }

        private void LoadProfiles()
        {
            var keys = Storage.GetKeys(ValidationSiteProfiles);

            ProfileComboBox.Items.Clear();

            foreach (var key in keys)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = key,
                    Tag = AppHost.Settings.GetString(ValidationSiteProfiles, key, string.Empty)
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

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenuHolder.ContextMenu = null;

            var context = GetContext();

            var commands = CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenuHolder.ContextMenu = contextMenu;
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

                    var databaseUri = new DatabaseUri(Context.Site, new DatabaseName(parts[0]));
                    var itemUri = new ItemUri(databaseUri, new ItemId(Guid.Parse(parts[1])));
                    var itemVersionUri = new ItemVersionUri(itemUri, new Language(parts[2]), new Data.Version(int.Parse(parts[3])));

                    record.ItemUri = itemVersionUri;
                    record.ItemPath = item.GetAttributeValue("itempath");
                }

                validations.Add(record);
            }

            RenderValidations();
        }

        private void ParseValidations([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            if (LoadGenerating(root))
            {
                Loading.HideLoading(Generating, NoItems, ItemsHolder, NotGenerated);

                StartReloadCountdown();

                return;
            }

            var xml = root.Value;
            if (string.IsNullOrEmpty(xml))
            {
                Loading.HideLoading(NotGenerated, NoItems, ItemsHolder, Generating);
                return;
            }

            var element = xml.ToXElement();
            if (element == null)
            {
                Loading.HideLoading(NoItems, ItemsHolder, Generating, NotGenerated);
                return;
            }

            LoadLastUpdate(element);
            ParseValidationElement(element);

            Loading.HideLoading(ItemsHolder, NoItems, Generating, NotGenerated);
        }

        private void Rerun([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rerun();
        }

        private void SelectDatabaseAndLanguages([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();
            var command = new SelectDatabasesAndLanguages();

            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void StartReloadCountdown()
        {
            StopTimer();

            CountdownSeconds = 6;

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
            HiddenItems = AppHost.Settings.GetString("Validation\\Site\\Hidden", profileName, string.Empty);

            AppHost.Settings.Set("Validation\\Site", "Current", profileName);
        }
    }
}
