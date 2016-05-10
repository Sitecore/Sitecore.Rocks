// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors.Editors
{
    [RenderingEditor("{7717EB6C-9F90-4C58-826D-5E87722A0318}")]
    public partial class TextPropertyEditor : IRenderingEditor
    {
        private EditorBuilder editorBuilder;

        public TextPropertyEditor()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        public RenderingItem RenderingItem { get; set; }

        public void Update()
        {
            editorBuilder.Update();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            DataContext = RenderingItem;

            editorBuilder = new EditorBuilder(Properties, RenderingItem);

            editorBuilder.AddHeader("Properties");
            editorBuilder.AddId();
            editorBuilder.AddPlaceHolder();

            editorBuilder.AddVerticalSpace();
            editorBuilder.AddHeader("Data");
            editorBuilder.AddStringProperty("Default Text", "Text");

            editorBuilder.AddVerticalSpace();
            editorBuilder.AddHeader("Data Bindings");
            editorBuilder.AddBindingProperty("Text", "TextBinding");
        }
    }
}
