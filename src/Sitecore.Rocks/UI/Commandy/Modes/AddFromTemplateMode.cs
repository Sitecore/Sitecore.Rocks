// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class AddFromTemplateMode : ModeBase
    {
        private readonly List<TemplateHeader> templates = new List<TemplateHeader>();

        public AddFromTemplateMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Add from Template";
            Alias = "n";

            var databaseSelectionContext = commandy.Parameter as IDatabaseSelectionContext;
            if (databaseSelectionContext != null)
            {
                AppHost.Server.GetTemplates(databaseSelectionContext.DatabaseUri, LoadTemplates);
            }
            else
            {
                IsReady = true;
            }
        }

        [NotNull]
        public IEnumerable<TemplateHeader> Templates
        {
            get { return templates; }
        }

        public override string Watermark
        {
            get { return "Template Name"; }
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.Items.Any();
        }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var command = hit.Tag as ICommand;
            if (command == null)
            {
                return;
            }

            AppHost.Usage.ReportCommand(command, parameter);
            command.Execute(parameter);
        }

        private void LoadTemplates([NotNull] IEnumerable<TemplateHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            templates.AddRange(items);
            IsReady = true;
        }
    }
}
