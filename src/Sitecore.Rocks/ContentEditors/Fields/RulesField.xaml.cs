// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.UI.RuleEditors;

namespace Sitecore.Rocks.ContentEditors.Fields
{
    [FieldControl("rules")]
    public partial class RulesField : IReusableFieldControl
    {
        private XDocument rules;

        public RulesField()
        {
            InitializeComponent();
        }

        [NotNull]
        protected DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        protected string FieldSource { get; set; }

        public Control GetFocusableControl()
        {
            return this;
        }

        public bool IsSupported(Field sourceField)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));

            return true;
        }

        public void UnsetField()
        {
        }

        public event ValueModifiedEventHandler ValueModified;

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var value = ((IFieldControl)this).GetValue();

            if (string.IsNullOrEmpty(value) || value.Contains("<ruleset />"))
            {
                value = "<ruleset><rule uid=\"" + Guid.NewGuid().ToString("B").ToUpperInvariant() + "\" /></ruleset>";
            }

            var root = value.ToXElement();
            if (root == null)
            {
                return;
            }

            var ruleModel = new RuleModel(root);

            var dialog = new RuleEditorDialog();
            dialog.Initialize(DatabaseUri, FieldSource, ruleModel);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            rules = root.Document;
            Presenter.RuleModel = ruleModel;
            Presenter.Render();

            RaiseModified();
        }

        Control IFieldControl.GetControl()
        {
            return this;
        }

        string IFieldControl.GetValue()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            rules.Save(output);

            output.Flush();

            return writer.ToString();
        }

        private void RaiseModified()
        {
            var valueModified = ValueModified;
            if (valueModified != null)
            {
                valueModified();
            }
        }

        void IFieldControl.SetField(Field sourceField)
        {
            Debug.ArgumentNotNull(sourceField, nameof(sourceField));

            FieldSource = sourceField.Source;
            DatabaseUri = sourceField.FieldUris.First().ItemVersionUri.DatabaseUri;

            var actions = new List<RuleEditor.ActionItem>();
            var conditions = new List<RuleEditor.ConditionItem>();

            var displayData = sourceField.DisplayData.ToXElement();
            if (displayData != null)
            {
                var root = displayData.Element("rules");

                if (root != null)
                {
                    var conditionsElement = root.Element("conditions");
                    if (conditionsElement != null)
                    {
                        foreach (var element in conditionsElement.Elements())
                        {
                            var condition = new RuleEditor.ConditionItem();
                            conditions.Add(condition);

                            condition.Id = element.GetAttributeValue("id");
                            condition.Category = element.GetAttributeValue("category");
                            condition.Text = element.Value;
                        }
                    }

                    var actionsElement = root.Element("actions");
                    if (actionsElement != null)
                    {
                        foreach (var element in actionsElement.Elements())
                        {
                            var action = new RuleEditor.ActionItem();
                            actions.Add(action);

                            action.Id = element.GetAttributeValue("id");
                            action.Category = element.GetAttributeValue("category");
                            action.Text = element.Value;
                        }
                    }
                }
            }

            Presenter.Actions = actions;
            Presenter.Conditions = conditions;
            Presenter.DatabaseUri = DatabaseUri;
            Presenter.DataSource = FieldSource;
        }

        private void SetModified([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            RaiseModified();
        }

        void IFieldControl.SetValue(string value)
        {
            Debug.ArgumentNotNull(value, nameof(value));

            try
            {
                rules = string.IsNullOrEmpty(value) ? XDocument.Parse("<ruleset />") : XDocument.Parse(value);
            }
            catch
            {
                rules = XDocument.Parse("<ruleset />");
            }

            var ruleModel = new RuleModel(rules.Root);

            Presenter.RuleModel = ruleModel;

            Presenter.Render();
        }
    }
}
