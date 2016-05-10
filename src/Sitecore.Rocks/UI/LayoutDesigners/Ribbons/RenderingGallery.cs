// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Fluent;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.LayoutDesigners.Controls;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Ribbons
{
    // [ToolbarElement(typeof(LayoutDesignerContext), 3100, "Home", "Renderings", ElementType = RibbonElementType.Gallery)]
    public class RenderingGallery : IToolbarElement, IToolbarGalleryFactory
    {
        public void Create(InRibbonGallery gallery, [CanBeNull] object parameter)
        {
            Assert.ArgumentNotNull(gallery, nameof(gallery));

            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var layoutDesigner = context.LayoutDesigner;

            gallery.Header = "Renderings";
            gallery.Icon = new Icon("Resources/32x32/Plus.png").GetSource();
            gallery.LargeIcon = new Icon("Resources/32x32/Plus.png").GetSource();

            LoadRenderingGallery(layoutDesigner, gallery);
        }

        private void AddRenderingFromRibbon([NotNull] LayoutDesigner layoutDesigner, [NotNull] object sender, [NotNull] RenderingButton button)
        {
            Debug.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));
            Debug.ArgumentNotNull(sender, nameof(sender));

            var itemHeader = button.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            var renderingContainer = layoutDesigner.GetView().GetRenderingContainer();

            var rendering = new RenderingItem(renderingContainer, itemHeader);

            layoutDesigner.LayoutDesignerView.AddRendering(rendering);
        }

        private void LoadRenderingGallery([NotNull] LayoutDesigner layoutDesigner, [NotNull] InRibbonGallery gallery)
        {
            Debug.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));
            Debug.ArgumentNotNull(gallery, nameof(gallery));

            Site.RequestCompleted completed = delegate(string response)
            {
                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var items = root.Elements().Select(element => ItemHeader.Parse(layoutDesigner.DatabaseUri, element)).Where(e => MatchesSpeakCoreVersion(e, layoutDesigner)).ToList();
                var renderings = new List<RenderingButton>();

                foreach (var item in items.OrderBy(i => i.ParentName).ThenBy(i => i.Name))
                {
                    var button = new RenderingButton
                    {
                        Label = item.Name,
                        Icon = item.Icon,
                        Tag = item,
                        ToolTip = ToolTipBuilder.BuildToolTip(item)
                    };

                    button.PreviewMouseUp += (sender, args) => { AddRenderingFromRibbon(layoutDesigner, sender, button); };

                    renderings.Add(button);
                }

                renderings = renderings.OrderBy(i => i.Label).ToList();

                gallery.ItemsSource = renderings;
            };

            layoutDesigner.DatabaseUri.Site.Execute("Layouts.GetRenderings", completed, layoutDesigner.DatabaseUri.DatabaseName.ToString());
        }

        private bool MatchesSpeakCoreVersion([NotNull] ItemHeader itemHeader, [NotNull] LayoutDesigner layoutDesigner)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));
            Debug.ArgumentNotNull(layoutDesigner, nameof(layoutDesigner));

            if (string.IsNullOrEmpty(layoutDesigner.SpeakCoreVersion))
            {
                return true;
            }

            var version = ((IItemData)itemHeader).GetData("ex.speakversionid") ?? string.Empty;

            return string.IsNullOrEmpty(version) || string.IsNullOrEmpty(layoutDesigner.SpeakCoreVersionId) || string.Equals(version, layoutDesigner.SpeakCoreVersionId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
