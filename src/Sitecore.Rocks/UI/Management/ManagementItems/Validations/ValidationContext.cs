// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations
{
    public class ValidationContext : ICommandContext
    {
        public ValidationContext([NotNull] ValidationViewer validationViewer, [NotNull] IEnumerable<ValidationDescriptor> validations)
        {
            Assert.ArgumentNotNull(validationViewer, nameof(validationViewer));
            Assert.ArgumentNotNull(validations, nameof(validations));

            ValidationViewer = validationViewer;
            Validations = validations;
        }

        [NotNull]
        public IEnumerable<ValidationDescriptor> Validations { get; private set; }

        [NotNull]
        public ValidationViewer ValidationViewer { get; private set; }
    }
}
