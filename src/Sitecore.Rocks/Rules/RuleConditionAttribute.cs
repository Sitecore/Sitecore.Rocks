// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Rules
{
    [Localizable(false), AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), MeansImplicitUse]
    public class RuleConditionAttribute : ExtensibilityAttribute
    {
        public RuleConditionAttribute([NotNull] string displayText, [NotNull] string category)
        {
            Assert.ArgumentNotNull(displayText, nameof(displayText));
            Assert.ArgumentNotNull(category, nameof(category));

            DisplayText = displayText;
            Category = category;
        }

        [NotNull]
        public string Category { get; private set; }

        [NotNull]
        public string DisplayText { get; private set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            RuleManager.LoadType(type, this);
        }
    }
}
