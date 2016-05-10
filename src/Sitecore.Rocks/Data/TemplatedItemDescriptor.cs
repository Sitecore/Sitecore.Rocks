// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class TemplatedItemDescriptor : ItemDescriptor, ITemplatedItem
    {
        public TemplatedItemDescriptor([NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] ItemId templateId, [NotNull] string templateName) : base(itemUri, name)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(templateId, nameof(templateId));
            Assert.ArgumentNotNull(templateName, nameof(templateName));

            TemplateId = templateId;
            TemplateName = templateName;
        }

        public ItemId TemplateId { get; }

        public string TemplateName { get; }
    }
}
