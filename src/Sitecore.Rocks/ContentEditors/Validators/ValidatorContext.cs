// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Validators
{
    public class ValidatorContext : ICommandContext
    {
        public ValidatorContext([NotNull] ValidatorMarker validatorMarker, [NotNull] ContentEditor contentEditor, [CanBeNull] Validator validator)
        {
            Assert.ArgumentNotNull(validatorMarker, nameof(validatorMarker));
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            ValidatorMarker = validatorMarker;
            ContentEditor = contentEditor;
            Validator = validator;
        }

        [NotNull]
        public ContentEditor ContentEditor { get; private set; }

        [CanBeNull]
        public Validator Validator { get; private set; }

        [NotNull]
        public ValidatorMarker ValidatorMarker { get; private set; }
    }
}
