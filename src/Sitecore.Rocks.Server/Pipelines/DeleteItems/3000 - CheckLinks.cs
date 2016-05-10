// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Pipelines.DeleteItems
{
    [Pipeline(typeof(DeleteItemPipeline), 3000)]
    public class CheckLinks : PipelineProcessor<DeleteItemPipeline>
    {
        protected override void Process(DeleteItemPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            using (new SecurityDisabler())
            {
                FindLinks(pipeline, pipeline.Item);
            }
        }

        private void FindLinks([NotNull] DeleteItemPipeline pipeline, [NotNull] Item item)
        {
            var links = Globals.LinkDatabase.GetReferrers(item);
            if (links.Length <= 0)
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

                var sourceFieldId = itemLink.SourceFieldID;
                if (ID.IsNullOrEmpty(sourceFieldId))
                {
                    continue;
                }

                var field = sourceItem.Fields[sourceFieldId];
                if (field == null)
                {
                    continue;
                }

                var customField = FieldTypeManager.GetField(field);
                if (customField == null)
                {
                    continue;
                }

                pipeline.Output.WriteStartElement("item");
                pipeline.Output.WriteAttributeString("section", "Links");
                pipeline.Output.WriteAttributeString("id", item.ID.ToString());
                pipeline.Output.WriteAttributeString("sourceid", itemLink.SourceItemID.ToString());
                pipeline.Output.WriteAttributeString("path", item.Paths.Path);
                pipeline.Output.WriteAttributeString("sourcepath", sourceItem.Paths.Path);
                pipeline.Output.WriteAttributeString("level", "warning");
                pipeline.Output.WriteValue(string.Format("Removes the link in the \"{2}\" field from \"{0}\" to \"{1}\".", sourceItem.Paths.Path, item.Paths.Path, field.Name));
                pipeline.Output.WriteEndElement();

                if (!pipeline.DryRun)
                {
                    RemoveLink(sourceItem, itemLink);
                }
            }
        }

        private void RemoveLink(Item sourceItem, ItemLink itemLink)
        {
            var versions = sourceItem.Versions.GetVersions();

            foreach (var version in versions)
            {
                var field = version.Fields[itemLink.SourceFieldID];
                if (field == null)
                {
                    continue;
                }

                var customField = FieldTypeManager.GetField(field);
                if (customField == null)
                {
                    continue;
                }

                version.Editing.BeginEdit();
                customField.RemoveLink(itemLink);
                version.Editing.EndEdit();
            }
        }
    }
}
