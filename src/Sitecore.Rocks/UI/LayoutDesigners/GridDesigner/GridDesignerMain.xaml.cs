// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Newtonsoft.Json;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.LayoutDesigners.GridDesigner
{
  /// <summary>
  /// GridDesigner class.
  /// </summary>
  public partial class GridDesignerMain
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GridDesignerMain" /> class.
    /// </summary>
    /// <param name="databaseUri">The database Uri.</param>
    /// <param name="currentLayoutTypeValue">The current layout type value.</param>
    /// <param name="numberOfCell">The number of cell.</param>
    /// <param name="cellAttributes">The cell attributes.</param>
    public GridDesignerMain([NotNull] DatabaseUri databaseUri, [CanBeNull] string currentLayoutTypeValue, int numberOfCell, [NotNull] string cellAttributes)
    {
      InitializeComponent();
      this.InitializeDialog();

      Site.RequestCompleted completed = delegate(string response)
      {
        LayoutType currentLayoutType = null;
        var layoutTypes = new List<LayoutType>();

        var root = response.ToXElement();
        if (root != null)
        {
          foreach (var element in root.Elements())
          {
            var layoutType = GetLayoutTypeFromItem(element);
            if (layoutType != null)
            {
              layoutTypes.Add(layoutType);
              if (layoutType.Name.Equals(currentLayoutTypeValue))
              {
                currentLayoutType = layoutType;
              }
            }
          }
        }

        InitializeGridDesigner(layoutTypes, currentLayoutType, numberOfCell, cellAttributes);
      };

      databaseUri.Site.Execute("Layouts.GetGridDesignerLayouts", completed, databaseUri.DatabaseName.Name);
    }

    /// <summary>
    /// Gets the layout type from item.
    /// </summary>
    /// <param name="element">The element.</param>    
    /// <returns></returns>
    private static LayoutType GetLayoutTypeFromItem(XElement element)
    {
      var layoutType = new LayoutType();
      Console.Write("ID:" + element.GetAttributeValue("id"));
      Guid guid;
      if (!Guid.TryParse(element.GetAttributeValue("id"), out guid))
      {
        return null;
      }

      layoutType.Id = guid;
      layoutType.Name = element.GetAttributeValue("name");
      layoutType.Large = int.Parse(element.GetAttributeValue("large"));
      layoutType.Medium = int.Parse(element.GetAttributeValue("medium"));
      layoutType.Small = int.Parse(element.GetAttributeValue("small"));
      layoutType.XSmall = int.Parse(element.GetAttributeValue("xsmall"));
      return layoutType;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Gets or sets the number of cells value.
    /// </summary>
    /// <value>
    /// The number of cells value.
    /// </value>
    public int NumberOfCellsValue { get; set; }

    /// <summary>
    /// Gets or sets the layout type value.
    /// </summary>
    /// <value>
    /// The layout type value.
    /// </value>
    public LayoutType LayoutTypeValue { get; set; }

    /// <summary>
    /// Gets or sets the cell attributes value.
    /// </summary>
    /// <value>
    /// The cell attributes value.
    /// </value>
    public string CellAttributesValue { get; set; }

    /// <summary>
    /// The sizes count
    /// </summary>
    private const int SizesCount = 4;

    /// <summary>
    /// The medium size
    /// </summary>
    private const int MediumGridSize = 1200;

    /// <summary>
    /// The small size
    /// </summary>
    private const int SmallGridSize = 768;

    /// <summary>
    /// The extra small size
    /// </summary>
    private const int ExtraSmallGridSize = 414;

    /// <summary>
    /// Gets or sets the grid attributes.
    /// </summary>
    /// <value>
    /// The grid attributes.
    /// </value>
    private GridAttributes GridAttributes { get; set; }

    /// <summary>
    /// Gets or sets the grid columns number.
    /// </summary>
    /// <value>
    /// The grid columns number.
    /// </value>
    private int[] GridColumnsNumber { get; set; }

    /// <summary>
    /// Gets or sets the total cells.
    /// </summary>
    /// <value>
    /// The total cells.
    /// </value>
    private int TotalCells { get; set; }

    /// <summary>
    /// Gets or sets the size of the current preview.
    /// </summary>
    /// <value>
    /// The size of the current preview.
    /// </value>
    private Sizes CurrentPreviewSize { get; set; }

    /// <summary>
    /// Gets or sets the index of the current preview size.
    /// </summary>
    /// <value>
    /// The index of the current preview size.
    /// </value>
    private int CurrentPreviewSizeIndex { get; set; }

    /// <summary>
    /// Gets or sets the type of the current layout.
    /// </summary>
    /// <value>
    /// The type of the current layout.
    /// </value>
    private LayoutType CurrentLayoutType { get; set; }

    /// <summary>
    /// Gets or sets the layout types.
    /// </summary>
    /// <value>
    /// The layout types.
    /// </value>
    private List<LayoutType> LayoutTypes { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Initializes the grid designer.
    /// </summary>
    /// <param name="cellAttributes">The cell attributes.</param>
    /// <param name="currentLayoutType">The current layout type.</param>
    /// <param name="layoutTypes">The layout types.</param>
    /// <param name="numberOfCell">The number of cell.</param>
    private void InitializeGridDesigner(List<LayoutType> layoutTypes, [CanBeNull] LayoutType currentLayoutType, int numberOfCell, string cellAttributes)
    {
      InitializeControlsAndProperties(layoutTypes, currentLayoutType, numberOfCell, cellAttributes);
      InitializeGrid();
      InitializeEvents();
    }

    /// <summary>
    /// Initializes the events.
    /// </summary>
    private void InitializeEvents()
    {
      NumberOfCells.TextChanged += NumberOfCellsTextChanged;
      LayoutType.SelectionChanged += LayoutTypeSelectionChanged;
      PreviewSize.SelectionChanged += PreviewSizeSizeSelectionChanged;
      NumberOfCells.GotFocus += SpanGotFocus;
      NumberOfCells.MouseDoubleClick += SpanGotFocus;
      NumberOfCells.PreviewMouseLeftButtonDown += SelectivelyIgnoreMouseButton;
      SaveButton.Click += SaveButtonClick;
      CancelButton.Click += CancelButtonClick;
    }

    /// <summary>
    /// Initializes the controls and properties.
    /// </summary>
    private void InitializeControlsAndProperties([NotNull] List<LayoutType> layoutTypes, [CanBeNull] LayoutType currentLayoutType, int componentNumberOfCell, [NotNull] string componentCellAttributes)
    {
      LayoutTypes = layoutTypes;

      if (componentNumberOfCell == 0)
      {
        componentNumberOfCell = 9;
      }

      var componentLayoutType = currentLayoutType;
      if (componentLayoutType == null)
      {
        componentLayoutType = layoutTypes[0];
      }

      foreach (var layoutType in layoutTypes)
      {
        LayoutType.Items.Add(new ComboBoxItem
        {
          Content = layoutType.Name,
          Tag = layoutType.Id,
          IsSelected = layoutType.Id.Equals(componentLayoutType.Id)
        });
      }

      CurrentLayoutType = componentLayoutType;
      TotalCells = componentNumberOfCell;
      PreviewSize.SelectedIndex = 0;
      NumberOfCells.Text = TotalCells.ToString(CultureInfo.InvariantCulture);
      CurrentPreviewSize = (Sizes)Enum.Parse(typeof(Sizes), ((ComboBoxItem)PreviewSize.SelectedItem).Tag.ToString(), true);
      CurrentPreviewSizeIndex = (int)CurrentPreviewSize;
      SetGridColumnsNumber();
      SetGridSize();
      SetSizesLabels();
      LoadData(componentCellAttributes);
    }

    /// <summary>
    /// Formats the type of the layout.
    /// </summary>
    /// <param name="layoutType">Type of the layout.</param>
    /// <returns></returns>
    private string FormatLayoutType(LayoutType layoutType)
    {
      return layoutType.Large + "-" + layoutType.Medium + "-" + layoutType.Small + "-" + layoutType.XSmall;
    }

    /// <summary>
    /// Resets the cells span.
    /// </summary>
    private void ResetGridAttributes()
    {
      GridAttributes = new GridAttributes
      {
        SizeAttributes = new List<SizeAttributes>
                {
                    new SizeAttributes
                    {
                        Size = "Large",
                        CellAttributes = new List<CellAttributes>()
                    },
                    new SizeAttributes
                    {
                        Size = "Medium",
                        CellAttributes = new List<CellAttributes>()
                    },
                    new SizeAttributes
                    {
                        Size = "Small",
                        CellAttributes = new List<CellAttributes>()
                    },
                    new SizeAttributes
                    {
                        Size = "XSmall",
                        CellAttributes = new List<CellAttributes>()
                    }
                },
        LayoutType = new Guid(((ComboBoxItem)LayoutType.SelectedItem).Tag.ToString()),
        NumberOfCells = TotalCells
      };

      for (var i = 0; i < SizesCount; i++)
      {
        GridAttributes.SizeAttributes[i].NumberOfColumns = GridColumnsNumber[i];
        for (var j = 0; j < TotalCells; j++)
        {
          GridAttributes.SizeAttributes[i].CellAttributes.Add(new CellAttributes
          {
            Index = j + 1,
            Span = 1
          });
        }
      }

      InitializeGrid();
    }

    /// <summary>
    /// Initializes the grid.
    /// </summary>
    private void InitializeGrid()
    {
      var totalColSpan = 0;
      for (var cellIndex = 0; cellIndex < TotalCells; cellIndex++)
      {
        if (GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span > GridColumnsNumber[CurrentPreviewSizeIndex])
        {
          GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span = GridColumnsNumber[CurrentPreviewSizeIndex];
        }

        totalColSpan += GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span;
      }

      var rowsNumber = totalColSpan / GridColumnsNumber[CurrentPreviewSizeIndex];
      if (totalColSpan % GridColumnsNumber[CurrentPreviewSizeIndex] > 0)
      {
        rowsNumber++;
      }

      Grid.RowDefinitions.Clear();
      Grid.ColumnDefinitions.Clear();
      Grid.Children.Clear();

      for (var rowIndex = 0; rowIndex < rowsNumber; rowIndex++)
      {
        Grid.RowDefinitions.Add(new RowDefinition());
      }

      for (var colIndex = 0; colIndex < GridColumnsNumber[CurrentPreviewSizeIndex]; colIndex++)
      {
        Grid.ColumnDefinitions.Add(new ColumnDefinition());
      }

      var row = 0;
      var col = 0;
      var lastRowIndex = 0;
      for (var cellIndex = 0; cellIndex < TotalCells; cellIndex++)
      {
        var border = new Border
        {
          Background = new SolidColorBrush(Colors.SteelBlue),
          BorderThickness = new Thickness(1),
          HorizontalAlignment = HorizontalAlignment.Stretch,
          VerticalAlignment = VerticalAlignment.Stretch
        };

        if ((col + GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span) <= GridColumnsNumber[CurrentPreviewSizeIndex])
        {
          Grid.SetRow(border, row);
          Grid.SetColumn(border, col);
          lastRowIndex = row;

          col += GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span;
          if (col >= GridColumnsNumber[CurrentPreviewSizeIndex])
          {
            col = 0;
            row++;
          }
        }
        else
        {
          col = 0;
          row++;
          Grid.SetRow(border, row);
          Grid.SetColumn(border, col);
          lastRowIndex = row;
          col = GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span;
          if (col >= GridColumnsNumber[CurrentPreviewSizeIndex])
          {
            col = 0;
            row++;
          }
        }

        var textBox = new TextBox
        {
          Text = GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span.ToString(CultureInfo.InvariantCulture),
          Width = 30,
          Height = 20,
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center,
          Tag = cellIndex.ToString(CultureInfo.InvariantCulture),
          TextAlignment = TextAlignment.Right,
          IsEnabled = true
        };
        textBox.TextChanged += SpanTextChanged;
        textBox.GotFocus += SpanGotFocus;
        textBox.MouseDoubleClick += SpanGotFocus;
        textBox.PreviewMouseLeftButtonDown += SelectivelyIgnoreMouseButton;

        border.Child = textBox;
        Grid.SetColumnSpan(border, GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Span);
        var label = new Label
        {
          Content = "Cell" + GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[cellIndex].Index,
          Foreground = new SolidColorBrush(Colors.White),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Top,
          Margin = new Thickness(2),
          FontSize = 10
        };
        Grid.SetRow(label, Grid.GetRow(border));
        Grid.SetColumn(label, Grid.GetColumn(border));
        Grid.Children.Add(border);
        Grid.Children.Add(label);
      }

      if (Grid.RowDefinitions.Count() - 1 > lastRowIndex)
      {
        for (var i = lastRowIndex + 1; i < Grid.RowDefinitions.Count(); i++)
        {
          Grid.RowDefinitions.RemoveAt(i);
        }
      }

      if (Grid.RowDefinitions.Count() - 1 < lastRowIndex)
      {
        for (var i = Grid.RowDefinitions.Count(); i == lastRowIndex; i++)
        {
          Grid.RowDefinitions.Add(new RowDefinition());
        }
      }
    }

    /// <summary>
    /// Numbers the of cells text changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
    private void NumberOfCellsTextChanged(object sender, TextChangedEventArgs e)
    {
      var textBox = sender as TextBox;
      if (textBox != null)
      {
        int number;
        var success = int.TryParse(((TextBox)sender).Text, out number);
        if (success & number >= 0)
        {
          TotalCells = number;
          ResetGridAttributes();
          InitializeGrid();
        }
        else
        {
          textBox.Text = TotalCells.ToString(CultureInfo.InvariantCulture);
        }
      }
    }

    /// <summary>
    /// Previews the size size selection changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
    private void PreviewSizeSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CurrentPreviewSize = (Sizes)Enum.Parse(typeof(Sizes), ((ComboBoxItem)PreviewSize.SelectedItem).Tag.ToString(), true);
      CurrentPreviewSizeIndex = (int)CurrentPreviewSize;
      SetGridSize();
      SetGridColumnsNumber();
      InitializeGrid();
    }

    /// <summary>
    /// Spans the text changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
    private void SpanTextChanged(object sender, TextChangedEventArgs e)
    {
      var textBox = sender as TextBox;
      if (textBox != null)
      {
        int number;
        var success = int.TryParse(((TextBox)sender).Text, out number);
        if (success & number >= 0)
        {
          if (number > GridColumnsNumber[CurrentPreviewSizeIndex])
          {
            number = GridColumnsNumber[CurrentPreviewSizeIndex];
            textBox.Text = GridColumnsNumber[CurrentPreviewSizeIndex].ToString(CultureInfo.InvariantCulture);
          }

          GridAttributes.SizeAttributes[CurrentPreviewSizeIndex].CellAttributes[Convert.ToInt32(textBox.Tag)].Span = number;
          InitializeGrid();
        }
        else
        {
          textBox.Text = "1";
        }
      }
    }

    /// <summary>
    /// Layouts the type selection changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
    private void LayoutTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var layouTypeId = ((ComboBoxItem)LayoutType.SelectedItem).Tag.ToString();
      CurrentLayoutType = LayoutTypes.Where(layoutType => layoutType.Id.ToString().Equals(layouTypeId)).First();
      SetGridColumnsNumber();
      SetSizesLabels();
      ResetGridAttributes();
    }

    /// <summary>
    /// Sets the sizes labels.
    /// </summary>
    private void SetSizesLabels()
    {

      if (GridAttributes != null)
      {
        GridAttributes.LayoutType = CurrentLayoutType.Id;
      }

      LargeLabel.Content = CurrentLayoutType.Large;
      MediumLabel.Content = CurrentLayoutType.Medium;
      SmallLabel.Content = CurrentLayoutType.Small;
      XSmallLabel.Content = CurrentLayoutType.XSmall;
    }

    /// <summary>
    /// Sets the size of the grid.
    /// </summary>
    private void SetGridSize()
    {
      switch (CurrentPreviewSize)
      {
        case Sizes.Large:
          Grid.Width = Viewport.Width - 2;
          break;
        case Sizes.Medium:
          Grid.Width = (int)((Viewport.Width / 1600) * MediumGridSize) - 2;
          break;
        case Sizes.Small:
          Grid.Width = (int)((Viewport.Width / 1600) * SmallGridSize) - 2;
          break;
        case Sizes.XSmall:
          Grid.Width = (int)((Viewport.Width / 1600) * ExtraSmallGridSize) - 2;
          break;
      }
    }

    /// <summary>
    /// Span textbox got focus handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void SpanGotFocus(object sender, RoutedEventArgs e)
    {
      var tb = sender as TextBox;

      if (tb != null)
      {
        tb.SelectAll();
      }
    }

    /// <summary>
    /// Selectively ignore mouse button handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
    private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
    {
      var tb = sender as TextBox;
      if (tb != null)
      {
        if (!tb.IsKeyboardFocusWithin)
        {
          e.Handled = true;
          tb.Focus();
        }
      }
    }

    /// <summary>
    /// Sets the grid columns number.
    /// </summary>
    private void SetGridColumnsNumber()
    {
      var values = new[] { CurrentLayoutType.Large, CurrentLayoutType.Medium, CurrentLayoutType.Small, CurrentLayoutType.XSmall };
      GridColumnsNumber = new int[SizesCount];
      for (var index = 0; index < SizesCount; index++)
      {
        GridColumnsNumber[index] = Convert.ToInt32(values[index]);
        if (GridAttributes != null)
        {
          GridAttributes.SizeAttributes[index].NumberOfColumns = Convert.ToInt32(values[index]);
        }
      }

      SetPreviewSizeLabels();
    }

    /// <summary>
    /// Sets the preview size labels.
    /// </summary>
    private void SetPreviewSizeLabels()
    {
      switch (CurrentPreviewSize)
      {
        case Sizes.Large:
          GridSizeLabel.Content = ">= 1200px";
          break;
        case Sizes.Medium:
          GridSizeLabel.Content = "992px -> 1199px";
          break;
        case Sizes.Small:
          GridSizeLabel.Content = "768px -> 991px";
          break;
        case Sizes.XSmall:
          GridSizeLabel.Content = "< 768px";
          break;
      }
    }

    /// <summary>
    /// Reads the cells data and validates it.
    /// </summary>
    /// <param name="componentCellsAttributes">The component cells attributes.</param>
    private void LoadData(string componentCellsAttributes)
    {
      try
      {
        GridAttributes = JsonConvert.DeserializeObject<GridAttributes>(componentCellsAttributes);

        ValidateCellsData();
      }
      catch (Exception e)
      {
        AppHost.Output.LogException(e);
        ResetGridAttributes();
      }
    }

    /// <summary>
    /// Validates the cells data.
    /// </summary>
    private void ValidateCellsData()
    {
      if (GridAttributes == null || GridAttributes.NumberOfCells < 1 || GridAttributes.LayoutType.Equals(Guid.Empty) || GridAttributes.SizeAttributes == null || !GridAttributes.SizeAttributes.Any())
      {
        ResetGridAttributes();
        return;
      }

      if (TotalCells != GridAttributes.NumberOfCells)
      {
        ResetGridAttributes();
        return;
      }

      if (!CurrentLayoutType.Id.Equals(GridAttributes.LayoutType))
      {
        ResetGridAttributes();
        return;
      }

      var numberOfCells = -1;
      foreach (var sizeAttribute in GridAttributes.SizeAttributes)
      {
        if (sizeAttribute == null)
        {
          ResetGridAttributes();
          return;
        }

        if (numberOfCells == -1)
        {
          numberOfCells = GridAttributes.SizeAttributes[0].CellAttributes.Count;
        }
        else if (GridAttributes.SizeAttributes[0].CellAttributes.Count != numberOfCells)
        {
          ResetGridAttributes();
          return;
        }
      }

      // Is Valid 
      InitializeGrid();
    }

    /// <summary>
    /// Save button handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
      CellAttributesValue = JsonConvert.SerializeObject(GridAttributes);
      LayoutTypeValue = CurrentLayoutType;
      NumberOfCellsValue = GridAttributes.NumberOfCells;

      DialogResult = true;
      Close();
    }

    /// <summary>
    /// Cancel button handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
      Close();
    }

    #endregion Methods
  }
}
