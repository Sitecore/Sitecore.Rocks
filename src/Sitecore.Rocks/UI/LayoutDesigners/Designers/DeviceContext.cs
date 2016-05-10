// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers
{
    public class DeviceContext : ICommandContext
    {
        public DeviceContext([NotNull] DeviceModel device)
        {
            Assert.ArgumentNotNull(device, nameof(device));

            Device = device;
        }

        [NotNull]
        public DeviceModel Device { get; private set; }
    }
}
