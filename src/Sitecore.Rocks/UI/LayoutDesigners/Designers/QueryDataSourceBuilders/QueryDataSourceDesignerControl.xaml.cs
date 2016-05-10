// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.QueryDataSourceBuilders
{
    public partial class QueryDataSourceDesignerControl : IDesigner
    {
        private bool activated;

        public QueryDataSourceDesignerControl([NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            InitializeComponent();

            DataContext = rendering;
            Rendering = rendering;

            Rendering.PropertyChanged += SetProperty;
            QueryBuilder.TextChanged += HandleTextChanged;
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

            QueryBuilder.DatabaseUri = Rendering.ItemUri.DatabaseUri;
            QueryBuilder.Text = Rendering.GetParameterValue("Query") ?? string.Empty;
        }

        public void Close()
        {
            Rendering.PropertyChanged -= SetProperty;
        }

        private void HandleTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rendering.SetParameterValue("Query", QueryBuilder.Text);
        }

        private void SetProperty([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName != "Query")
            {
                return;
            }

            QueryBuilder.Text = Rendering.GetParameterValue("Query") ?? string.Empty;
        }
    }
}
