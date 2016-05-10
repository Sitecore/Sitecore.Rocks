// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Publishing
{
    [Command(Submenu = PublishSubmenu.Name), Feature(FeatureNames.Publishing)]
    public class Republish : PublishDatabaseCommand
    {
        public Republish()
        {
            Group = "PublishDatabase";
            Text = Resources.Republish_Republish_Republish;
            PublishingText = Resources.Republish_Republish_Republishing___;
            Mode = 0;
            SortingValue = 5200;
        }
    }
}
