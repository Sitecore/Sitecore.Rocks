// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class ExportAsXml : CommandBase
    {
        public ExportAsXml()
        {
            Text = Resources.Export_Export_Export_as_XML___;
            Group = "Export";
            SortingValue = 2405;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return false;
            }

            if (context.QueryAnalyzer.DataGrids.Count == 0)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as QueryAnalyzerContext;
            if (context == null)
            {
                return;
            }

            var fileName = context.QueryAnalyzer.ScriptFileName;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = AppHost.Settings.Get("QueryAnalyzer", "RecentFile", "script.txt") as string ?? string.Empty;
            }

            fileName = Path.ChangeExtension(fileName, @"xml");

            var dialog = new SaveFileDialog
            {
                Title = Resources.Export_Execute_Export,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = @"Xml files|*.xml|All files|*.*"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            foreach (var dataGrid in context.QueryAnalyzer.DataGrids)
            {
                dataGrid.CommitEdit();
            }

            ExportTables(dialog.FileName, context.QueryAnalyzer.DataTables);
        }

        private void ExportTables([NotNull] string fileName, [NotNull] List<QueryAnalyzer.ResultDataTable> dataTables)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(dataTables, nameof(dataTables));

            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                var output = new XmlTextWriter(writer)
                {
                    Indentation = 2,
                    Formatting = Formatting.Indented,
                    IndentChar = ' '
                };

                var exporter = new QueryExporter();

                exporter.ExportTables(output, dataTables, string.Empty);

                output.Flush();

                writer.Close();
            }
        }
    }
}
