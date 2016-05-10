// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public class RuleEditorContext : ICommandContext
    {
        [NotNull]
        public RuleEditor RuleEditor { get; set; }

        [NotNull]
        public RulesPresenter RulePresenter { get; set; }

        [CanBeNull]
        public ListBoxItem SelectedRuleEntry { get; set; }
    }
}
