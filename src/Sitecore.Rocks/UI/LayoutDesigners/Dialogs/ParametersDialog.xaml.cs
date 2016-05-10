// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs
{
    public partial class ParametersDialog
    {
        private readonly List<NameValue> parameters = new List<NameValue>();

        public ParametersDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public Dictionary<string, string> Parameters { get; private set; }

        public void Initialize([NotNull] Dictionary<string, string> parameters)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            Title = "Rendering Parameters";
            Parameters = parameters;

            foreach (var pair in parameters)
            {
                this.parameters.Add(new NameValue
                {
                    Name = pair.Key,
                    Value = pair.Value
                });
            }

            ParametersGrid.ItemsSource = this.parameters;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            EnableButtons();
        }

        private void EnableButtons()
        {
            OK.IsEnabled = true;
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Parameters.Clear();
            foreach (var parameter in parameters)
            {
                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    Parameters[parameter.Name] = parameter.Value;
                }
            }

            this.Close(true);
        }

        public class NameValue
        {
            [NotNull]
            public string Name { get; set; }

            [NotNull]
            public string Value { get; set; }
        }
    }
}
