// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.DateTimeExtensions;

namespace Sitecore.Rocks.UI.LogViewer.Commands
{
    [Command]
    public class SaveToFile : CommandBase
    {
        public SaveToFile()
        {
            Text = Resources.Save_Save_Save_to_File;
            Group = "Save";
            SortingValue = 300;
            Icon = new Icon("Resources/16x16/save.png");
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is LogViewerContext;
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as LogViewerContext;
            if (context == null)
            {
                return;
            }

            var items = new List<object>();
            foreach (var item in context.LogViewer.ListView.Items)
            {
                items.Add(item);
            }

            var dialog = new SaveFileDialog
            {
                Title = Resources.CopyItemXmlDialog_SaveClick_Save_Item_Xml_to_File,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = @"Log",
                DefaultExt = @".xml",
                Filter = string.Format(@"{0} (.xml)|*.xml|{1}|*.*", Resources.CopyItemXmlDialog_SaveClick_Xml_documents, Resources.All_files)
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            using (var writer = new FileStream(dialog.FileName, FileMode.Create))
            {
                var output = new XmlTextWriter(writer, Encoding.UTF8)
                {
                    Indentation = 2,
                    IndentChar = ' ',
                    Formatting = Formatting.Indented
                };

                output.WriteStartElement(@"log");

                foreach (var item in items)
                {
                    var logItem = item as LogItem;
                    if (logItem == null)
                    {
                        continue;
                    }

                    output.WriteStartElement(@"item");
                    output.WriteAttributeString(@"category", logItem.Category);
                    output.WriteAttributeString(@"title", logItem.Title);
                    output.WriteAttributeString(@"state", DateTimeExtensions.ToIsoDate(logItem.PublishDate));

                    output.WriteValue(logItem.Description);

                    output.WriteEndElement();
                }

                output.WriteEndElement();

                output.Flush();
                writer.Flush();

                writer.Close();
            }
        }
    }
}
