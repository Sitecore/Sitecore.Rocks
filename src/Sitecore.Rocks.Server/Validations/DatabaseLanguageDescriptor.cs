// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;

namespace Sitecore.Rocks.Server.Validations
{
    public class DatabaseLanguageDescriptor
    {
        public DatabaseLanguageDescriptor([NotNull] Database database, [NotNull] Language language)
        {
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(language, nameof(language));

            Database = database;
            Language = language;
        }

        [NotNull]
        public Database Database { get; private set; }

        [NotNull]
        public Language Language { get; private set; }
    }
}
