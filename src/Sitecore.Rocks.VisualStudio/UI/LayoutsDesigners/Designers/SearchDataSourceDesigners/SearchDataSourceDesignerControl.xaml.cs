// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Designers;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutsDesigners.Designers.SearchDataSourceDesigners
{
    public partial class SearchDataSourceDesignerControl : IDesigner
    {
        private bool activated;

        public SearchDataSourceDesignerControl([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            InitializeComponent();

            DataContext = rendering;
            Rendering = rendering;

            Rendering.PropertyChanged += SetProperty;
            SearchBuilder.TextChanged += HandleTextChanged;
        }

        [NotNull]
        protected RenderingItem Rendering { get; }

        public void Activate()
        {
            if (activated)
            {
                return;
            }

            activated = true;

            SearchBuilder.DatabaseUri = Rendering.ItemUri.DatabaseUri;
            SearchBuilder.Text = Rendering.GetParameterValue("Text");
        }

        public void Close()
        {
            Rendering.PropertyChanged -= SetProperty;
        }

        private void HandleTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rendering.SetParameterValue("Text", SearchBuilder.Text);
        }

        private void SetProperty([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName != "Text")
            {
                return;
            }

            SearchBuilder.Text = Rendering.GetParameterValue("Text");
        }
    }
}
