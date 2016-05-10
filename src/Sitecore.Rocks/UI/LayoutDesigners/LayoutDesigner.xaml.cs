// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Text;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Overlays;

namespace Sitecore.Rocks.UI.LayoutDesigners
{
    public partial class LayoutDesigner : ISavable, IHasEditorPane, ISelectionTracking
    {
        public const string DragIdentifier = "Sitecore.Rendering";

        private bool isLoadingView;

        private bool modified;

        public LayoutDesigner()
        {
            InitializeComponent();

            var view = GetView();
            SetDesignerView(view);
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; private set; }

        [NotNull]
        public IEnumerable<FieldUri> FieldUris { get; private set; }

        [NotNull]
        public ILayoutDesignerView LayoutDesignerView { get; private set; }

        public bool Modified
        {
            get { return modified; }

            set
            {
                if (isLoadingView)
                {
                    return;
                }

                if (modified == value)
                {
                    return;
                }

                modified = value;
                Pane.SetModifiedFlag(value);
            }
        }

        public IEditorPane Pane { get; set; }

        [NotNull]
        public string SpeakCoreVersion { get; set; } = string.Empty;

        [NotNull]
        public string SpeakCoreVersionId { get; private set; } = string.Empty;

        public void AddPlaceholder()
        {
            LayoutDesignerView.AddPlaceholder(DatabaseUri);
        }

        public void AddRendering()
        {
            var renderingContainer = LayoutDesignerView.GetRenderingContainer();
            if (renderingContainer == null)
            {
                return;
            }

            var dialog = new SelectRenderingsDialog
            {
                DatabaseUri = DatabaseUri,
                RenderingContainer = renderingContainer,
                SpeakCoreVersionId = SpeakCoreVersionId
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            Action<IEnumerable<RenderingItem>> getSelectedRenderings = delegate(IEnumerable<RenderingItem> selectedRenderings)
            {
                foreach (var rendering in selectedRenderings.Reverse())
                {
                    LayoutDesignerView.AddRendering(rendering);
                }
            };

            dialog.GetSelectedRenderings(getSelectedRenderings);
        }

        [NotNull]
        public OverlayCanvas GetCanvas()
        {
            return Canvas;
        }

        public IEnumerable<object> GetSelectedObjects()
        {
            return LayoutDesignerView.GetSelectedObjects();
        }

        public void Initialize([NotNull] string paneCaption, [NotNull] IEnumerable<FieldUri> fieldUris, [NotNull] string layoutDefinition)
        {
            Assert.ArgumentNotNull(paneCaption, nameof(paneCaption));
            Assert.ArgumentNotNull(fieldUris, nameof(fieldUris));
            Assert.ArgumentNotNull(layoutDefinition, nameof(layoutDefinition));

            Pane.Caption = string.Format(Rocks.Resources.LayoutDesigner_Initialize__0____Layout, paneCaption);
            FieldUris = fieldUris;
            DatabaseUri = fieldUris.First().DatabaseUri;

            ShowRibbon();

            LoadLayout(layoutDefinition, () =>
            {
                DetermineSpeakVersion();

                var context = new LayoutDesignerContext(this, null, Enumerable.Empty<LayoutDesignerItem>());
                Ribbon.Render(context);
                Ribbon.Update(LayoutDesignerView);

                Modified = false;
            });
        }

        public void Reload()
        {
            var view = GetView();
            SetView(view);
        }

        public void Save()
        {
            var layoutDefinition = SaveLayout();

            if (!CheckSpeakCoreVersion())
            {
                return;
            }

            AppHost.Output.Log(@"Saving layout: " + layoutDefinition);

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                AppHost.Output.Log(@"Saved layout: " + response);

                foreach (var fieldUri in FieldUris)
                {
                    Notifications.RaiseFieldChanged(this, fieldUri, layoutDefinition);
                }
            };

            foreach (var fieldUri in FieldUris)
            {
                fieldUri.Site.DataService.ExecuteAsync("Layouts.SaveLayout", completed, fieldUri.DatabaseName.ToString(), fieldUri.ItemId.ToString(), fieldUri.Language.ToString(), fieldUri.Version.ToString(), fieldUri.FieldId.ToString(), layoutDefinition);
            }

            Modified = false;
        }

        public void SetView([NotNull] ILayoutDesignerView layoutDesignerView)
        {
            Assert.ArgumentNotNull(layoutDesignerView, nameof(layoutDesignerView));

            AppHost.Settings.SetString("LayoutDesigner", "View", layoutDesignerView.GetType().FullName);

            isLoadingView = true;
            try
            {
                var layout = SaveLayout();

                SetDesignerView(layoutDesignerView);

                Ribbon.Render(new LayoutDesignerContext(this, null, Enumerable.Empty<LayoutDesignerItem>()));

                LoadLayout(layout, () => { });

                layoutDesignerView.UpdateTracking();
            }
            finally
            {
                isLoadingView = false;
            }
        }

