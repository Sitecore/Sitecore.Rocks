// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class SelectItem
    {
        public SelectItem()
        {
            Fields = new List<SelectField>();
        }

        public List<SelectField> Fields { get; private set; }

        public Item Item { get; set; }
    }
}
