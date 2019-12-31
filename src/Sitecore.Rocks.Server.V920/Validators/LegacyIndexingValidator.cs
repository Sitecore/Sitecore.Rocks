using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.Abstractions.Validators;

namespace Sitecore.Rocks.Server.V920.Validators
{
    public class LegacyIndexingValidator : ILegacyIndexingValidator
    {
        public bool ShouldValidate => false;
        public bool ServerSpecificPropertiesSettingValue => false;
        public TimeSpan UpdateIntervalSettingValue => TimeSpan.Zero;
    }
}
