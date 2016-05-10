// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.Controls
{
    public class ButtonIcon : Image
    {
        private string icon;

        public ButtonIcon()
        {
            SetValue(KeyboardNavigation.IsTabStopProperty, false);
            Width = 16;
            Height = 16;
        }

        [CanBeNull]
        public string Icon
        {
            get { return icon; }

            set
            {
                icon = value;
                if (value == null)
                {
                    Source = null;
                    return;
                }

                Source = new Icon(value).GetSource();
            }
        }
    }
}
