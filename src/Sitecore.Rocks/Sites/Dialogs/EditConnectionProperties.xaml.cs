// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Sites.Dialogs
{
    public partial class EditConnectionProperties
    {
        private Connection _connection;

        public EditConnectionProperties()
        {
            InitializeComponent();
        }

        [CanBeNull]
        public Connection Connection
        {
            get { return _connection; }

            set
            {
                _connection = value;
                PropertyGrid.SelectedObject = _connection;
            }
        }
    }
}
