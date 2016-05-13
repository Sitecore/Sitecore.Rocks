// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.VisualStudio.Annotations;
using Sitecore.VisualStudio.UI.LayoutDesigners.Items;

namespace Sitecore.VisualStudio.UI.LayoutDesigners.Editors.GridDesigners
{
  using System.Drawing.Design;
  using Sitecore.VisualStudio.UI.LayoutDesigners.Properties.DropDowns;

  public class GridDesignerEditor : UITypeEditor
  {
    [CanBeNull]
    public override object EditValue([NotNull] ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      var renderingItem = context.Instance as RenderingItem;
      if (renderingItem == null)
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        return value;
      }

      var renderingContainer = renderingItem.RenderingContainer;
      if (renderingContainer == null)
      {
        return value;
      }

      var databaseUri = renderingContainer.DatabaseUri;

      var numberOfCells = renderingItem.GetParameterValue("NumberOfCells");
      var layoutTypeValue = renderingItem.GetParameterValue("LayoutType");
      var cellAttributes = renderingItem.GetParameterValue("CellAttributes") ?? string.Empty;

      if (string.IsNullOrEmpty(numberOfCells))
      {
        numberOfCells = "6";
      }

      int cells;
      if (!int.TryParse(numberOfCells, out cells))
      {
        cells = 9;
      }

      var dialog = new GridDesignerMain(databaseUri, layoutTypeValue, cells, cellAttributes);

      if (AppHost.Shell.ShowDialog(dialog) != true)
      {
        // ReSharper disable once AssignNullToNotNullAttribute
        return value;
      }

      renderingItem.SetParameterValue("NumberOfCells", dialog.NumberOfCellsValue);
      renderingItem.SetParameterValue("LayoutType", new DropDownValue(new Tuple<string, string>(dialog.LayoutTypeValue.Name, dialog.LayoutTypeValue.Id.ToString("B").ToUpperInvariant())));
      return dialog.CellAttributesValue;
    }

    public override UITypeEditorEditStyle GetEditStyle([CanBeNull] ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.Modal;
    }
  }
}