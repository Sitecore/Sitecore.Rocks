using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.Abstractions.Validators;

namespace Sitecore.Rocks.Server.V910.Validators
{
    public class LegacyIndexingValidator : ILegacyIndexingValidator
    {
        public bool ShouldValidate => true;

        public bool ServerSpecificPropertiesSettingValue => Sitecore.Configuration.Settings.Indexing.ServerSpecificProperties;

        public TimeSpan UpdateIntervalSettingValue => Sitecore.Configuration.Settings.Indexing.UpdateInterval;
    }
}
