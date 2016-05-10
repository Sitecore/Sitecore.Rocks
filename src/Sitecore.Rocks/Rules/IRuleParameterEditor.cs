// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Rules
{
    public interface IRuleParameterEditor
    {
        [NotNull]
        string GetValue([NotNull] string defaultValue, [CanBeNull] object parameter);
    }
}
