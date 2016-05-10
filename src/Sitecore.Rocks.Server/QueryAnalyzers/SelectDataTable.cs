// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;

namespace Sitecore.Rocks.Server.QueryAnalyzers
{
    public class SelectDataTable
    {
        public SelectDataTable()
        {
            Columns = new List<SelectColumn>();
            Items = new List<SelectItem>();
        }

        public List<SelectColumn> Columns { get; set; }

        public List<SelectItem> Items { get; set; }
    }
}
