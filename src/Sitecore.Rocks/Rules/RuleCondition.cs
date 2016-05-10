// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Rules
{
    public abstract class RuleCondition
    {
        public abstract void Evaluate([CanBeNull] object parameter, [NotNull] RuleStack stack);
    }
}
