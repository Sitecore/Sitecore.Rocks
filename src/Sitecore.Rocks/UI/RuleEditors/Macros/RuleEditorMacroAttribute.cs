// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.RuleEditors.Macros
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse]
    public class RuleEditorMacroAttribute : ExtensibilityAttribute
    {
        public RuleEditorMacroAttribute([NotNull, Localizable(false)] string macroName)
        {
            MacroName = macroName;
            Assert.ArgumentNotNull(macroName, nameof(macroName));
        }

        [NotNull]
        public string MacroName { get; private set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            RuleEditorMacroManager.LoadType(type, this);
        }
    }
}
