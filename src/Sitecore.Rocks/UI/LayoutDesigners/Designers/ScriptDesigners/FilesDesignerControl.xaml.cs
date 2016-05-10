// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.ScriptDesigners
{
    public partial class FilesDesignerControl : IDesigner
    {
        public FilesDesignerControl([NotNull] DeviceModel device, [NotNull] IEnumerable<string> fileNames)
        {
            Assert.ArgumentNotNull(device, nameof(device));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            InitializeComponent();

            Device = device;
            FileNames = fileNames;

            RenderFiles();
        }

        [NotNull]
        public DeviceModel Device { get; private set; }

        [NotNull]
        public IEnumerable<string> FileNames { get; }

        public void Activate()
        {
        }

        public void Close()
        {
        }

        private void RenderFiles()
        {
            ScriptStackPanel.Children.Clear();

            foreach (var f in FileNames.OrderBy(Path.GetFileName))
            {
                var fileName = f;

                var textBlock = new TextBlock();

                var inline = new Run
                {
                    Text = Path.GetFileName(fileName)
                };

                var hyperlink = new Hyperlink(inline)
                {
                    Style = FindResource("ResourceLink") as Style
                };

                hyperlink.Click += (sender, args) => AppHost.Files.OpenFile(fileName);

                textBlock.Inlines.Add(hyperlink);
                textBlock.ToolTip = fileName;

                ScriptStackPanel.Children.Add(textBlock);
            }
        }
    }
}
