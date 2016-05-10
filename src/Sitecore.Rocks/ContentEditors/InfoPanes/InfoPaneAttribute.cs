// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentEditors.InfoPanes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse, BaseTypeRequired(typeof(IInfoPane))]
    public class InfoPaneAttribute : ExtensibilityAttribute
    {
        public InfoPaneAttribute([NotNull, Localizable(false)] string paneName, double priority)
        {
            Assert.ArgumentNotNull(paneName, nameof(paneName));

            PaneName = paneName;
            Priority = priority;
        }

        [NotNull, Localizable(false)]
        public string PaneName { get; private set; }

        public double Priority { get; set; }

        public override void Initialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            InfoPaneManager.LoadType(type, this);
        }
    }
}
