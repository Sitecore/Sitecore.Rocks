// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.UserControlExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.QueryAnalyzers.Commands;
using Sitecore.Rocks.UI.QueryAnalyzers.Dialogs;

namespace Sitecore.Rocks.UI.QueryAnalyzers
{
    public partial class QueryAnalyzer : ISavable, IContextProvider
    {
        private DatabaseUri _databaseUri;

        public QueryAnalyzer()
        {
            DataTables = new List<ResultDataTable>();
            DataGrids = new List<DataGrid>();

            InitializeComponent();

            SyntaxEditor.Text = @"help;";
            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return _databaseUri; }

            private set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _databaseUri = value;

                ExecuteCompleted loadQuerySyntax = delegate(string response, ExecuteResult result)
                {
                    if (!DataService.HandleExecute(response, result))
                    {
                        return;
                    }

                    SyntaxEditor.LoadQuerySyntax(response);
                };
                _databaseUri.Site.DataService.ExecuteAsync("QueryAnalyzer.GetKeywords", loadQuerySyntax);
            }
        }

        [NotNull]
        public List<DataGrid> DataGrids { get; set; }

        [NotNull]
        public List<ResultDataTable> DataTables { get; set; }

        [NotNull]
        public IEditorPane Pane { get; set; }

        [CanBeNull]
        public string ScriptFileName { get; set; }

        [CanBeNull]
        protected DataGrid ActiveDataGrid { get; set; }

        protected bool IsLoading { get; set; }

        public void AppendScript([NotNull] string script)
        {
            Assert.ArgumentNotNull(script, nameof(script));

            var text = SyntaxEditor.Text;

            if (text == @"help;")
            {
                text = script;
            }
            else
            {
                text += '\n' + script;
            }

            SyntaxEditor.Text = text;
        }

        public void Execute()
        {
            if (!ExecuteButton.IsEnabled)
            {
                return;
            }

            var script = SyntaxEditor.SelectedText;

            if (string.IsNullOrEmpty(script))
            {
                script = SyntaxEditor.Text;
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                ExecuteButton.IsEnabled = true;

                if (!DataService.HandleExecute(response, result, true))
                {
                    Clear();

                    if (response.StartsWith(@"***ERROR***"))
                    {
                        response = response.Mid(12);
                    }

                    if (response.StartsWith(@"Exception has been thrown by the target of an invocation."))
                    {
                        response = response.Mid(58);
                    }

                    Messages.Text = response;
                    ShowTabs();
                    SetCaret(response);
                    return;
                }

                LoadTables(response);
            };

            script = script.Replace("\r\n", "\n");

            ExecuteButton.IsEnabled = false;

            DatabaseUri.Site.DataService.ExecuteAsync("QueryAnalyzer.Run", c, DatabaseUri.DatabaseName.ToString(), string.Empty, script, "0");
        }

        [NotNull]
        public object GetContext()
        {
            var selectedItems = new List<IItem>();

            if (ActiveDataGrid != null)
            {
                foreach (var selectedItem in ActiveDataGrid.SelectedItems)
                {
                    var dataRowView = selectedItem as DataRowView;
                    if (dataRowView == null)
                    {
                        Trace.Expected(typeof(DataRowView));
                        continue;
                    }

                    var item = dataRowView.Row as IItem;
                    if (item == null)
                    {
                        Trace.Expected(typeof(IItem));
                        continue;
                    }

                    selectedItems.Add(item);
                }
            }

            var script = SyntaxEditor.Text;

            return new QueryAnalyzerContext(this, script, selectedItems);
        }

        public void Initialize([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            SetDatabaseUri(databaseUri);
        }

        public void Save()
        {
            var databaseUri = DatabaseUri;
            var fields = new List<Tuple<FieldUri, string>>();

            foreach (var dataTable in DataTables)
            {
                foreach (ResultDataRow dataRow in dataTable.Rows)
                {
                    if (dataRow.RowState == DataRowState.Unchanged)
                    {
                        continue;
                    }

                    for (var index = 0; index < dataRow.ItemArray.Length; index++)
                    {
                        var fieldUri = dataRow.FieldArray[index];
                        if (fieldUri == null)
                        {
                            continue;
                        }

                        var value = dataRow.ItemArray[index];
                        if (value == null)
                        {
                            continue;
                        }

                        var v = value.ToString();

                        if (v == "\b")
                        {
                            v = string.Empty;
                        }

                        var field = new Tuple<FieldUri, string>(fieldUri, v);

                        fields.Add(field);
                    }
                }
            }

            ItemModifier.Edit(databaseUri, fields);
        }

        public void SetDatabaseUri([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            DatabaseUri = databaseUri;
            Pane.Caption = $@"Query Analyzer [{databaseUri.DatabaseName}/{databaseUri.Site.Name}]";
            Clear();
        }

        public void SetScript([NotNull] string script)
        {
            Assert.ArgumentNotNull(script, nameof(script));

            SyntaxEditor.Text = script;
        }

        public void ShowMacroScriptHelp()
        {
            ScriptMacroHelp.Visibility = Visibility.Visible;
        }

        private void Clear()
        {
            ActiveDataGrid = null;

            foreach (var dataTable in DataTables)
            {
                dataTable.RowChanged -= DataTableOnRowChanged;
            }

            foreach (var dataGrid in DataGrids)
            {
                dataGrid.BeginningEdit -= OnBeginningEdit;
            }

            DataTables.Clear();
            DataGrids.Clear();
            Messages.Text = string.Empty;

            Grids.RowDefinitions.Clear();
            Grids.Children.Clear();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;
            Keyboard.Focus(SyntaxEditor);
        }

        private void DataTableOnRowChanged([NotNull] object sender, [NotNull] DataRowChangeEventArgs dataRowChangeEventArgs)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(dataRowChangeEventArgs, nameof(dataRowChangeEventArgs));

            if (IsLoading)
            {
                return;
            }

            // this.Pane.SetModifiedFlag(true);
            Save();
        }

        private void Execute([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Execute();
        }

        [CanBeNull]
        private XElement GetRoot([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            try
            {
                return XDocument.Parse(response).Root;
            }
            catch (Exception ex)
            {
                Trace.Catch(ex);
                return null;
            }
        }

        private void HandleKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Execute();
                e.Handled = true;
            }
        }

        private void HandleRightMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
        }

        private void InsertFields([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new InsertFieldsDialog(DatabaseUri);
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var s in d.SelectedFieldNames)
            {
                if (sb.Length > 0)
                {
                    sb.Append(@", ");
                }

                sb.Append(s);
            }

            SyntaxEditor.ReplaceSelectedText(sb.ToString());
        }

        private void InsertPath([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var d = new SelectItemDialog();

            d.Initialize(Rocks.Resources.QueryAnalyzer_InsertPath_Insert_Path, new ItemUri(DatabaseUri, new ItemId(DatabaseTreeViewItem.RootItemGuid)));
            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            d.GetSelectedItemPath(delegate(string itemPath)
            {
                var sb = new StringBuilder();
                foreach (var s in itemPath.Split('/'))
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }

                    var item = s;
                    if (Regex.IsMatch(item, @"\W"))
                    {
                        item = '#' + item + '#';
                    }

                    sb.Append('/');
                    sb.Append(item);
                }

                SyntaxEditor.ReplaceSelectedText(sb.ToString());
            });
        }

        private void LoadColumns([NotNull] ResultDataTable dataTable, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));
            Debug.ArgumentNotNull(element, nameof(element));

            var columnsElement = element.Element(@"columns");
            if (columnsElement == null)
            {
                return;
            }

            foreach (var e in columnsElement.Elements())
            {
                var dataColumn = new DataColumn(e.GetAttributeValue("name"), typeof(string))
                {
                    ReadOnly = e.GetAttributeValue("isreadonly") == @"true"
                };

                dataTable.Columns.Add(dataColumn);
            }
        }

        private void LoadGridSplitter(int count)
        {
            Grids.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(3)
            });

            var gridSplitter = new GridSplitter
            {
                ResizeDirection = GridResizeDirection.Rows,
                Height = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0),
                Background = SystemColors.ControlBrush
            };

            Grids.Children.Add(gridSplitter);
            gridSplitter.SetValue(Grid.RowProperty, count);
        }

        private void LoadRows([NotNull] ResultDataTable dataTable, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(dataTable, nameof(dataTable));
            Debug.ArgumentNotNull(element, nameof(element));

            var itemsElement = element.Element(@"rows");
            if (itemsElement == null)
            {
                return;
            }

            if (!itemsElement.Elements().Any())
            {
                var dataRow = (ResultDataRow)dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
                return;
            }

            foreach (var item in itemsElement.Elements())
            {
                var fieldArray = new List<FieldUri>();
                var columnArray = new List<string>();

                var languageName = item.GetAttributeValue("language", LanguageManager.CurrentLanguage.Name) ?? "en";

                var itemUri = new ItemUri(DatabaseUri, new ItemId(new Guid(item.GetAttributeValue("id"))));
                var itemVersionUri = new ItemVersionUri(itemUri, new Language(languageName), Data.Version.Latest);

                foreach (var field in item.Elements())
                {
                    FieldUri fieldUri = null;

                    var id = field.GetAttributeValue("id");
                    if (!string.IsNullOrEmpty(id))
                    {
                        fieldUri = new FieldUri(itemVersionUri, new FieldId(new Guid(id)));
                    }

                    fieldArray.Add(fieldUri);
                    columnArray.Add(field.Value);
                }

                var dataRow = (ResultDataRow)dataTable.NewRow();

                dataRow.Name = item.GetAttributeValue("name");
                dataRow.Icon = new Icon(DatabaseUri.Site, item.GetAttributeValue("icon"));
                dataRow.ItemUri = itemUri;
                dataRow.FieldArray = fieldArray.ToArray();
                dataRow.ItemArray = columnArray.ToArray();

                dataTable.Rows.Add(dataRow);
            }
        }

        [NotNull]
        private FrameworkElement LoadTable([NotNull] XElement element)
        {
            // data table
            Debug.ArgumentNotNull(element, nameof(element));

            var dataTable = new ResultDataTable
            {
                TableName = @"Table"
            };

            dataTable.RowChanged += DataTableOnRowChanged;

            LoadColumns(dataTable, element);
            LoadRows(dataTable, element);

            dataTable.AcceptChanges();

            DataTables.Add(dataTable);

            // datagrid
            var dataGrid = new DataGrid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                RowHeaderWidth = 0,
                CanUserReorderColumns = false,
                CanUserSortColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                Tag = dataTable
            };

            dataGrid.BeginningEdit += OnBeginningEdit;
            dataGrid.PreviewMouseRightButtonDown += HandleRightMouseDown;

            DataGrids.Add(dataGrid);

            dataGrid.ItemsSource = dataTable.DefaultView;

            return dataGrid;
        }

        private void LoadTables([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            IsLoading = true;
            try
            {
                Clear();

                var root = GetRoot(response);
                if (root == null)
                {
                    return;
                }

                var rowCount = 0;
                var elements = root.Elements().ToList();
                var maxHeight = elements.Count(element => element.Element(@"columns") == null) > 1 ? 400 : -1;
                var messages = new StringBuilder();

                foreach (var element in elements)
                {
                    if (rowCount > 0)
                    {
                        LoadGridSplitter(rowCount);
                        rowCount++;
                    }

                    var valueElement = element.Element(@"value");

                    if (valueElement != null)
                    {
                        messages.AppendLine(valueElement.Value);
                        messages.AppendLine(string.Empty);
                    }
                    else
                    {
                        var frameworkElement = LoadTable(element);

                        var rowDefinition = new RowDefinition();

                        if (maxHeight > 0)
                        {
                            frameworkElement.MinHeight = 200;
                            rowDefinition.Height = new GridLength(1, GridUnitType.Star);
                        }

                        Grids.RowDefinitions.Add(rowDefinition);
                        Grids.Children.Add(frameworkElement);

                        frameworkElement.SetValue(Grid.RowProperty, rowCount);
                        rowCount++;
                    }
                }

                Messages.Text = messages.ToString();

                ShowTabs();
                Pane.SetModifiedFlag(false);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnBeginningEdit([NotNull] object sender, DataGridBeginningEditEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var rowIndex = e.Row.GetIndex();
            var columnIndex = e.Column.DisplayIndex;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
            {
                e.Cancel = true;
                return;
            }

            var dataTable = dataGrid.Tag as DataTable;
            if (dataTable == null)
            {
                Trace.Expected(typeof(DataTable));
                e.Cancel = true;
                return;
            }

            var row = dataTable.Rows[rowIndex] as ResultDataRow;
            if (row == null)
            {
                Trace.Expected(typeof(ResultDataRow));
                e.Cancel = true;
                return;
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (row.FieldArray == null)
            {
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                e.Cancel = true;
                return;
            }

            var field = row.FieldArray[columnIndex];

            e.Cancel = field == null;
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenuPane.ContextMenu = null;
            ActiveDataGrid = null;

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var dataGrid = frameworkElement.GetAncestorOrSelf<DataGrid>();
                if (dataGrid != null)
                {
                    ActiveDataGrid = dataGrid;
                }
            }

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context).ToList();
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenuPane.ContextMenu = contextMenu;
        }

        private void OpenMenu([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            var contextMenu = ContextMenuExtensions.GetContextMenu(context);
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.PlacementTarget = Menu;
            contextMenu.IsOpen = true;
        }

        private void OpenSamples([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Browsers.Navigate("https://github.com/Sitecore/Sitecore.Rocks/blob/master/docs/QueryAnalyzer/QueryAnalyzerSamples.md");
        }

        private void OpenScript([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rocks.Commands.CommandManager.Execute(typeof(OpenScript), GetContext());
        }

        private void OpenXpathBuilder([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Windows.Factory.OpenXpathBuilder(DatabaseUri);
        }

        private void SaveScript([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rocks.Commands.CommandManager.Execute(typeof(SaveScript), GetContext());
        }

        private void SetCaret([NotNull] string response)
        {
            Debug.ArgumentNotNull(response, nameof(response));

            var n = response.IndexOf(@"expected at position", StringComparison.Ordinal);
            if (n < 0)
            {
                return;
            }

            var text = response.Mid(n + 20);
            if (text.EndsWith(@"."))
            {
                text = text.Left(text.Length - 1);
            }

            int pos;
            if (!int.TryParse(text, out pos))
            {
                return;
            }

            SyntaxEditor.CaretOffset = pos + 1;
        }

        private void ShowTabs()
        {
            if (Grids.Children.Count == 0 && !string.IsNullOrEmpty(Messages.Text))
            {
                MessageTab.IsSelected = true;
            }
            else
            {
                ResultTab.IsSelected = true;
            }
        }

        private void ToolBarLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.InitializeToolBar(sender);
        }

        public class ResultDataRow : DataRow, IItem
        {
            protected internal ResultDataRow([NotNull] DataRowBuilder builder) : base(builder)
            {
                Debug.ArgumentNotNull(builder, nameof(builder));

                Icon = Icon.Empty;
                ItemUri = ItemUri.Empty;
                Name = string.Empty;
                Language = string.Empty;
            }

            [NotNull]
            public FieldUri[] FieldArray { get; set; }

            public Icon Icon { get; set; }

            public ItemUri ItemUri { get; set; }

            public string Language { get; set; }

            public string Name { get; set; }
        }

        public class ResultDataTable : DataTable
        {
            protected override Type GetRowType()
            {
                return typeof(ResultDataRow);
            }

            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                Debug.ArgumentNotNull(builder, nameof(builder));
                return new ResultDataRow(builder);
            }
        }
    }
}
