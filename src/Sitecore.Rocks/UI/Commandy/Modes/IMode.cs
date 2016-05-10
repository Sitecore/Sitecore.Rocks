// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    public interface IMode
    {
        [NotNull]
        string Alias { get; }

        bool IsReady { get; }

        [NotNull]
        string Name { get; }

        [NotNull]
        string Watermark { get; }

        bool CanExecute([CanBeNull] object parameter);

        void Execute([NotNull] Hit hit, [NotNull] object parameter);

        event EventHandler IsReadyChanged;

        [CanBeNull]
        IMode SwitchMode([NotNull] string alias);
    }
}
