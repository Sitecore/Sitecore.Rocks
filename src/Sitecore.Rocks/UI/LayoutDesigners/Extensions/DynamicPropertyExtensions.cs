// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Properties;

namespace Sitecore.Rocks.UI.LayoutDesigners.Extensions
{
    public static class DynamicPropertyExtensions
    {
        public static bool CanRead([NotNull] this DynamicProperty dynamicProperty)
        {
            Assert.ArgumentNotNull(dynamicProperty, nameof(dynamicProperty));

            var o = dynamicProperty.Attributes[@"bindmode"];
            if (o is BindingMode)
            {
                var bindMode = (BindingMode)o;
                if (bindMode == BindingMode.Read || bindMode == BindingMode.ReadWrite)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
