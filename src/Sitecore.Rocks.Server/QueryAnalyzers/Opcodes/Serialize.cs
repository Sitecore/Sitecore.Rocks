// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Serialize : ItemsOpcode
    {
        public Serialize([CanBeNull] Opcode from) : base(from)
        {
        }

        protected override void Execute(Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            Manager.DumpItem(item);
        }
    }
}
