// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Pipelines.DeleteItems
{
    [Pipeline(typeof(DeleteItemPipeline), 1000)]
    public class CheckTemplateLinks : PipelineProcessor<DeleteItemPipeline>
    {
        protected override void Process(DeleteItemPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            using (new SecurityDisabler())
            {
                FindTemplates(pipeline, pipeline.Item);
            }
        }

        private void FindTemplates([NotNull] DeleteItemPipeline pipeline, [NotNull] Item item)
        {
            if (item.TemplateID != TemplateIDs.Template)
            {
                return;
            }

            var links = Globals.LinkDatabase.GetReferrers(item);
            if (links.Length <= 0 || IsStandardValuesLink(item, links))
            {
                return;
            }

            foreach (var itemLink in links)
            {
                var sourceItem = item.Database.GetItem(itemLink.SourceItemID);
                if (sourceItem == null)
                {
                    continue;
                }

                if (pipeline.WillBeDeleted(sourceItem))
                {
                    continue;
                }

                pipeline.Output.WriteStartElement("item");
                pipeline.Output.WriteAttributeString("section", "Templates");
                pipeline.Output.WriteAttributeString("id", item.ID.ToString());
                pipeline.Output.WriteAttributeString("sourceid", itemLink.SourceItemID.ToString());
                pipeline.Output.WriteAttributeString("path", item.Paths.Path);
                pipeline.Output.WriteAttributeString("sourcepath", sourceItem.Paths.Path);
                pipeline.Output.WriteAttributeString("level", "error");
                pipeline.Output.WriteValue(string.Format("Makes the item \"{0}\" invalid because the template \"{1}\" will be removed.", sourceItem.Paths.Path, item.Paths.Path));
                pipeline.Output.WriteEndElement();
            }
        }

        private bool IsStandardValuesLink([NotNull] Item item, [NotNull] ItemLink[] links)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(links, nameof(links));

            if (links.Length != 1)
            {
                return false;
            }

            var link = links[0];

            TemplateItem template = item;

            var standardValues = template.StandardValues;
            if (standardValues != null)
            {
                return link.SourceItemID == standardValues.ID || link.TargetItemID == standardValues.ID;
            }

            return false;
        }
    }
}
