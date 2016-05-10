// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Projects.Commands
{
    [Command(Submenu = ProjectsSubmenu.Name)]
    public class RebindFileItems : CommandBase
    {
        public RebindFileItems()
        {
            Text = "Bind File Based Items";
            Group = "Files";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            return GetProject(item.ItemUri) != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            if (AppHost.MessageBox("Are you sure you want to rebind any unbound files?", "Rebind File Items", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var project = GetProject(item.ItemUri);
                if (project == null)
                {
                    return;
                }

                var total = 0;
                var added = 0;

                foreach (var element in root.Elements())
                {
                    total++;
                    var id = element.GetAttributeValue("id");
                    var path = element.GetAttributeValue("path");

                    if (project.Contains(path))
                    {
                        continue;
                    }

                    var projectItem = new ProjectFileItem(project);
                    projectItem.Items.Add(new ItemUri(item.ItemUri.DatabaseUri, new ItemId(new Guid(id))));
                    projectItem.File = path;

                    project.Add(projectItem);
                    added++;
                }

                project.Save();
                AppHost.MessageBox(string.Format("Rebound {0} of {1} file-based item(s).", added, total), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            var itemUri = item.ItemUri;

            itemUri.Site.DataService.ExecuteAsync("Projects.RebindFileItems", callback, itemUri.DatabaseUri.DatabaseName.ToString(), itemUri.ItemId.ToString());
        }

        [CanBeNull]
        private Project GetProject([NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            foreach (var project in ProjectManager.Projects)
            {
                var site = project.Site;
                if (site == null)
                {
                    continue;
                }

                if (string.Compare(site.HostName, itemUri.Site.HostName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return project;
                }
            }

            return null;
        }
    }
}
