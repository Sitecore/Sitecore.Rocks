// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Appearances;
using Sitecore.Rocks.ContentEditors.Commands.Views;
using Sitecore.Rocks.ContentEditors.Panels;
using Sitecore.Rocks.ContentEditors.Panes;
using Sitecore.Rocks.ContentEditors.Pipelines.LoadItems;
using Sitecore.Rocks.ContentEditors.Pipelines.SaveItem;
using Sitecore.Rocks.ContentEditors.Pipelines.SetFieldVisibility;
using Sitecore.Rocks.ContentEditors.Skins;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DependencyObjectExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentEditors
{
    public partial class ContentEditor : ISavable, IContextProvider, ISelectionTracking
    {
        private static readonly object SyncRoot = new object();

        public ContentEditor()
        {
            ContentModel = new ContentModel();
            Journal = new Journal<List<ItemVersionUri>>();

            InitializeComponent();

            AppearanceOptions = AppearanceManager.GetDefaultAppearanceOptions(this);

            BackButton.CommandTarget = this;
            ForwardButton.CommandTarget = this;
            Filter = string.Empty;
            ItemBreadcrumb.Initialize(this);

            Notifications.RegisterItemEvents(this, modified: ItemModified, renamed: ItemRenamed, deleted: ItemDeleted);
            Notifications.RegisterFieldEvents(this, FieldChanged);
            Notifications.RegisterTemplateEvents(this, saved: TemplateSaved, changed: TemplateChanged);

            GotFocus += FocusControl;
            Loaded += ControlLoaded;
            Notifications.Unloaded += ControlUnloaded;

            FilterWidth.Width = new GridLength(AppHost.Settings.GetInt("ContentEditor", "SplitterPosition", 150));
        }

        [NotNull]
        public AppearanceOptions AppearanceOptions { get; set; }

        [NotNull]
        public ContentModel ContentModel { get; set; }

        [NotNull, Localizable(false)]
        public string Filter { get; set; }

        [NotNull]
        public Journal<List<ItemVersionUri>> Journal { get; }

        [NotNull]
        public IEditorPane Pane { get; set; }

        [CanBeNull]
        private LoadItemsOperation LoadItemsOperation { get; set; }

        public void ClearFieldControls([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            foreach (var field in contentModel.Fields)
            {
                if (field.Control == null)
                {
                    continue;
                }

                FieldControlManager.RemoveWatcher(field.Control);
                field.Control.ValueModified -= contentModel.ValueModified;
            }

            var skin = AppearanceOptions.Skin as ISupportsReusableFieldControls;
            if (skin == null)
            {
                return;
            }

            foreach (var field in contentModel.Fields)
            {
                var removed = skin.RemoveFieldControl(field);
                if (!removed)
                {
                    continue;
                }

                var reusableFieldControl = field.Control as IReusableFieldControl;
                if (reusableFieldControl == null)
                {
                    continue;
                }

                reusableFieldControl.UnsetField();

                FieldTypeManager.Reuse(field.ActualFieldType, reusableFieldControl);
            }

            skin.Clear();
        }

        public void Disable()
        {
            Editor.IsEnabled = false;
        }

        public void Enable()
        {
            Editor.IsEnabled = true;
        }

        [NotNull]
        public object GetContext()
        {
            return new ContentEditorContext(this);
        }

        public void LoadItems([NotNull] List<ItemVersionUri> itemUriList, [NotNull] LoadItemsOptions options)
        {
            Assert.ArgumentNotNull(itemUriList, nameof(itemUriList));
            Assert.ArgumentNotNull(options, nameof(options));

            if (LoadItemsOperation != null && LoadItemsOperation.ItemsToLoad > 0)
            {
                AppHost.MessageBox(Rocks.Resources.You_are_already_loading_items, Rocks.Resources.ContentEditorLoadItemsLoading_in_Progress, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (ContentModel.IsModified)
            {
                if (options.ForceReload)
                {
                    switch (AppHost.MessageBox("There are unsaved changes.\n\nDo you want to save?", "Information", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Cancel:
                            return;
                        case MessageBoxResult.Yes:
                            Save();
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                }
                else
                {
                    options.NewTab = true;
                    options.ForceReload = true;
                    AppHost.Windows.OpenContentEditor(itemUriList, options);
                    return;
                }
            }

            Pane.Caption = Rocks.Resources.Loading;
            ItemBreadcrumb.StartLoading();

            Editor.IsEnabled = false;
            try
            {
                ClearFieldControls(ContentModel);

                LoadItemsOperation = new LoadItemsOperation
                {
                    ItemsToLoad = itemUriList.Count,
                    Options = options
                };

                var pipeline = LoadItemsPipeline.Run().WithParameters(itemUriList, LoadItemsOperation);

                ContentModel = pipeline.ContentModel;

                if (itemUriList.Count == 0)
                {
                    LoadItemsCompleted();
                    return;
                }

                try
                {
                    foreach (var uri in itemUriList)
                    {
                        if (uri.Site.DataService.GetItemFieldsAsync(uri, GetItemFieldsCallback))
                        {
                            continue;
                        }

                        AppHost.Output.Log("Failed to load item: " + uri.ItemId);
                        LoadItemsOperation.ItemsToLoad = 0;
                        return;
                    }
                }
                catch
                {
                    AppHost.Output.Log("Failed to load items");
                    LoadItemsOperation.ItemsToLoad = 0;
                    throw;
                }
            }
            finally
            {
                Editor.IsEnabled = true;
            }
        }

        public void LoadItemsCompleted()
        {
            AppHost.Output.Log("LoadItemsCompleted");
            ContentModel.BuildModel();
            ContentModel.IsModified = false;

            RenderItems();

            AppHost.Selection.Track(Pane, ContentModel.Items.Select(item => new TrackSelectionDescriptor(item)));

            ActiveContext.RaiseContentModelChanged();

            ItemBreadcrumb.EndLoading();
        }

        public void Refresh()
        {
            ContentModel.GetChanges();
            LoadEditor();
            Ribbon.Update(this);
        }

        public void Save()
        {
            SaveContentModel();
        }

        [CanBeNull]
        public SaveItemPipeline SaveContentModel(bool postMacro = false)
        {
            var contentModel = ContentModel;
            if (contentModel.IsEmpty)
            {
                return null;
            }

            contentModel.GetChanges();

            return SaveItemPipeline.Run().WithParameters(contentModel, this, postMacro);
        }

        public void ShowRibbon()
        {
            var isVisible = AppHost.Settings.GetBool("ContentEditor", "ShowRibbon", false);

            Ribbon.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateContextualRibbon([NotNull] Field field)
        {
            var context = new ContentEditorFieldContext(this, field, this);

            Ribbon.RenderContextualTabs(field.Type, Brushes.Green, context);
        }

        public void Validate([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            var validateBar = AppearanceOptions.Skin.GetValidatorBar();
            if (validateBar == null)
            {
                return;
            }

            contentModel.GetChanges();
            validateBar.Update(contentModel);
        }

        private void ApplyFieldsFilter()
        {
            foreach (var field in ContentModel.Fields)
            {
                var name = field.Name;
                if (FieldManager.IsStandardField(field))
                {
                    name = name.Substring(2);
                }

                field.IsFiltered = !name.IsFilterMatch(Filter);
            }
        }

        private void CanGoBack([NotNull] object sender, [NotNull] CanExecuteRoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.CanExecute = Journal.CanGoBack;
        }

        private void CanGoForward([NotNull] object sender, [NotNull] CanExecuteRoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.CanExecute = Journal.CanGoForward;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(args, nameof(args));

            Loaded -= ControlLoaded;

            ShowRibbon();
            var context = new ContentEditorContext(this);
            Ribbon.Render(context);
            Editor.Focus();

            var contentModel = ContentModel;
            if (contentModel.IsEmpty)
            {
                ShowEmptyEditor();
            }
        }

        private void ControlUnloaded([NotNull] object sender, [NotNull] object window)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(window, nameof(window));

            if (!this.IsContainedIn(window))
            {
                return;
            }

            Notifications.Unloaded -= ControlUnloaded;

            if (ActiveContext.ActiveContentEditor == this)
            {
                ActiveContext.ActiveContentEditor = null;

                if (ActiveContext.Focused == Focused.ContentEditor)
                {
                    ActiveContext.Focused = Focused.None;
                }
            }
        }

        private void FieldChanged([NotNull] object sender, [NotNull] FieldUri fieldUri, [NotNull] string newValue)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(fieldUri, nameof(fieldUri));
            Debug.ArgumentNotNull(newValue, nameof(newValue));

            if (sender.Equals(this))
            {
                return;
            }

            foreach (var field in ContentModel.Fields)
            {
                if (field.Control == null)
                {
                    continue;
                }

                bool updated;
                if (field.Shared)
                {
                    updated = field.FieldUris.Any(f => (f.ItemVersionUri.ItemUri == fieldUri.ItemVersionUri.ItemUri) && (f.FieldId == fieldUri.FieldId));
                }
                else if (field.Unversioned)
                {
                    updated = field.FieldUris.Any(f => (f.ItemVersionUri.ItemUri == fieldUri.ItemVersionUri.ItemUri) && (f.ItemVersionUri.Language == fieldUri.ItemVersionUri.Language) && (f.FieldId == fieldUri.FieldId));
                }
                else
                {
                    updated = field.FieldUris.Any(f => f == fieldUri);
                }

                if (!updated)
                {
                    continue;
                }

                field.Control.SetValue(newValue);
            }

            if (fieldUri.FieldId == FieldIds.Icon && ContentModel.Fields.SelectMany(f => f.FieldUris).Any(f => f.ItemVersionUri.ItemUri == fieldUri.ItemVersionUri.ItemUri))
            {
                var control = AppearanceOptions.Skin.GetControl();

                var image = control.FindChild<Image>(@"QuickInfoIcon");
                if (image == null)
                {
                    return;
                }

                var path = @"/sitecore/shell/~/icon/" + newValue.Replace(@"16x16", @"16x16");

                var icon = new Icon(fieldUri.Site, path);

                image.Source = icon.GetSource();
            }
        }

        private void FieldsFilterChanged([NotNull] object sender, [NotNull] string filterText)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(filterText, nameof(filterText));

            Filter = filterText;

            var contentModel = ContentModel;

            contentModel.GetChanges();

            ApplyFieldsFilter();
            SetFieldVisibility(contentModel);

            LoadFieldControls(contentModel);
            RenderSkin(false, false);
        }

        private void FocusControl([NotNull] object sender, [NotNull] RoutedEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(args, nameof(args));

            ActiveContext.ActiveContentEditor = this;
            ActiveContext.Focused = Focused.ContentEditor;
        }

        private void FocusFieldControl([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var control = (Control)sender;
            control.Loaded -= FocusFieldControl;

            Keyboard.Focus(control);
        }

        private void GetItemFieldsCallback([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (LoadItemsOperation == null)
            {
                AppHost.Output.Log("LoadItemOperation is null");
                return;
            }

            if (LoadItemsOperation.ItemsToLoad <= 0)
            {
                AppHost.Output.Log("LoadItemOperation.ItemToLoad <= 0");
                return;
            }

            lock (SyncRoot)
            {
                ContentModel.Items.Add(item);
            }

            LoadItemsOperation.ItemsToLoad--;

            if (LoadItemsOperation.ItemsToLoad <= 0)
            {
                Dispatcher.Invoke(new Action(LoadItemsCompleted));
            }
        }

        IEnumerable<object> ISelectionTracking.GetSelectedObjects()
        {
            if (ContentModel.Items == null || ContentModel.IsEmpty)
            {
                return null;
            }

            return ContentModel.Items.Select(item => new TrackSelectionDescriptor(item));
        }

        private void GoBack([NotNull] object sender, [NotNull] ExecutedRoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var list = Journal.GoBack();
            if (list == null)
            {
                return;
            }

            var options = new LoadItemsOptions(false);

            LoadItems(list, options);
        }

        private void GoForward([NotNull] object sender, [NotNull] ExecutedRoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var list = Journal.GoForward();

            var options = new LoadItemsOptions(false);

            LoadItems(list, options);
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var contentModel = ContentModel;
            if (!contentModel.IsSingle)
            {
                return;
            }

            if (contentModel.FirstItem.Uri.ItemUri == itemUri)
            {
                Pane.Close();
            }
        }

        private void ItemModified([NotNull] object sender, [NotNull] ContentModel contentModel, bool isModified)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            if (contentModel == ContentModel)
            {
                Pane.SetModifiedFlag(isModified);
            }
        }

        private void ItemRenamed([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var contentModel = ContentModel;
            if (contentModel.IsEmpty)
            {
                return;
            }

            if (contentModel.IsMultiple)
            {
                return;
            }

            if (contentModel.FirstItem.Uri.ItemUri != itemUri)
            {
                return;
            }

            contentModel.FirstItem.Name = newName;
            RenderItems();
        }

        private void LoadAppearance()
        {
            AppearanceOptions = AppearanceManager.GetAppearanceOptions(this);
        }

        private void LoadEditor()
        {
            ClearFieldControls(ContentModel);
            Editor.Children.Clear();

            LoadAppearance();
            SetFieldVisibility(ContentModel);
            LoadFieldControls(ContentModel);
            LoadSkin();

            Validate(ContentModel);
        }

        private void LoadFieldControls([NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            foreach (var field in contentModel.Fields)
            {
                if (!field.IsVisible)
                {
                    field.Control = null;
                    continue;
                }

                var fieldType = field.Type;
                if (AppearanceOptions.RawValues)
                {
                    fieldType = @"__raw_textfield";
                }

                var fieldControl = FieldTypeManager.GetInstance(fieldType);

                if (!fieldControl.IsSupported(field))
                {
                    fieldType = @"__raw_textfield";
                    fieldControl = FieldTypeManager.GetInstance(fieldType);
                }

                field.ActualFieldType = fieldType;
                field.Control = fieldControl;
                field.Control.SetField(field);
                field.Control.SetValue(field.Value);
                field.Control.ValueModified += contentModel.ValueModified;
            }
        }

        private void LoadSkin()
        {
            Editor.Children.Add(AppearanceOptions.Skin.GetControl());

            var validateBar = AppearanceOptions.Skin.GetValidatorBar();
            if (validateBar != null)
            {
                validateBar.ContentEditor = this;
            }

            RenderSkin(true, true);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = (ContentEditorContext)GetContext();
            context.EventArgs = e;

            var commands = Rocks.Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
        }

        private void OpenMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            var contextMenu = ContextMenuExtensions.GetContextMenu(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = Menu;
            contextMenu.IsOpen = true;
        }

        private void Reload()
        {
            var command = new Reload();
            var context = GetContext();
            if (!command.CanExecute(context))
            {
                return;
            }

            AppHost.Usage.ReportCommand(command, context);
            command.Execute(context);
        }

        private void RenderItems()
        {
            Editor.IsEnabled = true;

            var contentModel = ContentModel;
            if (contentModel.IsEmpty)
            {
                ShowEmptyEditor();
                return;
            }

            UpdateCaption(contentModel);

            LoadEditor();

            ItemBreadcrumb.Update(contentModel);
            Ribbon.Update(this);

            if (LoadItemsOperation != null && LoadItemsOperation.Options.AddToJournal)
            {
                Journal.Push(contentModel.UriList);
            }
        }

        private void RenderSkin(bool focus, bool renderPanels)
        {
            ApplyFieldsFilter();

            var result = AppearanceOptions.Skin.RenderFields();

            if (renderPanels)
            {
                var panelRenderer = AppearanceOptions.Skin as ISupportsPanelRendering;
                if (panelRenderer != null)
                {
                    panelRenderer.RenderPanels(ContentModel, AppearanceOptions.Panels);
                }
            }

            if (focus && result != null)
            {
                if (result.IsLoaded)
                {
                    Keyboard.Focus(result);
                    result.Focus();
                }
                else
                {
                    result.Loaded += FocusFieldControl;
                }
            }
        }

        private void SaveSplitterPosition([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set("ContentEditor", "SplitterPosition", FilterWidth.Width.ToString());
        }

        private void SetFieldVisibility([NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            foreach (var field in contentModel.Fields)
            {
                var pipeline = SetFieldVisibilityPipeline.Run().WithParameters(contentModel, AppearanceOptions, field);

                field.IsVisible = pipeline.IsVisible;
            }
        }

        private void ShowEmptyEditor()
        {
            Editor.Children.Clear();
            Editor.Children.Add(new NoItems());
        }

        private void TemplateChanged([NotNull] object sender, [NotNull] ItemUri itemUri, [NotNull] ItemUri newTemplateUri)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(newTemplateUri, nameof(newTemplateUri));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var item = ContentModel.Items.FirstOrDefault(i => i.Uri.ItemUri == itemUri);
            if (item == null)
            {
                return;
            }

            if (AppHost.MessageBox(string.Format("The template of the item '{0}' has changed.\n\nDo you want to reload the item(s) in the Item Editor?", item.Name), Rocks.Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Reload();
            }
        }

        private void TemplateSaved([NotNull] object sender, [NotNull] ItemUri templateUri, [NotNull] string templateName)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));
            Debug.ArgumentNotNull(templateName, nameof(templateName));

            if (!ContentModel.Items.Any(item => item.Uri.DatabaseUri == templateUri.DatabaseUri && item.TemplateId == templateUri.ItemId))
            {
                return;
            }

            if (AppHost.MessageBox(string.Format(Rocks.Resources.ContentEditor_TemplateSaved_, templateName), Rocks.Resources.Confirmation, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Reload();
            }
        }

        private void UpdateCaption([NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            string caption;

            if (contentModel.IsMultiple)
            {
                caption = contentModel.UriList.Count + Rocks.Resources.ContentEditor_UpdateCaption__items;
            }
            else
            {
                caption = contentModel.FirstItem.Name;
            }

            caption += @".item"; // +contentModel.FirstItem.Uri.Site.Name;

            Pane.Caption = caption;
        }
    }
}
