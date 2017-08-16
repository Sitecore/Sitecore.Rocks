// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.SecurityModel;

namespace Sitecore.Rocks.Server.Pipelines.DeleteItems
{
    [Pipeline(typeof(DeleteItemPipeline), 2000)]
    public class CheckCloneLinks : PipelineProcessor<DeleteItemPipeline>
    {
        protected override void Process(DeleteItemPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            try
            {
                using (new SecurityDisabler())
                {
                    ProcessClones(pipeline);
                }
            }
            catch (MissingMethodException)
            {
            }
        }

        private void GetItemClones([NotNull] Item item, [NotNull] List<Item> clones)
        {
            var links = Globals.LinkDatabase.GetReferrers(item);

            foreach (var link in links)
            {
                if (link.SourceFieldID != FieldIDs.Source)
                {
                    continue;
                }

                var clone = link.GetSourceItem();
                if (clone != null)
                {
                    clones.Add(clone);
                }
            }
        }

        private void ProcessClones([NotNull] DeleteItemPipeline pipeline)
        {
            var clones = new List<Item>();

            GetItemClones(pipeline.Item, clones);
            if (!clones.Any())
            {
                return;
            }

            foreach (var clone in clones)
            {
                if (pipeline.WillBeDeleted(clone))
                {
                    continue;
                }

                if (!clone.IsClone)
                {
                    continue;
                }

                pipeline.Output.WriteStartElement("item");
                pipeline.Output.WriteAttributeString("section", "Clones");
                pipeline.Output.WriteAttributeString("id", pipeline.Item.ID.ToString());
                pipeline.Output.WriteAttributeString("sourceid", clone.ID.ToString());
                pipeline.Output.WriteAttributeString("path", pipeline.Item.Paths.Path);
                pipeline.Output.WriteAttributeString("sourcepath", clone.Paths.Path);
                pipeline.Output.WriteAttributeString("level", "info");
                pipeline.Output.WriteValue(string.Format("Unclones the item \"{1}\" which is a clone of \"{0}\".", pipeline.Item.Paths.Path, clone.Paths.Path));
                pipeline.Output.WriteEndElement();

                if (!pipeline.DryRun)
                {
                    UncloneItem(clone);
                }
            }
        }

        private void UncloneItem([NotNull] Item clone)
        {
            var cloneItem = new CloneItem(clone);
            cloneItem.Unclone();
        }
    }
}
