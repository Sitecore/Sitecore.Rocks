// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Shell.Environment
{
    public class FeatureHost
    {
        private Dictionary<string, FeatureDescriptor> _featureDescriptors;

        [NotNull]
        public virtual IEnumerable<FeatureDescriptor> Features
        {
            get { return _featureDescriptors.Values; }
        }

        [NotNull]
        public virtual FeatureDescriptor Add([NotNull] string featureName)
        {
            Assert.ArgumentNotNull(featureName, nameof(featureName));

            _featureDescriptors[featureName] = new FeatureDescriptor(featureName, string.Empty);

            return new FeatureDescriptor(featureName, string.Empty);
        }

        public void Clear()
        {
            _featureDescriptors = LoadFeatures();
        }

        public virtual bool IsEnabled([NotNull] string featureName)
        {
            Assert.ArgumentNotNull(featureName, nameof(featureName));

            FeatureDescriptor featureDescriptor;
            if (_featureDescriptors.TryGetValue(featureName, out featureDescriptor))
            {
                return featureDescriptor.IsEnabled;
            }

            return false;
        }

        [NotNull]
        protected Dictionary<string, FeatureDescriptor> LoadFeatures()
        {
            var result = new Dictionary<string, FeatureDescriptor>();

            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            foreach (var fileName in Directory.GetFiles(folder, "Sitecore.Rocks.*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;

                if (AppHost.Plugins.IsServerComponent(name))
                {
                    continue;
                }

                if (name == "Sitecore.Rocks")
                {
                    continue;
                }

                if (name == Constants.SitecoreRocksVisualStudio)
                {
                    continue;
                }

                if (name == "Sitecore.Rocks.Plugins")
                {
                    continue;
                }

                name = name.Mid(15);

                var feature = new FeatureDescriptor(name, fileName);

                result[name] = feature;
            }

            return result;
        }

        [NotNull]
        internal Dictionary<string, bool> GetFeatureStates()
        {
            var result = new Dictionary<string, bool>();

            foreach (var featureDescriptor in Features)
            {
                result[featureDescriptor.Name] = featureDescriptor.IsEnabled;
            }

            return result;
        }

        internal virtual bool IsFeatureAssemblyEnabled([NotNull] string file)
        {
            Debug.ArgumentNotNull(file, nameof(file));

            var feature = _featureDescriptors.Values.FirstOrDefault(f => string.Compare(f.FileName, file, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (feature == null)
            {
                return true;
            }

            return feature.IsEnabled;
        }
    }
}
