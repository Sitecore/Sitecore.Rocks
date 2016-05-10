// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs.RenderingEditors
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), BaseTypeRequired(typeof(IRenderingEditor)), MeansImplicitUse]
    public class RenderingEditorAttribute : ExtensibilityAttribute
    {
        public RenderingEditorAttribute([NotNull] string renderingId)
        {
            Assert.ArgumentNotNull(renderingId, nameof(renderingId));

            RenderingId = renderingId;
        }

        [NotNull]
        public string RenderingId { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            RenderingEditorManager.LoadType(type, this);
        }
    }
}
