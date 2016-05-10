// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Controls;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Rules
{
    public class RuleDesignerContext : ICommandContext
    {
        public ListBox Description { get; set; }

        public RuleDesigner RuleDesigner { get; set; }
    }
}
