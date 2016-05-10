// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.UI.Macros.Commands;

namespace Sitecore.Rocks.UI.Macros
{
    public class Macro
    {
        public Macro([NotNull] Rule rule, [NotNull] string text)
        {
            Assert.ArgumentNotNull(rule, nameof(rule));
            Assert.ArgumentNotNull(text, nameof(text));

            Rule = rule;
            Text = text;
        }

        [NotNull]
        public Rule Rule { get; }

        public MacroScope Scope { get; set; }

        [NotNull]
        public string Text { get; set; }

        public void Run([CanBeNull] object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            if (Scope == MacroScope.Once)
            {
                Rule.Execute(parameter);
                return;
            }

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            foreach (var item in context.Items)
            {
                Rule.Execute(new MacroCommand.ItemContext(item));
            }
        }
    }
}
