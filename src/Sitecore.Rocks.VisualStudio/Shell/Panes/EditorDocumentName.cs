// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Shell.Panes
{
    public static class EditorDocumentName
    {
        [NotNull]
        public static string GetDocumentName([NotNull] string name, [NotNull] string extension = "")
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(extension, nameof(extension));
            /*
      name = name.Replace("/", " ");

      if (string.IsNullOrEmpty(extension))
      {
        return name;
      }

      if (!extension.StartsWith("."))
      {
        extension = "." + extension;
      }

      if (!name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
      {
        name += extension;
      }

      return name;
      */

            return name.GetSafeCodeIdentifier();
        }
    }
}
