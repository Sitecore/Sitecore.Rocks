// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;

namespace Sitecore.Rocks.ContentTrees.Commands.WebAdministration
{
    public abstract class WorkerProcessBase : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (!Shell.WebAdministration.CanAdminister)
            {
                return false;
            }

            var selectedItems = context.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var item = selectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return false;
            }

            var webRootPath = item.Site.Connection.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                return false;
            }

            var application = Shell.WebAdministration.GetWebApplicationFromWebRootPath(webRootPath);
            if (application == null)
            {
                return false;
            }

            var appPoolName = application.ApplicationPoolName as string ?? string.Empty;
            if (string.IsNullOrEmpty(appPoolName))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return;
            }

            var webRootPath = item.Site.Connection.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                return;
            }

            var serverManager = Shell.WebAdministration.ServerManager;
            if (serverManager == null)
            {
                return;
            }

            var application = Shell.WebAdministration.GetWebApplicationFromWebRootPath(webRootPath);
            if (application == null)
            {
                return;
            }

            var appPoolName = application.ApplicationPoolName as string ?? string.Empty;
            if (string.IsNullOrEmpty(appPoolName))
            {
                return;
            }

            var list = new List<int>();

            foreach (var appPool in serverManager.ApplicationPools)
            {
                var n = appPool.Name as string ?? string.Empty;
                if (string.Compare(n, appPoolName, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                foreach (var workerProcess in appPool.WorkerProcesses)
                {
                    int workerProcessId = workerProcess.ProcessId;

                    list.Add(workerProcessId);
                }
            }

            if (!list.Any())
            {
                AppHost.MessageBox("The worker process could not be found.\n\nIt may not have been started yet.", Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Process(list);
        }

        protected abstract void Process(IEnumerable<int> workerProcessIdList);
    }
}
