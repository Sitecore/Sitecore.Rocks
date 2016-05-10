// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command, ShellMenuCommand(CommandIds.SearchReferences)]
    public class SearchReferences : SolutionCommand
    {
        public SearchReferences()
        {
            Text = "Search for References to this File";
        }

        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var site = project.Site;
            if (site == null)
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            IsVisible = true;
            return true;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (selectedItems.Count != 1)
            {
                return;
            }

            var items = GetProjectItems(selectedItems);

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var fileName = Path.GetFileName(item.Name);

            var databaseUri = new DatabaseUri(site, DatabaseName.Master);

            var queryAnalyzer = AppHost.Windows.Factory.OpenQueryAnalyzer(databaseUri);
            if (queryAnalyzer == null)
            {
                Trace.TraceError("Could not open query analyzer");
                return;
            }

            var script = string.Format("use master;\nselect * from //*[contains(@Path, '{0}')]", fileName);

            queryAnalyzer.SetScript(script);
            queryAnalyzer.Execute();
        }
    }
}
