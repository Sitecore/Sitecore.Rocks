// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors.Editors
{
    [RenderingEditor("{D47D6BFD-4F1F-4715-9FD1-5957EC0259F5}")]
    public partial class SearchDataSource : IRenderingEditor
    {
        private EditorBuilder editorBuilder;

        public SearchDataSource()
        {
            InitializeComponent();
            Loaded += ControlLoaded;
        }

        public RenderingItem RenderingItem { get; set; }

        public void Update()
        {
            editorBuilder.Update();
        }

        public void UpdateTarget()
        {
            editorBuilder.UpdateTarget();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            DataContext = RenderingItem;

            editorBuilder = new EditorBuilder(Properties, RenderingItem);

            editorBuilder.AddHeader("Configuration");
            editorBuilder.AddIdProperty("Item to Search From", "RootItemId");
            editorBuilder.AddIdProperty("Search Configuration", "SearchConfigItemId");
            editorBuilder.AddIdProperty("Facets Configuration", "FacetsRootItemId");

            editorBuilder.AddVerticalSpace();
            editorBuilder.AddHeader("Data Bindings");
            editorBuilder.AddBindingProperty("Selected Facets", "SelectedFacetsBinding");
            editorBuilder.AddBindingProperty("Item to Search From", "RootItemIdBinding");
        }
    }
}
