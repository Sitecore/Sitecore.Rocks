// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.ValidationIssues
{
    public class ValidationIssue : IItem
    {
        public ValidationIssue([NotNull] ItemUri itemUri, [NotNull] string name, [NotNull] Icon icon, [NotNull] Icon itemIcon, [NotNull] string categoryName, [NotNull] Icon categoryIcon, [NotNull] string text, [NotNull] string validatorId, [NotNull] string validatorName, [NotNull] Icon validatorIcon)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(icon, nameof(icon));
            Assert.ArgumentNotNull(itemIcon, nameof(itemIcon));
            Assert.ArgumentNotNull(categoryName, nameof(categoryName));
            Assert.ArgumentNotNull(categoryIcon, nameof(categoryIcon));
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(validatorId, nameof(validatorId));
            Assert.ArgumentNotNull(validatorName, nameof(validatorName));
            Assert.ArgumentNotNull(validatorIcon, nameof(validatorIcon));

            CategoryIcon = categoryIcon;
            CategoryName = categoryName;
            Icon = icon;
            ItemIcon = itemIcon;
            Name = name;
            ItemUri = itemUri;
            Text = text;
            ValidatorIcon = validatorIcon;
            ValidatorId = validatorId;
            ValidatorName = validatorName;
        }

        [NotNull]
        public Icon CategoryIcon { get; private set; }

        [NotNull]
        public string CategoryName { get; private set; }

        public Icon Icon { get; }

        [NotNull]
        public Icon ItemIcon { get; private set; }

        public ItemUri ItemUri { get; }

        public string Name { get; set; }

        [NotNull]
        public string Text { get; private set; }

        [NotNull]
        public Icon ValidatorIcon { get; private set; }

        [NotNull]
        public string ValidatorId { get; private set; }

        [NotNull]
        public string ValidatorName { get; private set; }
    }
}
