// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.UI.Rules.Commands
{
    [Command]
    public class MoveUp : MoveBase
    {
        public MoveUp()
        {
            Text = "Move Up";
            Group = "Sorting";
            SortingValue = 2000;
        }

        protected override int GetOffset()
        {
            return -1;
        }
    }
}
