// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public partial class PermissionToggle
    {
        public enum Permission
        {
            Allow = 0,

            Deny,

            NotSet
        }

        private Permission _state;

        public PermissionToggle()
        {
            InitializeComponent();
            State = Permission.NotSet;
        }

        [NotNull]
        public Permission State
        {
            get { return _state; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _state = value;
                switch (value)
                {
                    case Permission.Allow:
                        CnvAllowDisabled.Visibility = Visibility.Collapsed;
                        CnvAllowEnabled.Visibility = Visibility.Visible;
                        break;
                    case Permission.Deny:
                        CnvDenyDisabled.Visibility = Visibility.Collapsed;
                        CnvDenyEnabled.Visibility = Visibility.Visible;
                        break;
                    default:
                        CnvAllowDisabled.Visibility = Visibility.Visible;
                        CnvAllowEnabled.Visibility = Visibility.Collapsed;
                        CnvDenyDisabled.Visibility = Visibility.Visible;
                        CnvDenyEnabled.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }
    }
}
