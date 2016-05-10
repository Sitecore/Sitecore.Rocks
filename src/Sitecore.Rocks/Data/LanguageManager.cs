// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public static class LanguageManager
    {
        [NotNull]
        public static Language CurrentLanguage
        {
            get { return AppHost.Globals.CurrentLanguage; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                AppHost.Globals.CurrentLanguage = value;
            }
        }
    }
}
