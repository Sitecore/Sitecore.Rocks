// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.Shell
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ShellMenuCommandAttribute : Attribute
    {
        public ShellMenuCommandAttribute(int commandId)
        {
            CommandId = commandId;
            IsBound = true;
        }

        public ShellMenuCommandAttribute(ShellMenuCommandPlacement placement, double priority)
        {
            Placement = placement;
            Priority = priority;
            IsBound = false;
        }

        public int CommandId { get; private set; }

        public bool IsBound { get; private set; }

        public ShellMenuCommandPlacement Placement { get; private set; }

        public double Priority { get; private set; }
    }
}
