// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Validations
{
    public abstract class ViewRenderingValidation : ItemValidation
    {
        public static readonly ID ParametersTemplateFieldId = new ID("{7D24E54F-5C16-4314-90C9-6051AA1A7DA1}");

        public static readonly ID ViewRenderingTemplateId = new ID("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}");

        [NotNull]
        protected IEnumerable<TemplateField> GetParametersTemplateFields([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var parametersTemplateId = item[ParametersTemplateFieldId];
            if (string.IsNullOrEmpty(parametersTemplateId))
            {
                yield break;
            }

            var parameterTemplateItem = item.Database.GetItem(parametersTemplateId);
            if (parameterTemplateItem == null)
            {
                yield break;
            }

            var template = TemplateManager.GetTemplate(parameterTemplateItem.ID, parameterTemplateItem.Database);
            if (template == null)
            {
                yield break;
            }

            var fields = template.GetFields(true).OrderBy(f => f.Name).ToList();

            foreach (var field in fields)
            {
                if (field.Template.BaseIDs.Length != 0)
                {
                    yield return field;
                }
            }
        }

        protected bool IsViewRendering([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID != ViewRenderingTemplateId)
            {
                return false;
            }

            return true;
        }
    }
}
