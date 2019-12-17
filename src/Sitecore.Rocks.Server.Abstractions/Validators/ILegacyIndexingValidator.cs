using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Rocks.Server.Abstractions.Validators
{
    public interface ILegacyIndexingValidator
    {
        bool ShouldValidate { get; }
        bool ServerSpecificPropertiesSettingValue { get; }
        TimeSpan UpdateIntervalSettingValue { get; }
    }
}
