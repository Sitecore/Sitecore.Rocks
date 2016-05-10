// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs
{
    public partial class EditRendering
    {
        private object _source;

        public EditRendering()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public object Source
        {
            get { return _source; }

            set
            {
                _source = value;

                PropertyGrid.SelectedObject = _source;
            }
        }

        public void Update()
        {
        }
    }
}
