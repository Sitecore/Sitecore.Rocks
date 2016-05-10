// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.Controls
{
    [Flags]
    public enum ControlDragAdornerPosition
    {
        None = 0,

        Top = 1,

        Over = 2,

        Bottom = 4,

        All = Top | Over | Bottom
    }
}
