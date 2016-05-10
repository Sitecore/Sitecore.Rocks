// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class StringJournal : Journal<string>
    {
        public StringJournal([NotNull] string settingsKey, [NotNull] string settingsSubkey)
        {
            Assert.ArgumentNotNull(settingsKey, nameof(settingsKey));
            Assert.ArgumentNotNull(settingsSubkey, nameof(settingsSubkey));

            SettingsKey = settingsKey;
            SettingsSubkey = settingsSubkey;

            Load();
        }

        [NotNull]
        public string SettingsKey { get; }

        [NotNull]
        public string SettingsSubkey { get; }

        public void Load()
        {
            var list = AppHost.Settings.GetString(SettingsKey, SettingsSubkey, string.Empty);

            foreach (var text in list.Split('|'))
            {
                if (!string.IsNullOrEmpty(text))
                {
                    Push(text);
                }
            }
        }

        public void Save()
        {
            var sb = new StringBuilder();
            foreach (var s in GetHistory().Reverse())
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append(s);
            }

            AppHost.Settings.Set(SettingsKey, SettingsSubkey, sb.ToString());
        }
    }
}
