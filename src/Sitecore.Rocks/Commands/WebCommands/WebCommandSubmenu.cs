// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Commands.WebCommands
{
    public class WebCommandSubmenu : Submenu
    {
        public WebCommandSubmenu([NotNull] WebDataService dataService, [NotNull] XElement submenuElement)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(submenuElement, nameof(submenuElement));

            DataService = dataService;
            Text = submenuElement.GetElementValue("text", "[text missing]");
            SortingValue = submenuElement.GetElementValueInt("sorting", 1000);
            Group = submenuElement.GetElementValue("group", "WebCommands");

            SubmenuName = submenuElement.GetElementValue("name", Text);

            var context = submenuElement.GetElementValue("context");
            ParseContext(context);
        }

        [NotNull]
        public WebDataService DataService { get; private set; }

        private void ParseContext([NotNull] string context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            var contextType = Type.GetType(context);
            if (contextType == null)
            {
                AppHost.Output.Log("WebCommand: Context not found: " + context);
                return;
            }

            ContextType = contextType;
        }
    }
}
