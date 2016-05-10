// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Commands
{
    [Command]
    public class ExportAsCsv : CommandBase
    {
        public ExportAsCsv()
        {
            Text = "Export as CSV...";
            Group = "Export";
            SortingValue = 2410;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Validations.Any())
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ValidationContext;
            if (context == null)
            {
                return;
            }

            var fileName = AppHost.Settings.Get("Management\\Validation", "RecentFile", "validation.cvs") as string ?? "validation.cvs";

            fileName = Path.ChangeExtension(fileName, @"csv");

            var dialog = new SaveFileDialog
            {
                Title = "Export",
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = fileName,
                Filter = @"Csv files|*.csv|All files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            ExportTables(dialog.FileName, context.Validations, context.ValidationViewer.Context.Site.Name);
        }

        private void ExportColumns([NotNull] StreamWriter output)
        {
            Debug.ArgumentNotNull(output, nameof(output));

            output.WriteLine("type,title,problem,solution,item");
            output.WriteLine();
        }

        private void ExportRows([NotNull] StreamWriter output, [NotNull] IEnumerable<ValidationDescriptor> items, [NotNull] string siteName)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(siteName, nameof(siteName));

            var hiddenItems = AppHost.Settings.Get("Management\\Validation\\Hidden", siteName, string.Empty) as string ?? string.Empty;

            foreach (var item in items.OrderBy(i0 => i0.Category).ThenBy(i1 => i1.Title))
            {
                var key = item.GetKey();
                if (hiddenItems.Contains(key))
                {
                    continue;
                }

                var state = item.Severity.ToString().Replace(",", " ").Replace("\n", " ").Replace("\r", " ");
                var title = item.Title.Replace(",", " ").Replace("\n", " ").Replace("\r", " ");
                var problem = item.Problem.Replace(",", " ").Replace("\n", " ").Replace("\r", " ");
                var solution = item.Solution.Replace(",", " ").Replace("\n", " ").Replace("\r", " ");
                var itemPath = item.ItemPath.Replace(",", " ").Replace("\n", " ").Replace("\r", " ");

                output.WriteLine("{0},{1},{2},{3},{4}", state, title, problem, solution, itemPath);
            }
        }

        private void ExportTables([NotNull] string fileName, [NotNull] IEnumerable<ValidationDescriptor> items, [NotNull] string siteName)
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(siteName, nameof(siteName));

            using (var output = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                ExportColumns(output);

                ExportRows(output, items, siteName);

                output.Flush();

                output.Close();
            }
        }
    }
}
