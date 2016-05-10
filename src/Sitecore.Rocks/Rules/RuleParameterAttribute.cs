// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Rules
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true), MeansImplicitUse]
    public class RuleParameterAttribute : ExtensibilityAttribute
    {
        public RuleParameterAttribute([Localizable(false), NotNull] string editorName)
        {
            Assert.ArgumentNotNull(editorName, nameof(editorName));

            EditorName = editorName;
        }

        public string EditorName { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            RuleManager.LoadType(type, this);
        }
    }
}
