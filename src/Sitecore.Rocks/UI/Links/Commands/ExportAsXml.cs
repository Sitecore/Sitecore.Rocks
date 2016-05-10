// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Links.Commands
{
    [Command]
    public class ExportAsXml : CommandBase
    {
        public ExportAsXml()
        {
            Text = Resources.Export_Export_Export_as_XML___;
            Group = "Export";
            SortingValue = 2400;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is LinksContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LinksContext;
            if (context == null)
            {
                return;
            }

            var fileName = AppHost.Settings.Get("Links", "RecentFile", "links.xml") as string ?? string.Empty;

            var dialog = new SaveFileDialog
            {
                Title = Resources.Export_Execute_Export,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = Path.GetFileName(fileName),
                Filter = @"Xml files|*.xml|All files|*.*"
            };

            if (fileName.IndexOf(Path.DirectorySeparatorChar) >= 0)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(fileName);
            }

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            fileName = dialog.FileName;

            AppHost.Settings.Set("Links", "RecentFile", fileName);

            Export(context, fileName, context.LinkTab.TreeView);
        }

        private void Export([NotNull] LinksContext context, [NotNull] string fileName, [NotNull] System.Windows.Controls.TreeView treeView)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(treeView, nameof(treeView));

            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                var output = new XmlTextWriter(writer)
                {
                    Indentation = 2,
                    Formatting = Formatting.Indented,
                    IndentChar = ' '
                };

                output.WriteStartElement(@"Links");
                output.WriteAttributeString(@"id", context.LinkTab.ItemUri.ItemId.ToString());

                Export(output, treeView.Items);

                output.WriteEndElement();

                output.Flush();

                writer.Close();
            }
        }

        private void Export([NotNull] XmlTextWriter output, [NotNull] ItemCollection items)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                var itemHeader = treeViewItem.Tag as ItemHeader;
                if (itemHeader != null)
                {
                    output.WriteStartElement(@"Item");

                    output.WriteAttributeString(@"name", itemHeader.Name);
                    output.WriteAttributeString(@"id", itemHeader.ItemId.ToString());
                    output.WriteAttributeString(@"path", itemHeader.Path);
                    output.WriteAttributeString(@"template", itemHeader.TemplateName);
                    output.WriteAttributeString(@"templateid", itemHeader.TemplateId.ToString());

                    output.WriteEndElement();
                }

                var tag = treeViewItem.Tag as string;
                if (tag == null)
                {
                    continue;
                }

                var name = tag.ToPascalCase();

                output.WriteStartElement(name);
                Export(output, treeViewItem.Items);
                output.WriteEndElement();
            }
        }
    }
}
