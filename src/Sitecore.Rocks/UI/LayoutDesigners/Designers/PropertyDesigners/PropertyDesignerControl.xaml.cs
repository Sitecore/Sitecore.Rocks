// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.PropertyDesigners
{
    public partial class PropertyDesignerControl : IDesigner
    {
        private bool _activated;

        public PropertyDesignerControl([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            InitializeComponent();

            Rendering = rendering;
        }

        [NotNull]
        protected RenderingItem Rendering { get; }

        public void Activate()
        {
            if (_activated)
            {
                return;
            }

            _activated = true;

            Properties.SelectedObject = Rendering;
        }

        public void Close()
        {
        }
    }
}
