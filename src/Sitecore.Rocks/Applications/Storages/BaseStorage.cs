// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Applications.Storages
{
    public abstract class BaseStorage
    {
        public abstract void Delete([NotNull] string path);

        public abstract void Delete([NotNull] string path, [NotNull] string key);

        [NotNull]
        public abstract IEnumerable<string> GetKeys([NotNull] string path);

        [NotNull]
        public abstract IEnumerable<string> GetSubPaths([NotNull] string path);

        [CanBeNull]
        public abstract object Read([NotNull] string path, [NotNull] string key, [CanBeNull] object defaultValue);

        public abstract void Write([NotNull] string path, [NotNull] string key, [CanBeNull] object value);
    }
}
