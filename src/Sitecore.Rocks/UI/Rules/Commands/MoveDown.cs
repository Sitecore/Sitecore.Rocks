// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Rules.Commands
{
    [Command]
    public class MoveDown : MoveBase
    {
        public MoveDown()
        {
            Text = "Move Down";
            Group = "Sorting";
            SortingValue = 2100;
        }

        protected override int GetOffset()
        {
            return 1;
        }
    }
}
