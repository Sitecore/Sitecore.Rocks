// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners
{
    public interface ILayoutDesignerView : IContextProvider
    {
        void AddPlaceholder([NotNull] DatabaseUri databaseUri);

        void AddRendering([NotNull] RenderingItem rendering);

        [CanBeNull]
        IRenderingContainer GetRenderingContainer();

        [CanBeNull]
        IEnumerable<object> GetSelectedObjects();

        void LoadLayout([NotNull] DatabaseUri databaseUri, [NotNull] XElement layoutDefinition);

        event EventHandler Modified;

        void OpenMenu([NotNull] object sender);

        void RemoveRendering([NotNull] LayoutDesignerItem renderingItem);

        void SaveLayout([NotNull] XmlTextWriter output);

        void UpdateTracking();
    }
}
