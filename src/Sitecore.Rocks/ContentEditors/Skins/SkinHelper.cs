// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Skins
{
    public static class SkinHelper
    {
        [NotNull]
        public static string GetHeader([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            if (contentModel.IsMultiple)
            {
                return contentModel.Items.Count + Resources.SkinHelper_GetHeader__items;
            }

            return contentModel.FirstItem.Name;
        }

        public static void RenderLanguageAndVersion([NotNull] ComboBox languageComboBox, [NotNull] ComboBox versionComboBox, [NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(languageComboBox, nameof(languageComboBox));
            Assert.ArgumentNotNull(versionComboBox, nameof(versionComboBox));
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            if (contentModel.IsMultiple)
            {
                versionComboBox.Visibility = Visibility.Collapsed;
                languageComboBox.Visibility = Visibility.Collapsed;
                return;
            }

            RenderLanguages(languageComboBox, contentModel.FirstItem.Languages, contentModel.FirstItem.Uri.Language);
            RenderVersions(versionComboBox, contentModel.FirstItem.Versions, contentModel.FirstItem.Uri.Version);
        }

        public static void RenderLanguages([NotNull] ComboBox languageComboBox, [NotNull] List<string> languages, [NotNull] Language language)
        {
            Assert.ArgumentNotNull(languageComboBox, nameof(languageComboBox));
            Assert.ArgumentNotNull(languages, nameof(languages));
            Assert.ArgumentNotNull(language, nameof(language));

            languageComboBox.Visibility = Visibility.Visible;
            languageComboBox.Items.Clear();

            foreach (var l in languages)
            {
                languageComboBox.Items.Add(l);
            }

            languageComboBox.SelectedValue = language;
        }

        public static void RenderVersions([NotNull] ComboBox versionComboBox, [NotNull] List<int> versions, [NotNull] Version version)
        {
            Assert.ArgumentNotNull(versionComboBox, nameof(versionComboBox));
            Assert.ArgumentNotNull(versions, nameof(versions));
            Assert.ArgumentNotNull(version, nameof(version));

            versionComboBox.Visibility = Visibility.Visible;
            versionComboBox.Items.Clear();

            for (var n = versions.Count - 1; n >= 0; n--)
            {
                var v = versions[n];
                versionComboBox.Items.Add(v);
            }

            versionComboBox.SelectedValue = version;
        }
    }
}
