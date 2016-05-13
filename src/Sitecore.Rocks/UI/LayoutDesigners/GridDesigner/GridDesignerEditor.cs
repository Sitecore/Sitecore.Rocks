// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.DropDowns;

namespace Sitecore.Rocks.UI.LayoutDesigners.GridDesigner
{
  using System.Drawing.Design;

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