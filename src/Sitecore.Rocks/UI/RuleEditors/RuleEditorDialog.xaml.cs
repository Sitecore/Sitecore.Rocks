// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.RuleEditors
{
    public partial class RuleEditorDialog
    {
        public RuleEditorDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        public void Initialize([NotNull] DatabaseUri databaseUri, [NotNull] string dataSource, [NotNull] RuleModel ruleModel)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));
            Assert.ArgumentNotNull(ruleModel, nameof(ruleModel));

            Editor.Initialize(databaseUri, dataSource, ruleModel);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
