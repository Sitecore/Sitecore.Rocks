// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data.Items;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items
{
    [Validation("Item validators", "Validation", ExecutePerLanguage = true)]
    public class Validators : ItemValidation
    {
        private readonly ValidatorOptions options = new ValidatorOptions(true);

        public override bool CanCheck(string contextName, Item item)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));
            Assert.ArgumentNotNull(item, nameof(item));

            return contextName == "Site";
        }

        public override void Check(ValidationWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));

            var validatorCollection = ValidatorManager.BuildValidators(ValidatorsMode.ValidateButton, item);

            ValidatorManager.Validate(validatorCollection, options);

            foreach (BaseValidator validator in validatorCollection)
            {
                if (validator.Result == ValidatorResult.Valid || validator.Result == ValidatorResult.Unknown)
                {
                    continue;
                }

                var severity = SeverityLevel.None;

                switch (validator.Result)
                {
                    case ValidatorResult.Unknown:
                    case ValidatorResult.Valid:
                        severity = SeverityLevel.None;
                        break;
                    case ValidatorResult.Suggestion:
                        severity = SeverityLevel.Suggestion;
                        break;
                    case ValidatorResult.Warning:
                        severity = SeverityLevel.Warning;
                        break;
                    case ValidatorResult.Error:
                    case ValidatorResult.CriticalError:
                    case ValidatorResult.FatalError:
                        severity = SeverityLevel.Error;
                        break;
                }

                if (severity == SeverityLevel.None)
                {
                    continue;
                }

                var category = "Item Validation";
                var validatorItem = item.Database.GetItem(validator.ValidatorID);
                if (validatorItem != null)
                {
                    category = "Validation: " + validatorItem.Parent.Name;
                }

                output.Write(severity, category + " - " + validator.Name, validator.Text, string.Empty, item);
            }
        }
    }
}
