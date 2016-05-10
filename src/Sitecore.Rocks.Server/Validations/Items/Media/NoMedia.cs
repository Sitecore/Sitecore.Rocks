// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Media
{
    [Validation("Media item with no media", "Media", ExecutePerLanguage = true)]
    public class NoMedia : ItemValidation
    {
        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            if (!item.Paths.IsMediaItem)
            {
                return;
            }

            var template = TemplateManager.GetTemplate(item.TemplateID, item.Database);
            if (template == null)
            {
                return;
            }

            var templateField = template.GetField("Blob");
            if (templateField == null)
            {
                return;
            }

            var field = item.Fields["Blob"];
            if (field == null)
            {
                output.Write(SeverityLevel.Warning, "Media item with no media", "The media item does have any media associated.", "Either delete the item or associate a file.", item);
                return;
            }

            if (!field.HasBlobStream)
            {
                output.Write(SeverityLevel.Warning, "Media item with no media", "The media item does have any media associated.", "Either delete the item or associate a file.", item);
            }
        }
    }
}
