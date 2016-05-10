// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Toolbars
{
    public class ToolbarElement
    {
        public ToolbarElement([NotNull] IToolbarElement element, double sortOrder, [NotNull] string strip, [NotNull] string chunk, [CanBeNull] string icon, [CanBeNull] string text, [NotNull] Type contextType, RibbonElementType elementType = RibbonElementType.LargeButton)
        {
            Element = element;
            SortOrder = sortOrder;
            Strip = strip;
            Chunk = chunk;
            Icon = icon;
            Text = text;
            ContextType = contextType;
            ElementType = elementType;
        }

        [NotNull]
        public string Chunk { get; private set; }

        [NotNull]
        public Type ContextType { get; }

        public IToolbarElement Element { get; private set; }

        public RibbonElementType ElementType { get; private set; }

        [CanBeNull]
        public string Icon { get; private set; }

        public double SortOrder { get; private set; }

        [NotNull]
        public string Strip { get; private set; }

        [CanBeNull]
        public string Text { get; private set; }
    }
}
