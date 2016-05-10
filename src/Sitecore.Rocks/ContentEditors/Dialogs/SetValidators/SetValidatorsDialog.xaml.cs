// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentEditors.Dialogs.SetValidators
{
    public partial class SelectValidatorsDialog
    {
        public SelectValidatorsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        public void Initialize([NotNull] string title, [NotNull] DatabaseUri databaseUri, [NotNull] List<ItemId> quickActionBarValidators, [NotNull] List<ItemId> validateButtonValidators, [NotNull] List<ItemId> validatorBarValidators, [NotNull] List<ItemId> workflowValidators)
        {
            Assert.ArgumentNotNull(title, nameof(title));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(quickActionBarValidators, nameof(quickActionBarValidators));
            Assert.ArgumentNotNull(validateButtonValidators, nameof(validateButtonValidators));
            Assert.ArgumentNotNull(validatorBarValidators, nameof(validatorBarValidators));
            Assert.ArgumentNotNull(workflowValidators, nameof(workflowValidators));

            Title = title;

            QuickActionBar.Initialize(databaseUri, quickActionBarValidators);
            ValidateButton.Initialize(databaseUri, validateButtonValidators);
            ValidatorBar.Initialize(databaseUri, validatorBarValidators);
            Workflow.Initialize(databaseUri, workflowValidators);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
