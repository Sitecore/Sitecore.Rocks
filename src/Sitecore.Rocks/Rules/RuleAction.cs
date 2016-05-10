// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Rules
{
    public abstract class RuleAction : IRuleAction
    {
        public string Text { get; protected set; }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);
    }
}
