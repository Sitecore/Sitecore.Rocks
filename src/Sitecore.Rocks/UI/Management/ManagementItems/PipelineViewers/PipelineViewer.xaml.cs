// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.PipelineViewers
{
    [Management(ItemName, 2000)]
    public partial class PipelineViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Pipelines";

        public PipelineViewer()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public SiteManagementContext Context { get; set; }

        public XElement PipelinesElement { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var site = context as SiteManagementContext;
            if (site == null)
            {
                return false;
            }

            return site.Site.DataService.CanExecuteAsync("ContentTrees.GetWebConfig");
        }

        [NotNull]
        public object GetContext()
        {
            return new PipelineViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        public void Refresh()
        {
            LoadPipelines();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadPipelines();
        }

        private void LoadPipelines()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(PipelineList);
                ProcessorList.IsEnabled = true;

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var pipelines = element.Element("pipelines");
                if (pipelines == null)
                {
                    return;
                }

                PipelinesElement = pipelines;

                RenderPipelines(pipelines);
            };

            ProcessorList.ItemsSource = null;
            Loading.ShowLoading(PipelineList);
            Context.Site.DataService.ExecuteAsync("ContentTrees.GetWebConfig", callback);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void PipelineChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ProcessorList.ItemsSource = null;

            var listBoxItem = PipelineList.SelectedItem as Pipeline;
            if (listBoxItem == null)
            {
                return;
            }

            var pipeline = listBoxItem.Element;
            if (pipeline == null)
            {
                return;
            }

            var list = new List<Processor>();
            var index = 1;

            foreach (var element in pipeline.Elements())
            {
                var typeName = element.GetAttributeValue("type");
                var className = typeName;
                var method = element.GetAttributeValue("method");
                var assembly = string.Empty;

                var n = className.IndexOf(',');
                if (n >= 0)
                {
                    assembly = className.Mid(n + 1);
                    className = className.Left(n);
                }

                var processor = new Processor
                {
                    Index = index,
                    Assembly = assembly,
                    Class = className,
                    Method = method,
                    TypeName = typeName
                };

                list.Add(processor);

                index++;
            }

            ProcessorList.ItemsSource = list;

            ResizeGridViewColumn(ClassColumn);
            ResizeGridViewColumn(AssemblyColumn);
            ResizeGridViewColumn(MethodColumn);
        }

        private void RenderPipelines([NotNull] XElement pipelines)
        {
            Debug.ArgumentNotNull(pipelines, nameof(pipelines));

            var list = new List<Pipeline>();

            foreach (var element in pipelines.Elements().OrderBy(e => e.Name.ToString()))
            {
                var pipeline = new Pipeline
                {
                    Name = element.Name.ToString(),
                    Element = element
                };

                list.Add(pipeline);
            }

            PipelineList.ItemsSource = list;
        }

        private void ResizeGridViewColumn([NotNull] GridViewColumn column)
        {
            Debug.ArgumentNotNull(column, nameof(column));

            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        public class Pipeline
        {
            public XElement Element { get; set; }

            public string Name { get; set; }
        }

        public class Processor
        {
            public string Assembly { get; set; }

            public string Class { get; set; }

            public int Index { get; set; }

            public string Method { get; set; }

            public string TypeName { get; set; }
        }
    }
}
