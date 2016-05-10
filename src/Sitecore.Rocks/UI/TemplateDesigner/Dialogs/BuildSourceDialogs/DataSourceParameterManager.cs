// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    [ExtensibilityInitialization(PreInit = "Clear")]
    public static class DataSourceParameterManager
    {
        private static readonly List<DataSourceParameterDescriptor> dataSourceParameters = new List<DataSourceParameterDescriptor>();

        [NotNull]
        public static IEnumerable<DataSourceParameterDescriptor> DataSourceParameters
        {
            get { return dataSourceParameters.OrderBy(i => i.Priority).ThenBy(i => i.Header); }
        }

        [UsedImplicitly]
        public static void Clear()
        {
            dataSourceParameters.Clear();
        }

        public static void LoadType([NotNull] Type type, [NotNull] DataSourceParameterAttribute attribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(attribute, nameof(attribute));

            var instance = Activator.CreateInstance(type) as IDataSourceParameter;
            if (instance == null)
            {
                throw new InvalidOperationException(string.Format("Type \"{0}\" must implement IDataSourceParameter", type.FullName));
            }

            var mediaSkin = new DataSourceParameterDescriptor
            {
                Instance = instance,
                Priority = attribute.Priority,
                Header = attribute.Name
            };

            dataSourceParameters.Add(mediaSkin);
        }

        public class DataSourceParameterDescriptor
        {
            [NotNull]
            public string Header { get; set; }

            [NotNull]
            public IDataSourceParameter Instance { get; set; }

            public double Priority { get; set; }
        }
    }
}
