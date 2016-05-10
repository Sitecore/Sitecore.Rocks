// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class ExportAsXml : CommandBase
    {
        public ExportAsXml()
        {
            Text = "Report as XML...";
            Group = "Export";
            SortingValue = 2400;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            return context.Validations.Any();
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            var fileName = AppHost.Settings.Get("Management\\Validation", "RecentFile", "validation.xml") as string ?? "validation.xml";

            var dialog = new SaveFileDialog
            {
                Title = "Export",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = @"Xml files|*.xml|All files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            ExportItems(dialog.FileName, context.Validations, context.ValidationViewer.Context.Site.Name);
        }

        private void ExportItems([NotNull] string fileName, [NotNull] IEnumerable<ValidationDescriptor> items, [NotNull] string siteName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(siteName, nameof(siteName));

            var hiddenItems = AppHost.Settings.Get("Management\\Validation\\Hidden", siteName, string.Empty) as string ?? string.Empty;

            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                var output = new XmlTextWriter(writer)
                {
                    Indentation = 2,
                    Formatting = Formatting.Indented,
                    IndentChar = ' '
                };

                output.WriteStartElement("items");

                foreach (var item in items.OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Title))
                {
                    var key = item.GetKey();
                    if (hiddenItems.Contains(key))
                    {
                        continue;
                    }

                    output.WriteStartElement("item");

                    output.WriteAttributeString("severity", item.Severity.ToString());

                    output.WriteElementString("title", item.Title);
                    output.WriteElementString("problem", item.Problem);
                    output.WriteElementString("solution", item.Solution);
                    output.WriteElementString("item", item.ItemPath);

                    output.WriteEndElement();
                }

                output.WriteEndElement();

                output.Flush();

                writer.Close();
            }
        }
    }
}
