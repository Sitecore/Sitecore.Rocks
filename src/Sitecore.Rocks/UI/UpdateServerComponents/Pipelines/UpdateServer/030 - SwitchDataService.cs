// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.UI.UpdateServerComponents.Pipelines.UpdateServer
{
    [Pipeline(typeof(UpdateServerPipeline), 3000)]
    public class SwitchDataService : PipelineProcessor<UpdateServerPipeline>
    {
        protected override void Process(UpdateServerPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Site == null)
            {
                return;
            }

            if (pipeline.DataService.GetType() != typeof(OldWebService))
            {
                return;
            }

            foreach (var updateInfo in pipeline.Updates)
            {
                if (!updateInfo.IsChecked)
                {
                    continue;
                }

                if (updateInfo.Name != @"Sitecore.Rocks.Server")
                {
                    continue;
                }

                if (AppHost.MessageBox(Resources.SwitchDataService_Process_, Resources.SwitchDataService_Process_Switch_Data_Service, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }

                pipeline.Site.SetDataServiceName(HardRockWebService.Name);
                pipeline.DataService = pipeline.Site.DataService;

                ConnectionManager.Save();
                return;
            }
        }
    }
}
