// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Fluent;
using Sitecore.Rocks.ContentEditors.Skins;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Toolbars
{
    [ToolbarElement(typeof(ContentEditorContext), 9000, "View", "Views", ElementType = RibbonElementType.Gallery)]
    public class SkinGallery : IToolbarElement, IToolbarGalleryFactory
    {
        public void Create(InRibbonGallery gallery, object parameter)
        {
            gallery.Header = "Views";
            gallery.Icon = new Icon("Resources/32x32/Plus.png").GetSource();
            gallery.LargeIcon = new Icon("Resources/32x32/Plus.png").GetSource();

            gallery.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
            {
                var button = args.AddedItems.OfType<SkinButton>().FirstOrDefault();
                if (button == null)
                {
                    return;
                }

                SetSkin(parameter, button);
            };

            LoadViewGallery(gallery);
        }

        private void LoadViewGallery(InRibbonGallery gallery)
        {
            var renderings = new List<SkinButton>();

            foreach (var item in SkinManager.Types)
            {
                var button = new SkinButton
                {
                    Label = item.Key,
                    Tag = item.Key
                };

                renderings.Add(button);
            }

            renderings = renderings.OrderBy(i => i.Label).ToList();

            gallery.ItemsSource = renderings;
        }

        private void SetSkin(object parameter, SkinButton button)
        {
            var context = parameter as ContentEditorContext;
            if (context == null)
            {
                return;
            }

            var contentModel = context.ContentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return;
            }

            var skinName = button.Tag as string;
            if (skinName == null)
            {
                return;
            }

            var options = AppHost.Settings.Options;
            options.Skin = skinName;
            options.Save();

            context.ContentEditor.Refresh();
        }
    }
}
