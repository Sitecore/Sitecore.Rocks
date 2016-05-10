// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors.Editors
{
    [RenderingEditor("{57F86E9A-1844-45CE-BF8A-62900AE17A92}")]
    public partial class TextBoxEditor : IRenderingEditor
    {
        private EditorBuilder editorBuilder;

        public TextBoxEditor()
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
            editorBuilder.AddHeader("Apperance");
            editorBuilder.AddStringProperty("Watermark", "Watermark");

            editorBuilder.AddVerticalSpace();
            editorBuilder.AddHeader("Data Bindings");
            editorBuilder.AddBindingProperty("Text", "TextBinding");
        }
    }
}
