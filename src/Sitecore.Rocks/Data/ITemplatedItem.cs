// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface ITemplatedItem : IItem
    {
        [NotNull]
        ItemId TemplateId { get; }

        [NotNull]
        string TemplateName { get; }
    }
}
