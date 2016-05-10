// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.Management.ManagementItems.PipelineViewers.Dialogs;

namespace Sitecore.Rocks.UI.Management.ManagementItems.PipelineViewers.Commands
{
    [Command]
    public class CreatePipelineProcessor : CommandBase
    {
        public CreatePipelineProcessor()
        {
            Text = Resources.CreateIncludeFile_CreateIncludeFile_Create_Include_File___;
            Group = "Add";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as PipelineViewerContext;
            if (context == null)
            {
                return false;
            }

            var viewer = context.PipelineViewer;

            if (viewer.PipelineList.SelectedItem == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as PipelineViewerContext;
            if (context == null)
            {
                return;
            }

            var viewer = context.PipelineViewer;

            var pipeline = viewer.PipelineList.SelectedItem as PipelineViewer.Pipeline;
            if (pipeline == null)
            {
                return;
            }

            var patch = string.Empty;
            var selectedIndex = viewer.ProcessorList.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var list = viewer.ProcessorList.ItemsSource as List<PipelineViewer.Processor>;
                if (list != null)
                {
                    var processor = list[selectedIndex];
                    patch = string.Format(" patch:before=\"processor[@type='{0}']\"", processor.TypeName);
                }
            }

            var d = new TypeAndAssemblyDialog(context.PipelineViewer.Context.Site);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var typeName = d.ClassName.Text;
            if (!string.IsNullOrEmpty(d.AssemblyName.Text))
            {
                typeName += ", " + d.AssemblyName.Text;
            }

            var text = string.Format("<configuration xmlns:patch=\"http://www.sitecore.net/xmlconfig/\">\n" + "  <sitecore>\n" + "    <pipelines>\n" + "      <{0}>\n" + "        <processor type=\"{1}\"{2} />\n" + "      </{0}>\n" + "    </pipelines>\n" + "  </sitecore>\n" + "</configuration>", pipeline.Name, typeName, patch);

            string folder;
            if (!string.IsNullOrEmpty(viewer.Context.Site.WebRootPath))
            {
                folder = Path.Combine(viewer.Context.Site.WebRootPath, "App_Config\\Include");
            }
            else
            {
                folder = AppHost.Settings.Get("PipelineViewer", "LastFolder", string.Empty) as string ?? string.Empty;
            }

            var fileName = Path.Combine(folder, string.Format("{0}.config", d.ClassName.Text));

            var dialog = new SaveFileDialog
            {
                Title = Resources.MediaManager_DownloadAttachment_Download,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = @"Include files|*.config"
            };

            if (!string.IsNullOrEmpty(fileName))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(fileName);
            }

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            AppHost.Settings.Set("PipelineViewer", "LastFolder", Path.GetDirectoryName(dialog.FileName));

            AppHost.Files.WriteAllText(dialog.FileName, text, Encoding.UTF8);

            AppHost.Files.OpenFile(dialog.FileName);

            viewer.Refresh();
        }
    }
}
