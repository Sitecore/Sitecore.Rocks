// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions
{
    [Pipeline(typeof(DefaultActionPipeline), 5000)]
    public class DesignTemplate : PipelineProcessor<DefaultActionPipeline>
    {
        protected override void Process(DefaultActionPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Handled)
            {
                return;
            }

            var context = pipeline.Context as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var items = context.Items.ToList();
            if (items.Count() != 1)
            {
                return;
            }

            var item = items.First() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            if (!IdManager.IsTemplate(item.TemplateId, "template"))
            {
                return;
            }

            if ((item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditTemplate) != DataServiceFeatureCapabilities.EditTemplate)
            {
                return;
            }

            AppHost.Windows.OpenTemplateDesigner(item.ItemUri);

            pipeline.Handled = true;
        }
    }
}
