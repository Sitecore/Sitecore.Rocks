// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Delete : ItemsOpcode
    {
        public Delete([CanBeNull] Opcode from) : base(from)
        {
        }

        protected override void Execute(Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.ID == ItemIDs.RootID)
            {
                throw new QueryException("Deleting the root item is too scary. Please wipe the database another way.");
            }

            item.Recycle();
        }
    }
}
