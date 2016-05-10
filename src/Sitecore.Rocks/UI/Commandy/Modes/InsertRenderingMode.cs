// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class InsertRenderingMode : ModeBase
    {
        private readonly List<RenderingItem> renderingItems = new List<RenderingItem>();

        public InsertRenderingMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Insert Rendering";
            Alias = "r";

            var context = commandy.Parameter as LayoutDesignerContext;
            if (context == null)
            {
                IsReady = true;
            }
        }

        [NotNull]
        public IEnumerable<RenderingItem> RenderingItems
        {
            get { return renderingItems; }
        }

        public override string Watermark
        {
            get { return "Rendering"; }
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var renderingItem = hit.Tag as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            renderingItem.RenderingContainer = context.LayoutDesigner.LayoutDesignerView.GetRenderingContainer();

            context.LayoutDesigner.LayoutDesignerView.AddRendering(renderingItem);
        }

        public void Load()
        {
            var context = Commandy.Parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var first = context.Items.FirstOrDefault();
            if (first == null)
            {
                return;
            }

            renderingItems.Clear();

            var databaseUri = first.ItemUri.DatabaseUri;

            ExecuteCompleted completed = (response, executeResult) =>
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                var items = root.Elements().Select(element => ItemHeader.Parse(databaseUri, element)).ToList();
                foreach (var itemHeader in items)
                {
                    renderingItems.Add(new RenderingItem(null, itemHeader));
                }

                IsReady = true;
            };

            AppHost.Server.Layouts.GetRenderings(databaseUri, completed);
        }
    }
}
