// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class ValueItem
    {
        public ValueItem([NotNull] string name, [NotNull] string value)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(value, nameof(value));

            Name = name;
            Value = value;
        }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public string Value { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
