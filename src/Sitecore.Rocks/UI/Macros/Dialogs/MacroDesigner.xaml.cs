// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Macros.Dialogs
{
    public partial class MacroDesigner
    {
        public MacroDesigner()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        public Macro Macro { get; set; }

        [CanBeNull]
        protected object Parameter { get; set; }

        public void Initialize([NotNull] Macro macro, [CanBeNull] object parameter)
        {
            Assert.ArgumentNotNull(macro, nameof(macro));

            Parameter = parameter;
            Macro = macro;
            MacroName.Text = macro.Text;
            Scope.SelectedIndex = Macro.Scope == MacroScope.PerItem ? 0 : 1;

            RuleDesigner.Initialize(Macro.Rule, parameter);
        }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Macro.Text = MacroName.Text;
            Macro.Scope = Scope.SelectedIndex == 0 ? MacroScope.PerItem : MacroScope.Once;

            this.Close(true);
        }
    }
}
