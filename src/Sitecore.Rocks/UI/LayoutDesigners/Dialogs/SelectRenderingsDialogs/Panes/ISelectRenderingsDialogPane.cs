// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Panes
{
    public interface ISelectRenderingsDialogPane
    {
        bool AllowMultipleRenderings { get; }

        [NotNull]
        DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        string Header { get; }

        [NotNull]
        IRenderingContainer RenderingContainer { get; set; }

        [NotNull]
        string SpeakCoreVersion { get; set; }

        bool AreButtonsEnabled();

        void Close();

        event MouseButtonEventHandler DoubleClick;

        void GetSelectedRenderings([NotNull] Action<IEnumerable<RenderingItem>> completed);

        event SelectionChangedEventHandler SelectionChanged;
    }
}
