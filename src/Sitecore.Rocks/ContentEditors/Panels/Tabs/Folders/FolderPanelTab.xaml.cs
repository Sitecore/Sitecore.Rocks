// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Panels.Tabs.Folders
{
    public partial class FolderPanelTab
    {
        public FolderPanelTab()
        {
            InitializeComponent();
        }

        protected PanelContext PanelContext { get; set; }

        public void Initialize([NotNull] PanelContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            PanelContext = context;
        }
    }
}
