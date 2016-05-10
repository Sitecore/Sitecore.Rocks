// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public static class FieldControlManager
    {
        private static readonly List<Tuple<IFieldControl, FileSystemWatcher>> Watchers = new List<Tuple<IFieldControl, FileSystemWatcher>>();

        public static void AddWatcher([NotNull] IFieldControl field, [NotNull] string value, [NotNull] string caption, [NotNull] string fileType, [NotNull] Action<string> edited)
        {
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(value, nameof(value));
            Assert.ArgumentNotNull(caption, nameof(caption));
            Assert.ArgumentNotNull(fileType, nameof(fileType));
            Assert.ArgumentNotNull(edited, nameof(edited));

            var watcher = AppHost.Files.EditText(value, caption, fileType, edited);

            Watchers.Add(new Tuple<IFieldControl, FileSystemWatcher>(field, watcher.Item2));
        }

        public static void RemoveWatcher([NotNull] IFieldControl field)
        {
            Assert.ArgumentNotNull(field, nameof(field));

            for (var i = Watchers.Count - 1; i >= 0; i--)
            {
                var tuple = Watchers[i];
                if (tuple.Item1 != field)
                {
                    continue;
                }

                tuple.Item2.EnableRaisingEvents = false;
                tuple.Item2.Dispose();

                Watchers.Remove(tuple);
            }
        }
    }
}