        public void ShowRibbon()
        {
            var isVisible = AppHost.Settings.GetBool("LayoutDesigner", "ShowRibbon", false);

            Ribbon.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            Toolbar.Visibility = !isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateRibbon([NotNull] IContextProvider contextProvider)
        {
            Assert.ArgumentNotNull(contextProvider, nameof(contextProvider));

            Ribbon.Update(contextProvider);
        }

        [NotNull]
        internal ILayoutDesignerView GetView()
        {
            var typeName = AppHost.Settings.GetString("LayoutDesigner", "View", typeof(LayoutListView).FullName);

            var type = Type.GetType(typeName) ?? typeof(LayoutListView);

            var view = Activator.CreateInstance(type, this) as ILayoutDesignerView ?? new LayoutListView(this);

            return view;
        }

        private void AddPlaceholder([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AddPlaceholder();
        }

        private void AddRendering([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AddRendering();
        }

        private bool CheckSpeakCoreVersion()
        {
            DetermineSpeakVersion();
            if (string.IsNullOrEmpty(SpeakCoreVersionId))
            {
                return true;
            }

            var renderingContainer = LayoutDesignerView.GetRenderingContainer();
            if (renderingContainer == null)
            {
                return true;
            }

            foreach (var rendering in renderingContainer.Renderings)
            {
                if (string.IsNullOrEmpty(rendering.SpeakCoreVersionId) || rendering.SpeakCoreVersionId == SpeakCoreVersionId)
                {
                    continue;
                }

                var renderingName = rendering.GetControlId();
                if (string.IsNullOrEmpty(renderingName))
                {
                    renderingName = rendering.Name;
                }

                if (AppHost.MessageBox($"The rendering '{renderingName}' requires SPEAK version '{rendering.SpeakCoreVersion}', but the PageCode specifies SPEAK version '{SpeakCoreVersion}'.\n\nAre you sure you want to continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return false;
                }
            }

            return true;
        }

        private void DetermineSpeakVersion()
        {
            var renderingContainer = LayoutDesignerView.GetRenderingContainer();
            if (renderingContainer == null)
            {
                return;
            }

            foreach (var rendering in renderingContainer.Renderings)
            {
                if (rendering.ItemId != "{DAFAFFB8-74AF-4141-A96A-70B16834CEC6}")
                {
                    continue;
                }

                var parameter = new UrlString(rendering.Parameters);
                var version = HttpUtility.UrlDecode(parameter.Parameters["SpeakCoreVersion"] ?? string.Empty);

                if (!string.IsNullOrEmpty(version))
                {
                    SpeakCoreVersionId = version;

                    var property = rendering.DynamicProperties.FirstOrDefault(p => p.Name == "SpeakCoreVersion");
                    if (property != null)
                    {
                        SpeakCoreVersion = property.Value != null ? property.Value.ToString() : string.Empty;
                    }
                }

                return;
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                AddRendering(sender, e);
                e.Handled = true;
            }
        }

        private void LoadLayout([NotNull] string layout, [NotNull] Action loaded)
        {
            Debug.ArgumentNotNull(layout, nameof(layout));
            Debug.ArgumentNotNull(loaded, nameof(loaded));

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    loaded();
                    return;
                }

                var layoutDefinition = response.ToXElement();
                if (layoutDefinition == null)
                {
                    loaded();
                    return;
                }

                isLoadingView = true;
                try
                {
                    LayoutDesignerView.LoadLayout(DatabaseUri, layoutDefinition);
                    loaded();
                }
                finally
                {
                    isLoadingView = false;
                }
            };

            AppHost.Server.GetLayout(layout, DatabaseUri, completed);
        }

        private void OpenMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LayoutDesignerView.OpenMenu(sender);
        }

        private void OpenPropertyWindow([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Windows.OpenPropertyWindow();
        }

        [NotNull]
        private string SaveLayout()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Indentation = 2,
                Formatting = Formatting.Indented,
                Namespaces = false
            };

            output.WriteStartElement(@"r");

            LayoutDesignerView.SaveLayout(output);

            output.WriteEndElement();

            return writer.ToString();
        }

        private void SetDesignerView([NotNull] ILayoutDesignerView layoutDesignerView)
        {
            Debug.ArgumentNotNull(layoutDesignerView, nameof(layoutDesignerView));

            LayoutDesignerView = layoutDesignerView;
            LayoutDesignerViewBorder.Child = LayoutDesignerView as UIElement;

            layoutDesignerView.Modified += (sender, args) => Modified = true;
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }
    }
}
