// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.DateTimeExtensions;

namespace Sitecore.Rocks.UI.JobViewer.Commands
{
    [Command]
    public class SaveToFile : CommandBase
    {
        public SaveToFile()
        {
            Text = Resources.Save_Save_Save_to_File;
            Group = "Save";
            SortingValue = 100;
            Icon = new Icon("Resources/16x16/save.png");
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is JobViewerContext;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as JobViewerContext;
            if (context == null)
            {
                return;
            }

            var items = new List<object>();
            foreach (var item in context.JobViewer.ListView.Items)
            {
                items.Add(item);
            }

            var dialog = new SaveFileDialog
            {
                Title = Resources.CopyItemXmlDialog_SaveClick_Save_Item_Xml_to_File,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = @"Jobs",
                DefaultExt = @".xml",
                Filter = $@"{Resources.CopyItemXmlDialog_SaveClick_Xml_documents} (.xml)|*.xml|{Resources.All_files}|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
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

                output.WriteStartElement(@"jobs");

                foreach (var item in items)
                {
                    var jobItem = item as JobItem;
                    if (jobItem == null)
                    {
                        continue;
                    }

                    output.WriteStartElement(@"job");
                    output.WriteAttributeString(@"name", jobItem.Name);
                    output.WriteAttributeString(@"category", jobItem.Category);
                    output.WriteAttributeString(@"state", jobItem.State);
                    output.WriteAttributeString(@"queuetime", DateTimeExtensions.ToIsoDate(jobItem.QueueTime));
                    output.WriteAttributeString(@"progress", jobItem.Progress);
                    output.WriteAttributeString(@"failed", jobItem.Failed ? @"yes" : @"no");
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
