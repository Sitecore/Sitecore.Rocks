// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors.Editors
{
    [RenderingEditor("{DAFAFFB8-74AF-4141-A96A-70B16834CEC6}")]
    public partial class PageCode : IRenderingEditor
    {
        private EditorBuilder editorBuilder;

        public PageCode()
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

            editorBuilder.AddHeader("Page Code");
            editorBuilder.AddText("The PageCode rendering initializes a SPEAK page by loading JavaScript files and CSS files.");
            editorBuilder.AddText("The PageCode rendering can also load the PageCode JavaScript which is a centralized piece of code like a CodeBehind in ASP.NET or a Forms file in WinForms. The PageCode file is optional.");
            editorBuilder.AddFileProperty("Page Code File:", "PageCodeScriptFileName");

            if (!string.IsNullOrEmpty(RenderingItem.ItemUri.Site.WebRootPath))
            {
                editorBuilder.AddActionButton(" Create PageCode File... ", CreatePageCode);
            }

            editorBuilder.AddVerticalSpace();
            editorBuilder.AddHeader("Server-side Page Code");
            editorBuilder.AddText("The PageCode rendering can also execute code on server at render time. Use this if you need server-side processing. The Type it points to must implement the IPageCode interface.");
            editorBuilder.AddStringProperty("Page Code Type Name:", "PageCodeTypeName");
        }

        private void CreatePageCode([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SaveFileDialog
            {
                Title = "Create PageCode file",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = @"PageCode",
                DefaultExt = @".js",
                Filter = @"JavaScript (.js)|*.js|All|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string text;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"Sitecore.Rocks.Resources.PageCode.js"))
            {
                var reader = new StreamReader(stream);
                text = reader.ReadToEnd();
            }

            var name = (Path.GetFileNameWithoutExtension(dialog.FileName) ?? string.Empty).GetSafeCodeIdentifier();
            text = text.Replace("$safeitemname$", name);
            AppHost.Files.WriteAllText(dialog.FileName, text, Encoding.UTF8);

            var fileName = dialog.FileName;
            if (fileName.StartsWith(RenderingItem.ItemUri.Site.WebRootPath, StringComparison.InvariantCultureIgnoreCase))
            {
                fileName = fileName.Mid(RenderingItem.ItemUri.Site.WebRootPath.Length);
                fileName = fileName.Replace("\\", "/");
            }

            var property = RenderingItem.DynamicProperties.FirstOrDefault(p => p.Name == "PageCodeScriptFileName");
            if (property != null)
            {
                property.Value = fileName;
            }

            UpdateTarget();

            AppHost.Files.OpenFile(dialog.FileName);
            var window = this.GetAncestorOrSelf<Window>();
            if (window != null)
            {
                window.Activate();
            }
        }
    }
}
