// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Rules
{
    public interface IRuleAction
    {
        [NotNull]
        string Text { get; }

        bool CanExecute([CanBeNull] object parameter);

        void Execute([CanBeNull] object parameter);
    }
}
