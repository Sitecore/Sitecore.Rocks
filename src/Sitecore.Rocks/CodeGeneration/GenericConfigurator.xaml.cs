// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.CodeGeneration
{
    public partial class GenericConfigurator
    {
        public GenericConfigurator([NotNull] CodeGenerationConfigurator codeGenerationConfigurator)
        {
            Assert.ArgumentNotNull(codeGenerationConfigurator, nameof(codeGenerationConfigurator));

            InitializeComponent();

            Configurator = codeGenerationConfigurator;
        }

        [NotNull]
        public CodeGenerationConfigurator Configurator { get; }

        private void RegenerateCode([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Configurator.RunCustomTool();
        }
    }
}
