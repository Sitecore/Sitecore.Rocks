// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MenuCommandAttribute : Attribute
    {
        public MenuCommandAttribute([NotNull] string mainMenu, [NotNull] string group, double priority)
        {
            Assert.ArgumentNotNull(mainMenu, nameof(mainMenu));
            Assert.ArgumentNotNull(group, nameof(group));

            MainMenu = mainMenu;
            Group = group;
            Priority = priority;
        }

        [NotNull]
        public string Group { get; private set; }

        [NotNull]
        public string MainMenu { get; private set; }

        public double Priority { get; private set; }
    }
}
