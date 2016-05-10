// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.ValidationIssues
{
    public class ValidationIssuesContext : ICommandContext, IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext
    {
        public ValidationIssuesContext([NotNull] ValidationIssues validationIssues)
        {
            Assert.ArgumentNotNull(validationIssues, nameof(validationIssues));

            ValidationIssues = validationIssues;
        }

        public IEnumerable<IItem> Items
        {
            get
            {
                var selectedItem = SelectedItem;

                if (selectedItem != null)
                {
                    yield return selectedItem;
                }
            }
        }

        [CanBeNull]
        public ValidationIssue SelectedItem { get; set; }

        [NotNull]
        public ValidationIssues ValidationIssues { get; private set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get
            {
                if (SelectedItem == null)
                {
                    return DatabaseUri.Empty;
                }

                return SelectedItem.ItemUri.DatabaseUri;
            }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }
    }
}
