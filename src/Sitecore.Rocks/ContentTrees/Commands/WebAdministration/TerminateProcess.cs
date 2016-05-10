// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.WebAdministration
{
    [Command(Submenu = WebServerSubmenu.Name), Feature(FeatureNames.WebServer)]
    public class TerminateProcess : WorkerProcessBase
    {
        public TerminateProcess()
        {
            Text = "Terminate IIS Worker Process";
            Group = "Process";
            SortingValue = 3000;
        }

        protected override void Process([NotNull] IEnumerable<int> workerProcessIdList)
        {
            Debug.ArgumentNotNull(workerProcessIdList, nameof(workerProcessIdList));

            foreach (var workerProcessId in workerProcessIdList)
            {
                foreach (var process in System.Diagnostics.Process.GetProcesses())
                {
                    if (process.Id != workerProcessId)
                    {
                        continue;
                    }

                    process.Kill();

                    AppHost.MessageBox("Process terminated.", Resources.Information, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
        }
    }
}
