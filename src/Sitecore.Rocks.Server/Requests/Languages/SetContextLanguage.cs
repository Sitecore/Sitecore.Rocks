// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Languages
{
    public class SetContextLanguage
    {
        [NotNull]
        public string Execute([NotNull] string languageName)
        {
            Assert.ArgumentNotNull(languageName, nameof(languageName));

            Context.Language = LanguageManager.GetLanguage(languageName);

            return string.Empty;
        }
    }
}
