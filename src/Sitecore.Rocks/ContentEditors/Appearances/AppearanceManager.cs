// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Panels;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Appearances
{
    public static class AppearanceManager
    {
        private static readonly List<AppearanceDescriptor> descriptors = new List<AppearanceDescriptor>();

        [NotNull, UsedImplicitly]
        public static IEnumerable<AppearanceDescriptor> AppearanceDescriptors
        {
            get { return descriptors; }
        }

        public static void Add([NotNull] AppearanceDescriptor appearanceDescriptor)
        {
            Assert.ArgumentNotNull(appearanceDescriptor, nameof(appearanceDescriptor));

            if (appearanceDescriptor.ItemId != ItemId.Empty)
            {
                var descriptor = descriptors.FirstOrDefault(d => d.ItemId == appearanceDescriptor.ItemId);
                if (descriptor != null)
                {
                    descriptors.Remove(descriptor);
                }
            }

            if (appearanceDescriptor.TemplateId != ItemId.Empty)
            {
                var descriptor = descriptors.FirstOrDefault(d => d.TemplateId == appearanceDescriptor.TemplateId);
                if (descriptor != null)
                {
                    descriptors.Remove(descriptor);
                }
            }

            if (appearanceDescriptor.IsDefault)
            {
                var descriptor = descriptors.FirstOrDefault(d => d.IsDefault);
                if (descriptor != null)
                {
                    descriptors.Remove(descriptor);
                }
            }

            descriptors.Add(appearanceDescriptor);
        }

        [NotNull]
        public static AppearanceDescriptor GetAppearanceDescriptor([NotNull] ContentModel contentModel)
        {
            Assert.ArgumentNotNull(contentModel, nameof(contentModel));

            if (!contentModel.IsSingle)
            {
                return GetDefaultAppearanceDescriptor();
            }

            var item = contentModel.FirstItem;

            var descriptor = descriptors.FirstOrDefault(d => d.ItemId == item.ItemUri.ItemId);
            if (descriptor != null)
            {
                return descriptor;
            }

            descriptor = descriptors.FirstOrDefault(d => d.TemplateId == item.TemplateId);
            if (descriptor != null)
            {
                return descriptor;
            }

            descriptor = descriptors.FirstOrDefault(d => d.IsDefault);
            if (descriptor != null)
            {
                return descriptor;
            }

            return GetDefaultAppearanceDescriptor();
        }

        [NotNull]
        public static AppearanceOptions GetAppearanceOptions([NotNull] ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            var descriptor = GetAppearanceDescriptor(contentEditor.ContentModel);

            return descriptor.GetAppearanceOptions(contentEditor);
        }

        [NotNull]
        public static AppearanceDescriptor GetDefaultAppearanceDescriptor()
        {
            var result = new AppearanceDescriptor();

            var panels = PanelManager.Panels.Where(descriptor => descriptor.IsVisible).ToList();

            panels.Sort((d1, d2) => d1.Priority.CompareTo(d2.Priority));

            foreach (var panel in panels)
            {
                result.Panels.Add(panel);
            }

            return result;
        }

        [NotNull]
        public static AppearanceOptions GetDefaultAppearanceOptions([NotNull] ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            var appearance = new AppearanceOptions
            {
                SkinName = AppHost.Settings.Options.Skin ?? string.Empty,
                Panels = new List<IPanel>(),
                ContentEditor = contentEditor
            };

            return appearance;
        }

        public static void Remove([NotNull] AppearanceDescriptor appearanceDescriptor)
        {
            Assert.ArgumentNotNull(appearanceDescriptor, nameof(appearanceDescriptor));

            descriptors.Remove(appearanceDescriptor);
        }

        public class AppearanceDescriptor
        {
            public AppearanceDescriptor()
            {
                Panels = new List<PanelManager.PanelDescriptor>();
                ItemId = ItemId.Empty;
                TemplateId = ItemId.Empty;
                IsDefault = false;
            }

            public bool IsDefault { get; set; }

            [NotNull]
            public ItemId ItemId { get; set; }

            [NotNull]
            public List<PanelManager.PanelDescriptor> Panels { get; set; }

            [NotNull]
            public string SkinName { get; set; }

            [NotNull]
            public ItemId TemplateId { get; set; }

            [NotNull]
            public AppearanceOptions GetAppearanceOptions([NotNull] ContentEditor contentEditor)
            {
                Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

                var options = AppHost.Settings.Options;

                var skinName = SkinName;
                if (string.IsNullOrEmpty(skinName))
                {
                    skinName = options.Skin ?? string.Empty;
                }

                var result = new AppearanceOptions
                {
                    ContentEditor = contentEditor,
                    SkinName = skinName,
                    StandardFields = options.ShowStandardFields,
                    RawValues = options.ShowRawValues,
                    FieldInformation = options.ShowFieldInformation,
                    FieldDisplayTitles = options.ShowFieldDisplayTitles
                };

                var panelContext = new PanelContext(result.Skin, contentEditor.ContentModel);

                result.Panels = Panels.Select(d => d.Panel).Where(p => p.CanRender(panelContext));

                return result;
            }
        }
    }
}
