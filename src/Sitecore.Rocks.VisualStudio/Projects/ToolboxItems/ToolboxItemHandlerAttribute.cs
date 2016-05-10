// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ToolboxItemHandlerAttribute : ExtensibilityAttribute
    {
        public ToolboxItemHandlerAttribute()
        {
            Extension = string.Empty;
        }

        public ToolboxItemHandlerAttribute([NotNull, Localizable(false)] string extension)
        {
            Assert.ArgumentNotNull(extension, nameof(extension));

            Extension = extension;
        }

        [NotNull, Localizable(false)]
        public string Extension { get; private set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            ToolboxItemManager.LoadType(type, this);
        }
    }
}
