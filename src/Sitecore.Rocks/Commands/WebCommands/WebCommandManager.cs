// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Commands.WebCommands
{
    public static class WebCommandManager
    {
        public static void Load([NotNull] WebDataService dataService, [NotNull] XElement webcommandsElement)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(webcommandsElement, nameof(webcommandsElement));

            foreach (var commandsElement in webcommandsElement.Elements())
            {
                ProcessCommands(dataService, commandsElement);
            }
        }

        private static void ProcessCommands([NotNull] WebDataService dataService, [NotNull] XElement commandsElement)
        {
            Debug.ArgumentNotNull(dataService, nameof(dataService));
            Debug.ArgumentNotNull(commandsElement, nameof(commandsElement));

            foreach (var submenuElement in commandsElement.Elements("submenu"))
            {
                var webCommandSubmenu = new WebCommandSubmenu(dataService, submenuElement);

                var submenu = submenuElement.GetElementValue("submenu");

                var commandDescriptor = new CommandManager.CommandDescriptor(webCommandSubmenu, submenu, typeof(WebCommand));

                CommandManager.Add(commandDescriptor);
            }

            foreach (var commandElement in commandsElement.Elements("command"))
            {
                var webCommand = new WebCommand(dataService, commandElement);

                var submenu = commandElement.GetElementValue("submenu");

                var commandDescriptor = new CommandManager.CommandDescriptor(webCommand, submenu, typeof(WebCommand));

                CommandManager.Add(commandDescriptor);
            }
        }
    }
}
