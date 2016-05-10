// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.WebAdministration
{
    [Command(Submenu = WebServerSubmenu.Name)]
    public class AttachToProcess : WorkerProcessBase
    {
        public AttachToProcess()
        {
            Text = "Attach to IIS Worker Process";
            Group = "Debug";
            SortingValue = 1000;
        }

        protected override void Process([NotNull] IEnumerable<int> workerProcessIdList)
        {
            Debug.ArgumentNotNull(workerProcessIdList, nameof(workerProcessIdList));

            foreach (var workerProcessId in workerProcessIdList)
            {
                var debuggedProcesses = SitecorePackage.Instance.Dte.Debugger.DebuggedProcesses;
                if (debuggedProcesses != null)
                {
                    if (debuggedProcesses.OfType<Process>().Any(p => p.ProcessID == workerProcessId))
                    {
                        continue;
                    }
                }

                var localProcesses = SitecorePackage.Instance.Dte.Debugger.LocalProcesses;
                if (localProcesses == null)
                {
                    continue;
                }

                var process = localProcesses.OfType<Process>().FirstOrDefault(p => p.ProcessID == workerProcessId);
                if (process != null)
                {
                    try
                    {
                        process.Attach();
                    }
                    catch (COMException ex)
                    {
                        AppHost.MessageBox(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return;
                }
            }
        }
    }
}
