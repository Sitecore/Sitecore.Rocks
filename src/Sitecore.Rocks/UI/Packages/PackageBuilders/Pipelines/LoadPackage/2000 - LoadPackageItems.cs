// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Pipelines.LoadPackage
{
    [Pipeline(typeof(LoadPackagePipeline), 2000)]
    public class LoadPackageItems : PipelineProcessor<LoadPackagePipeline>
    {
        protected override void Process(LoadPackagePipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var elements = pipeline.PackageElement.XPathSelectElements(@"/project/Sources/xitems/Entries/x-item");

            var items = new List<ItemUri>();

            foreach (var element in elements)
            {
                var value = element.Value;

                var parts = value.Split('/');

                if (parts.Length < 4)
                {
                    continue;
                }

                var databaseName = parts[1];
                Guid guid;
                if (!Guid.TryParse(parts[parts.Length - 3], out guid))
                {
                    continue;
                }

                var itemUri = new ItemUri(new DatabaseUri(pipeline.Site, new DatabaseName(databaseName)), new ItemId(guid));
                items.Add(itemUri);
            }

            var sb = new StringBuilder();

            foreach (var itemUri in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }

                sb.Append(itemUri.DatabaseName);
                sb.Append(',');
                sb.Append(itemUri.ItemId);
            }

            var itemList = sb.ToString();
            if (string.IsNullOrEmpty(itemList))
            {
                return;
            }

            pipeline.Site.DataService.ExecuteAsync("Items.GetItemHeaders", (response, result) => AddItems(pipeline.PackageBuilder, response, result), itemList);
        }

        private void AddItems([NotNull] PackageBuilder packageBuilder, [NotNull] string response, [NotNull] ExecuteResult result)
        {
            Debug.ArgumentNotNull(packageBuilder, nameof(packageBuilder));
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(result, nameof(result));

            if (!packageBuilder.InternalAddItems(response, result))
            {
                return;
            }

            packageBuilder.ShowItemList();
            packageBuilder.Modified = false;
        }
    }
}
