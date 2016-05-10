// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Commands
{
    [MeansImplicitUse]
    public class CommandAttribute : ExtensibilityAttribute
    {
        private string _submenu;

        public bool ExcludeFromSearch { get; set; }

        [NotNull, Localizable(false)]
        public string Submenu
        {
            get { return _submenu ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _submenu = value;
            }
        }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            CommandManager.LoadType(type, this);
        }
    }
}
