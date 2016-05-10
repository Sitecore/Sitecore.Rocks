// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.BreadcrumbDropDown
{
    public class BreadcrumbDropDownPipeline : Pipeline<BreadcrumbDropDownPipeline>
    {
        public Database Database { get; private set; }

        public ICollection<Item> Items { get; private set; }

        public string Path { get; set; }

        [NotNull]
        public BreadcrumbDropDownPipeline WithParameters([NotNull] Database database, [NotNull] string path)
        {
            Database = database;
            Path = path;

            Items = new List<Item>();

            Start();

            return this;
        }
    }
}
