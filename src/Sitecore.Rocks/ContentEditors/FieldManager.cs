// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public static class FieldManager
    {
        private const string RegistryPath = "ContentEditor\\Fields";

        private static readonly List<ItemId> hiddenContentFields;

        private static readonly List<ItemId> visibleStandardFields;

        static FieldManager()
        {
            visibleStandardFields = new List<ItemId>();
            hiddenContentFields = new List<ItemId>();

            LoadVisibleStandardFields();
            LoadHiddenContentFields();
        }

        public static bool GetContentFieldVisibility([NotNull] ItemId templateFieldId)
        {
            Assert.ArgumentNotNull(templateFieldId, nameof(templateFieldId));

            return !hiddenContentFields.Contains(templateFieldId);
        }

        public static bool GetStandardFieldVisibility([NotNull] ItemId templateFieldId)
        {
            Assert.ArgumentNotNull(templateFieldId, nameof(templateFieldId));

            return visibleStandardFields.Contains(templateFieldId);
        }

        public static bool IsStandardField([NotNull] Field field)
        {
            Assert.ArgumentNotNull(field, nameof(field));

            return field.Name.StartsWith(@"__");
        }

        public static void SetContentFieldVisibility([NotNull] ItemId templateFieldId, bool isVisible)
        {
            Assert.ArgumentNotNull(templateFieldId, nameof(templateFieldId));

            if (!isVisible)
            {
                if (!hiddenContentFields.Contains(templateFieldId))
                {
                    hiddenContentFields.Add(templateFieldId);
                }
            }
            else
            {
                hiddenContentFields.Remove(templateFieldId);
            }

            SaveHiddenContentFields();
        }

        public static void SetStandardFieldVisibility([NotNull] ItemId templateFieldId, bool isVisible)
        {
            Assert.ArgumentNotNull(templateFieldId, nameof(templateFieldId));

            if (isVisible)
            {
                if (!visibleStandardFields.Contains(templateFieldId))
                {
                    visibleStandardFields.Add(templateFieldId);
                }
            }
            else
            {
                visibleStandardFields.Remove(templateFieldId);
            }

            SaveVisibleStandardFields();
        }

        private static void LoadHiddenContentFields()
        {
            var s = AppHost.Settings.Get(RegistryPath, "Hidden", string.Empty) as string ?? string.Empty;

            foreach (var part in s.Split(','))
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                hiddenContentFields.Add(new ItemId(new Guid(part)));
            }
        }

        private static void LoadVisibleStandardFields()
        {
            var s = AppHost.Settings.Get(RegistryPath, "Visible", string.Empty) as string ?? string.Empty;

            foreach (var part in s.Split(','))
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                visibleStandardFields.Add(new ItemId(new Guid(part)));
            }
        }

        private static void SaveHiddenContentFields()
        {
            var sb = new StringBuilder();

            foreach (var itemId in hiddenContentFields)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                }

                sb.Append(itemId);
            }

            AppHost.Settings.Set(RegistryPath, "Hidden", sb.ToString());
        }

        private static void SaveVisibleStandardFields()
        {
            var sb = new StringBuilder();

            foreach (var itemId in visibleStandardFields)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                }

                sb.Append(itemId);
            }

            AppHost.Settings.Set(RegistryPath, "Visible", sb.ToString());
        }
    }
}
