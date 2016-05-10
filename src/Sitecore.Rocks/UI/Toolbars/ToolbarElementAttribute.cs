// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Toolbars
{
    [AttributeUsage(AttributeTargets.Class), BaseTypeRequired(typeof(IToolbarElement))]
    public class ToolbarElementAttribute : ExportAttribute
    {
        public ToolbarElementAttribute(Type contextType, double sortOrder, string strip, string chunk, RibbonElementType elementType = RibbonElementType.LargeButton) : base(typeof(IToolbarElement))
        {
            ContextType = contextType;
            SortOrder = sortOrder;
            Strip = strip;
            Chunk = chunk;
            ElementType = elementType;
        }

        public ToolbarElementAttribute(Type contextType, string strip, RibbonElementType elementType = RibbonElementType.LargeButton) : base(typeof(IToolbarElement))
        {
            ContextType = contextType;
            SortOrder = 0;
            Strip = strip;
            Chunk = string.Empty;
            ElementType = elementType;
        }

        public string Chunk { get; private set; }

        public Type ContextType { get; private set; }

        public RibbonElementType ElementType { get; set; }

        public string Icon { get; set; }

        public double SortOrder { get; private set; }

        public string Strip { get; private set; }

        public string Text { get; set; }
    }
}
