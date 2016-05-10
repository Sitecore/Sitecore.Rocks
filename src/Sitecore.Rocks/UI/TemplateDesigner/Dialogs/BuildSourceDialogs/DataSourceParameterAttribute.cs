// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse, BaseTypeRequired(typeof(IDataSourceParameter))]
    public class DataSourceParameterAttribute : ExtensibilityAttribute
    {
        public DataSourceParameterAttribute([NotNull, Localizable(false)] string name, double priority)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            Name = name;
            Priority = priority;
        }

        [NotNull, Localizable(false)]
        public string Name { get; private set; }

        public double Priority { get; set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            DataSourceParameterManager.LoadType(type, this);
        }
    }
}
