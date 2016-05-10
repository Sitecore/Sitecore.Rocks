// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.Environment;

namespace Sitecore.Rocks.UI.QueryAnalyzers.Commands
{
    [Command]
    public class ExportAsCsv : CommandBase
    {
        public ExportAsCsv()
        {
            Text = Resources.ExportAsCsv_ExportAsCsv_Export_as_Csv___;
            Group = "Export";
            SortingValue = 2410;
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

            fileName = Path.ChangeExtension(fileName, @"csv");

            var dialog = new SaveFileDialog
            {
                Title = Resources.Export_Execute_Export,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = @"Csv files|*.csv|All files|*.*"
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

        public void ExportTables([NotNull] StreamWriter output, [NotNull] List<QueryAnalyzer.ResultDataTable> dataTables)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(dataTables, nameof(dataTables));

            foreach (var dataTable in dataTables)
            {
                ExportColumns(output, dataTable);

                ExportRows(output, dataTable);
            }
        }

        private void ExportColumns([NotNull] StreamWriter output, [NotNull] QueryAnalyzer.ResultDataTable dataTable)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));

            var first = true;
            foreach (var column in dataTable.Columns)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Write(',');
                }

                var dataColumn = (DataColumn)column;

                output.Write(dataColumn.ColumnName);
            }

            output.WriteLine();
        }

        private void ExportRows([NotNull] StreamWriter output, [NotNull] QueryAnalyzer.ResultDataTable dataTable)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));

            foreach (QueryAnalyzer.ResultDataRow dataRow in dataTable.Rows)
            {
                var first = true;

                foreach (var t in dataRow.ItemArray)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        output.Write(',');
                    }

                    var value = t;
                    if (value != null)
                    {
                        output.Write("\"" + value.ToString().Replace("\"", "\"\"") + "\"");
                    }
                }

                output.WriteLine();
            }
        }

        private void ExportTables([NotNull] string fileName, [NotNull] List<QueryAnalyzer.ResultDataTable> dataTables)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(dataTables, nameof(dataTables));

            switch (AppHost.Files.GetFileStatus(fileName, FileAccess.Write, FileShare.None))
            {
                case FileStatus.AccessDenied:
                    AppHost.Output.Log(string.Format("Access to the path \"{0}\" is denied.", fileName));
                    AppHost.MessageBox(string.Format("Access to the path \"{0}\" is denied.\n\nPlease make that you have read permission to this path.", fileName), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new SilentException();

                case FileStatus.UsedByAnotherProcess:
                    AppHost.Output.Log(string.Format("The file \"{0}\" is being used by another process.", fileName));
                    AppHost.MessageBox(string.Format("The file \"{0}\" is being used by another process.\n\nPlease close any applications that might access this file and try again.", fileName), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new SilentException();

                case FileStatus.FileNotFound:
                    break;
            }

            using (var output = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                ExportTables(output, dataTables);

                output.Flush();

                output.Close();
            }
        }
    }
}
