// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ServiceModel;
using System.Windows;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    [Pipeline(typeof(TroubleshooterPipeline), 1000)]
    public class EndpointNotFound : PipelineProcessor<TroubleshooterPipeline>
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

            pipeline.Abort();

            pipeline.Cancelled = true;
            pipeline.Retry = false;

            if (pipeline.Silent)
            {
                return;
            }

            if (pipeline.DataService is HardRockWebService && !pipeline.UpdatedServerComponents)
            {
                pipeline.UpdatedServerComponents = true;

                switch (AppHost.MessageBox("The web site does not respond.\n\nThe connection is using the Hard Rock Web Server which may not yet have been installed on the server.\n\nDo you want to update server components?", Resources.Information, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        var d = new UpdateServerComponentsDialog();

                        d.Initialize(pipeline.DataService, string.Empty, string.Empty, null, null);
                        AppHost.Shell.ShowDialog(d);

                        pipeline.Cancelled = false;
                        pipeline.Retry = true;
                        return;
                    case MessageBoxResult.Cancel:
                        pipeline.Cancelled = true;
                        pipeline.Retry = false;
                        return;
                }
            }

            if (AppHost.MessageBox("The web site does not respond.\n\nPlease make sure the web site is up and running.\n\nDo you want to retry?", Resources.Information, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                pipeline.Cancelled = false;
                pipeline.Retry = true;
            }
        }
    }
}
