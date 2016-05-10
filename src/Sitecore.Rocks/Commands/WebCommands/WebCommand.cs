// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands.WebCommands.Dialogs.BrowserModalDialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Commands.WebCommands
{
    public class WebCommand : CommandBase
    {
        private Func<object, Type, bool> canExecute;

        public WebCommand([NotNull] WebDataService dataService, [NotNull] XElement commandElement)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(commandElement, nameof(commandElement));

            DataService = dataService;

            Window = string.Empty;
            WindowTitle = string.Empty;
            canExecute = (parameter, parameterType) => true;

            Text = commandElement.GetElementValue("text", "[text missing]");
            SortingValue = commandElement.GetElementValueInt("name", 1000);
            Group = commandElement.GetElementValue("group", "WebCommands");

            Request = commandElement.GetElementValue("request");
            RequireLogin = string.Compare(commandElement.GetElementValue("requirelogin", "true"), "true", StringComparison.InvariantCultureIgnoreCase) == 0;

            var windowElement = commandElement.Element("window");
            if (windowElement != null)
            {
                Window = windowElement.Value;
                WindowHeight = windowElement.GetAttributeInt("height", -1);
                WindowWidth = windowElement.GetAttributeInt("width", -1);
                WindowTitle = windowElement.GetAttributeValue("title");
            }

            var contextElement = commandElement.Element("context");
            if (contextElement != null)
            {
                AllowMultipleItems = string.Compare(contextElement.GetAttributeValue("allowmultipleitem"), "true", StringComparison.InvariantCultureIgnoreCase) == 0;
                AllowNoItems = string.Compare(contextElement.GetAttributeValue("allownoitem"), "true", StringComparison.InvariantCultureIgnoreCase) == 0;

                ParseContext(contextElement.Value);
            }

            Actions = commandElement.Element("afterexecute");
        }

        [CanBeNull]
        public XElement Actions { get; set; }

        public bool AllowMultipleItems { get; }

        public bool AllowNoItems { get; }

        [NotNull]
        public WebDataService DataService { get; }

        [NotNull]
        public string Request { get; }

        public bool RequireLogin { get; }

        [NotNull]
        public string Window { get; }

        public int WindowHeight { get; }

        [NotNull]
        public string WindowTitle { get; }

        public int WindowWidth { get; }

        public override bool CanExecute(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }

            if (!canExecute(parameter, parameter.GetType()))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var requestUrl = GetRequestUrl(Request, parameter);
            if (string.IsNullOrEmpty(requestUrl))
            {
                return;
            }

            switch (Window.ToLowerInvariant())
            {
                case "dialog":
                    ShowDialog(requestUrl, parameter);
                    break;

                case "window":
                    ShowWindow(requestUrl);
                    break;

                default:
                    RequestUrl(requestUrl);
                    break;
            }
        }

        private void ExecuteCommand([CanBeNull] object parameter, [NotNull] XElement element)
        {
            Debug.ArgumentNotNull(element, nameof(element));

            var typeName = element.GetAttributeValue("type");
            if (string.IsNullOrEmpty(typeName))
            {
                return;
            }

            var type = Type.GetType(typeName);
            if (type != null)
            {
                CommandManager.Execute(type, parameter);
            }
        }

        [NotNull]
        private string GetRequestUrl([NotNull] string url, [NotNull] object parameter)
        {
            Debug.ArgumentNotNull(url, nameof(url));
            Debug.ArgumentNotNull(parameter, nameof(parameter));

            var site = SiteManager.Sites.First(s => s.DataService == DataService);
            if (site == null)
            {
                AppHost.Output.Log("Site not found");
                return string.Empty;
            }

            var siteName = string.Empty;
            var databaseName = string.Empty;
            var itemId = string.Empty;
            var itemList = string.Empty;

            var siteSelectionContext = parameter as ISiteSelectionContext;
            if (siteSelectionContext != null)
            {
                siteName = siteSelectionContext.Site.Name;
            }

            var databaseSelectionContext = parameter as IDatabaseSelectionContext;
            if (databaseSelectionContext != null)
            {
                databaseName = databaseSelectionContext.DatabaseUri.DatabaseName.ToString();
            }

            var itemSelectionContext = parameter as IItemSelectionContext;
            if (itemSelectionContext != null)
            {
                var item = itemSelectionContext.Items.FirstOrDefault();
                if (item != null)
                {
                    itemId = item.ItemUri.ItemId.ToString();
                }

                if (itemSelectionContext.Items.Any())
                {
                    itemList = string.Join("|", itemSelectionContext.Items.Select(i => i.ItemUri.ItemId.ToString()));
                }
            }

            url = url.Replace("$sitename", siteName);
            url = url.Replace("$databasename", databaseName);
            url = url.Replace("$itemid", itemId);
            url = url.Replace("$items", itemList);
            url = url.Replace("$languagename", Language.Current.ToString());
            url = url.Replace("$username", site.Credentials.UserName);
            url = url.Replace("$password", site.Credentials.Password);

            if (RequireLogin)
            {
                url = AppHost.Browsers.GetUrl(site, url);
            }
            else if (!url.Contains(@"://"))
            {
                url = site.GetHost() + url;
            }

            return url;
        }

        private void ParseContext([NotNull] string context)
        {
            Debug.ArgumentNotNull(context, nameof(context));

            switch (context.ToLowerInvariant())
            {
                case "items":
                    context = "Sitecore.Rocks.Data.IItemSelectionContext";
                    break;
                case "databases":
                    context = "Sitecore.Rocks.Data.IDatabaseSelectionContext";
                    break;
                case "sites":
                    context = "Sitecore.Rocks.Data.ISiteSelectionContext";
                    break;
                case "sitecoreexplorer":
                    context = "Sitecore.Rocks.ContentTrees.ContentTreeContext";
                    break;
                case "itemeditor":
                    context = "Sitecore.Rocks.ContentEditors.ContentEditorContext";
                    break;
            }

            var contextType = Type.GetType(context);
            if (contextType == null)
            {
                AppHost.Output.Log("WebCommand: Context not found: " + context);
                return;
            }

            Func<object, Type, bool> test;

            if (contextType == typeof(IItemSelectionContext))
            {
                if (AllowMultipleItems && AllowNoItems)
                {
                    test = (parameter, parameterType) => typeof(IItemSelectionContext).IsAssignableFrom(parameterType);
                }
                else if (AllowMultipleItems)
                {
                    test = (parameter, parameterType) => typeof(IItemSelectionContext).IsAssignableFrom(parameterType) && ((IItemSelectionContext)parameter).Items.Any();
                }
                else if (AllowNoItems)
                {
                    test = (parameter, parameterType) => typeof(IItemSelectionContext).IsAssignableFrom(parameterType) && ((IItemSelectionContext)parameter).Items.Count() <= 1;
                }
                else
                {
                    test = (parameter, parameterType) => typeof(IItemSelectionContext).IsAssignableFrom(parameterType) && ((IItemSelectionContext)parameter).Items.Count() == 1;
                }
            }
            else if (contextType.IsInterface)
            {
                test = (parameter, parameterType) => contextType.IsAssignableFrom(parameterType);
            }
            else
            {
                test = (parameter, parameterType) => parameterType == contextType || parameterType.IsSubclassOf(contextType);
            }

            canExecute = (parameter, parameterType) => test(parameter, parameterType) && typeof(ISiteSelectionContext).IsAssignableFrom(parameterType) && ((ISiteSelectionContext)parameter).Site.DataService == DataService;
        }

        private void ProcessActions([CanBeNull] object parameter)
        {
            if (Actions == null)
            {
                return;
            }

            foreach (var element in Actions.Elements())
            {
                var action = element.Name.LocalName;

                switch (action.ToLowerInvariant())
                {
                    case "command":
                        ExecuteCommand(parameter, element);
                        break;
                }
            }
        }

        private void RequestUrl([NotNull] string requestUrl)
        {
            Debug.ArgumentNotNull(requestUrl, nameof(requestUrl));

            AppHost.Statusbar.SetText("Executing Command: " + Text);

            var r = new WebClient();
            r.DownloadString(requestUrl);
        }

        private void ShowDialog([NotNull] string requestUrl, [CanBeNull] object parameter)
        {
            Debug.ArgumentNotNull(requestUrl, nameof(requestUrl));

            var d = new BrowserModalDialog
            {
                Url = requestUrl
            };

            if (WindowHeight > 0)
            {
                d.Height = WindowHeight;
            }

            if (WindowWidth > 0)
            {
                d.Width = WindowWidth;
            }

            if (!string.IsNullOrEmpty(WindowTitle))
            {
                d.Title = WindowTitle;
            }

            d.ShowDialog();

            ProcessActions(parameter);
        }

        private void ShowWindow([NotNull] string requestUrl)
        {
            Debug.ArgumentNotNull(requestUrl, nameof(requestUrl));

            AppHost.Browsers.NavigateInternalBrowser(requestUrl);
        }
    }
}
