// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Windows;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.TemplateDesigner.Commands
{
    [Command]
    public class Save : CommandBase, IToolbarElement
    {
        public Save()
        {
            Text = Resources.Save_Save_Save;
            Group = "Save";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            return parameter is TemplateDesigner.Context;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TemplateDesigner.Context;
            if (context == null)
            {
                return;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            var icon = context.TemplateDesigner.TemplateIcon;
            if (icon == @"/sitecore/images/blank.gif")
            {
                icon = string.Empty;
            }

            output.WriteStartElement(@"template");
            output.WriteAttributeString(@"name", context.TemplateName);
            output.WriteAttributeString(@"id", context.TemplateUri.ItemId.ToString());
            output.WriteAttributeString(@"icon", icon);

            var baseTemplates = string.Empty;
            foreach (var baseTemplate in context.BaseTemplates)
            {
                if (!string.IsNullOrEmpty(baseTemplates))
                {
                    baseTemplates += @"|";
                }

                baseTemplates += baseTemplate.ToString();
            }

            output.WriteAttributeString(@"basetemplates", baseTemplates);

            foreach (var templateSection in context.Sections)
            {
                if (string.IsNullOrEmpty(templateSection.Name))
                {
                    continue;
                }

                output.WriteStartElement(@"section");
                output.WriteAttributeString(@"id", templateSection.Id);
                output.WriteAttributeString(@"name", templateSection.Name);

                foreach (var templateField in templateSection.Fields)
                {
                    if (string.IsNullOrEmpty(templateField.Name))
                    {
                        continue;
                    }

                    output.WriteStartElement(@"field");
                    output.WriteAttributeString(@"id", templateField.Id);
                    output.WriteAttributeString(@"name", templateField.Name);
                    output.WriteAttributeString(@"type", templateField.Type);
                    output.WriteAttributeString(@"source", templateField.Source);
                    output.WriteAttributeString(@"shared", templateField.Shared ? @"1" : @"0");
                    output.WriteAttributeString(@"unversioned", templateField.Unversioned ? @"1" : @"0");
                    output.WriteAttributeString(@"title", string.Empty);
                    output.WriteAttributeString(@"validatorbar", templateField.Validations);

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            var xml = writer.ToString();

            GetValueCompleted<string> callback = delegate(string response)
            {
                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox(Resources.Save_Execute_Failed_to_save_the_template, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                context.TemplateDesigner.SetModified(false);

                Notifications.RaiseTemplateSaved(this, context.TemplateUri, context.TemplateName);
            };

            context.TemplateUri.Site.DataService.SaveTemplateXml(xml, context.TemplateUri.DatabaseUri, callback);
        }
    }
}
