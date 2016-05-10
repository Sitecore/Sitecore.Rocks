// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Windows;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    [Pipeline(typeof(TroubleshooterPipeline), 900)]
    public class DevelopmentServer : PipelineProcessor<TroubleshooterPipeline>
    {
        protected override void Process(TroubleshooterPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Exception == null)
            {
                return;
            }

            if (!(pipeline.Exception is EndpointNotFoundException))
            {
                return;
            }

            if (pipeline.Silent)
            {
                return;
            }

            var serviceEndpoint = pipeline.Endpoint;
            if (serviceEndpoint == null)
            {
                return;
            }

            var endpointAddress = serviceEndpoint.Address;
            if (endpointAddress == null)
            {
                return;
            }

            var uri = endpointAddress.Uri;
            if (uri == null)
            {
                return;
            }

            var host = uri.Host;
            if (string.Compare(host, @"localhost", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }

            if (pipeline.StartedDevelopmentServer)
            {
                return;
            }

            pipeline.StartedDevelopmentServer = true;

            var solution = SitecorePackage.Instance.Dte.Solution;
            if (solution == null)
            {
                return;
            }

            var projects = solution.Projects;
            if (projects == null)
            {
                return;
            }

            if (projects.Count == 0)
            {
                switch (AppHost.MessageBox("The web site does not respond.\n\nIt looks like the server address is a local web site. If the web site is running on ASP.NET Development Server, the server may not have been started yet.\n\nDo you want to retry?", Resources.Information, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        pipeline.Cancelled = false;
                        pipeline.Retry = true;
                        pipeline.Abort();
                        return;

                    case MessageBoxResult.Cancel:
                        pipeline.Cancelled = true;
                        pipeline.Retry = false;
                        pipeline.Abort();
                        break;
                }

                return;
            }

            switch (AppHost.MessageBox("The web site does not respond.\n\nIt looks like the server address is a local web site. If the web site is running on ASP.NET Development Server, the server may not have been started yet.\n\nDo you want to Start Without Debugging to launch the server and retry?", Resources.Information, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    if (!StartDevelopmentServer())
                    {
                        break;
                    }

                    pipeline.Cancelled = false;
                    pipeline.Retry = true;
                    pipeline.Abort();
                    return;

                case MessageBoxResult.Cancel:
                    pipeline.Cancelled = true;
                    pipeline.Retry = false;
                    pipeline.Abort();
                    break;
            }
        }

        private bool StartDevelopmentServer()
        {
            try
            {
                SitecorePackage.Instance.Dte.ExecuteCommand("Debug.StartWithoutDebugging");
            }
            catch (COMException ex)
            {
                AppHost.MessageBox(string.Format("Failed to start the ASP.NET Development Server.\n\n{0}", ex.Message), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
        }
    }
}
