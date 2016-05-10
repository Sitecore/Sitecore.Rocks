// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility
{
    public class FeatureDescriptor
    {
        private bool _isEnabled = true;

        public FeatureDescriptor([NotNull] string name, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            Name = name;
            FileName = fileName;

            IsEnabled = AppHost.Settings.GetBool("Features", name, true);
        }

        [NotNull]
        public string FileName { get; private set; }

        public bool IsEnabled
        {
            get { return _isEnabled; }

            set
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;
                AppHost.Settings.SetBool("Features", Name, value);
            }
        }

        [NotNull]
        public string Name { get; }
    }
}
