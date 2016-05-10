// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors.Panels;
using Sitecore.Rocks.ContentEditors.Skins;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Appearances
{
    public class AppearanceOptions
    {
        private ISkin skin;

        private string skinName;

        [NotNull]
        public ContentEditor ContentEditor { get; set; }

        public bool FieldDisplayTitles { get; set; }

        public bool FieldInformation { get; set; }

        [NotNull]
        public IEnumerable<IPanel> Panels { get; set; }

        public bool RawValues { get; set; }

        [NotNull]
        public ISkin Skin
        {
            get
            {
                if (skin == null)
                {
                    skin = SkinManager.GetInstance(SkinName) ?? SkinManager.GetDefaultInstance();
                    skin.ContentEditor = ContentEditor;
                    skin.ContentModel = ContentEditor.ContentModel;
                }

                return skin;
            }
        }

        [NotNull]
        public string SkinName
        {
            get { return skinName ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                skinName = value;
                skin = null;
            }
        }

        public bool StandardFields { get; set; }
    }
}
