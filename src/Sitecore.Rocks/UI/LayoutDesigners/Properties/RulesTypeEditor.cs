// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.RuleEditors;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties
{
    public class RulesTypeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(provider, nameof(provider));

            var renderingItem = context.Instance as RenderingItem;
            if (renderingItem == null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            var ruleset = value as string ?? string.Empty;

            if (string.IsNullOrEmpty(ruleset) || ruleset.Contains("<ruleset />"))
            {
                ruleset = "<ruleset><rule /></ruleset>";
            }

            var root = ruleset.ToXElement();
            if (root == null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            var dataSource = string.Empty;
            if (renderingItem.ItemUri.Site.SitecoreVersion >= Constants.Versions.Version7)
            {
                // /sitecore/system/Settings/Rules/Conditional Renderings
                dataSource = "{C3D3CAAF-1F07-43FB-B8FF-3EC6D712262C}";
            }

            var ruleModel = new RuleModel(root);

            var dialog = new RuleEditorDialog();
            dialog.Initialize(renderingItem.ItemUri.DatabaseUri, dataSource, ruleModel);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return value;
            }

            return ruleModel.ToString();
        }

        public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
