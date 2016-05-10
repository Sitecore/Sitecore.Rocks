// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations.Items.Templates
{
    [Validation("Duplicate template field name", "Templates")]
    public class DuplicateTemplateField : ItemValidation
    {
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

            if (item.TemplateID != TemplateIDs.Template)
            {
                return;
            }

            var template = TemplateManager.GetTemplate(item.ID, item.Database);
            if (template == null)
            {
                return;
            }

            var templateFields = template.GetFields();

            for (var i0 = 0; i0 < templateFields.Length; i0++)
            {
                var field0 = templateFields[i0];

                for (var i1 = i0 + 1; i1 < templateFields.Length; i1++)
                {
                    var field1 = templateFields[i1];
                    if (field0 == field1)
                    {
                        continue;
                    }

                    if (string.Compare(field0.Name, field1.Name, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    output.Write(SeverityLevel.Warning, "Duplicate template field name", string.Format("The template contains two or more field with the same name \"{0}\". Even if these fields are located in different sections, it is still not recommended as the name is ambiguous.", field0.Name), "Rename one or more of the fields.", item);
                }
            }
        }
    }
}
