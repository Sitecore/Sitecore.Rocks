// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CommandIdAttribute : Attribute
    {
        public CommandIdAttribute(int commandId, [NotNull] Type expectedCommandContext)
        {
            Assert.ArgumentNotNull(expectedCommandContext, nameof(expectedCommandContext));

            CommandId = commandId;
            ExpectedCommandContext = expectedCommandContext;
        }

        public int CommandId { get; private set; }

        [NotNull]
        public Type ExpectedCommandContext { get; set; }

        [CanBeNull]
        public string Group { get; set; }

        [Localizable(false), CanBeNull]
        public string Icon { get; set; }

        public int Priority { get; set; }

        [CanBeNull]
        public string Text { get; set; }

        [CanBeNull]
        public string ToolBar { get; set; }
    }
}
