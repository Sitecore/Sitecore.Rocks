// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Net.Pipelines.Troubleshooter;
using Sitecore.Rocks.NewWebService;
using Sitecore.Rocks.Shell.Pipelines.LoggedIn;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.Data.DataServices
{
    [DataService(Name, Priority = 1000)]
    public class HardRockWebService : WebDataService
    {
        public const string Name = "Hard Rock Web Service";

        private SitecoreWebService2SoapClient dataService;

        public HardRockWebService()
        {
            DisableLoginErrorMessage = false;
            HasWebSite = true;

            Capabilities = DataServiceCapabilities.GetChildrenTemplateId | DataServiceCapabilities.GetItemFieldsValueList;

            FeatureCapabilities = DataServiceFeatureCapabilities.Publish | DataServiceFeatureCapabilities.RebuildLinkDatabase | DataServiceFeatureCapabilities.RebuildSearchIndex | DataServiceFeatureCapabilities.EditTemplate | DataServiceFeatureCapabilities.CopyPasteItemXml | DataServiceFeatureCapabilities.InsertOptions | DataServiceFeatureCapabilities.Projects | DataServiceFeatureCapabilities.EditLayouts | DataServiceFeatureCapabilities.Jobs | DataServiceFeatureCapabilities.Logs | DataServiceFeatureCapabilities.Execute;
        }

        [CanBeNull]
        protected internal SitecoreWebService2SoapClient DataService
        {
            get
            {
                if (dataService == null)
                {
                    InitializeDataService();
                }

                return dataService;
            }
        }

        public override bool CanExecuteAsync(string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            return true;
        }

        public override void ResetDataService()
        {
            dataService = null;
        }

        public override bool TestConnection(string physicalPath)
        {
            Assert.ArgumentNotNull(physicalPath, nameof(physicalPath));

            bool retry;
            do
            {
                retry = false;
                SitecoreWebService2SoapClient service = null;

                try
                {
                    service = GetDataService();

                    var result = service.Login(GetCredentials());

                    if (result == @"Invalid user or password.")
                    {
                        AppHost.MessageBox(Resources.HardRockWebService_TestConnection_Invalid_user_name_or_password_, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    if (service == null)
                    {
                        AppHost.MessageBox(ex.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    var pipeline = TroubleshooterPipeline.Run().WithParameters(this, true, ex, service.Endpoint);
                    if (pipeline.Cancelled)
                    {
                        return false;
                    }

                    retry = pipeline.Retry;
                }
            }
            while (retry);

            return true;
        }

        protected override bool CheckVersion()
        {
            if (dataService == null || string.IsNullOrEmpty(WebServiceVersion))
            {
                InitializeDataService();

                if (dataService == null)
                {
                    return false;
                }
            }

            var webServiceVersion = WebServiceVersion ?? string.Empty;
            var assemblyVersion = Assembly.GetExecutingAssembly().GetFileVersion().ToString();

            if (webServiceVersion == assemblyVersion)
            {
                return true;
            }

            if (DisableLoginErrorMessage)
            {
                return false;
            }

            var text = string.Format(Resources.HardRockWebService_CheckVersion_, webServiceVersion, assemblyVersion);

            var dialogResult = AppHost.MessageBox(text, Resources.Warning, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (dialogResult != MessageBoxResult.OK)
            {
                return false;
            }

            UpdateServerComponentsDialog.Execute(this, Connection.HostName, Connection.WebRootPath, null);

            return false;
        }

        protected override XmlElement ExecuteAddFromMaster(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Debug.ArgumentNotNull(parentUri, nameof(parentUri));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.AddFromMaster(parentUri.ItemId.ToString(), templateUri.ItemId.ToString(), newName, templateUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteAddFromTemplate(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Debug.ArgumentNotNull(parentUri, nameof(parentUri));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.AddFromTemplate(parentUri.ItemId.ToString(), templateUri.ItemId.ToString(), newName, templateUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecuteAddVersionAsync(ItemVersionUri uri, EventHandler<AddVersionCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var service = DataService;
            if (service == null)
            {
                return;
            }

            service.AddVersionAsync(uri.ItemUri.ItemId.ToString(), uri.Language.Name, uri.ItemUri.DatabaseName.Name, GetCredentials(), callback);
        }

        protected override XmlElement ExecuteCopyTo(ItemUri itemUri, ItemId newParentId, string newName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newParentId, nameof(newParentId));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.CopyTo(itemUri.ItemId.ToString(), newParentId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecuteCopyToAsync(ItemUri itemUri, ItemId newParentId, string newName, EventHandler<CopyToCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newParentId, nameof(newParentId));
            Debug.ArgumentNotNull(newName, nameof(newName));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var service = DataService;
            if (service == null)
            {
                return;
            }

            service.CopyToAsync(itemUri.ItemId.ToString(), newParentId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials(), callback);
        }

        protected override XmlElement ExecuteDelete(ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.Delete(itemUri.ItemId.ToString(), true, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteDuplicate(ItemUri itemUri, string newName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.Duplicate(itemUri.ItemId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecuteExecuteAsync(string typeName, object[] parameters, EventHandler<ExecuteCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(typeName, nameof(typeName));
            Debug.ArgumentNotNull(parameters, nameof(parameters));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var array = new ArrayOfAnyType();
            array.AddRange(parameters);

            var service = DataService;
            if (service == null)
            {
                var results = new object[]
                {
                    string.Empty
                };

                var args = new ExecuteCompletedEventArgs(results, new Exception(@"No Service"), true, null);

                callback(this, args);
                return;
            }

            service.ExecuteAsync(typeName, array, GetCredentials(), callback);
        }

        protected override XmlElement ExecuteGetChildren(ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.GetChildren(itemUri.ItemId.ToString(), itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override bool ExecuteGetChildrenAsync(ItemUri itemUri, EventHandler<GetChildrenCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var service = DataService;
            if (service == null)
            {
                return false;
            }

            service.GetChildrenAsync(itemUri.ItemId.ToString(), itemUri.DatabaseName.Name, GetCredentials(), callback);
            return true;
        }

        protected override XmlElement ExecuteGetDatabases()
        {
            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.GetDatabases(GetCredentials());
        }

        protected override XmlElement ExecuteGetItemFields(ItemVersionUri uri)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.GetItemFields(uri.ItemUri.ItemId.ToString(), uri.Language.Name, uri.Version.ToString(), true, uri.ItemUri.DatabaseName.Name, GetCredentials());
        }

        protected override bool ExecuteGetItemFieldsAsync(ItemVersionUri itemVersionUri, EventHandler<GetItemFieldsCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var service = DataService;
            if (service == null)
            {
                return false;
            }

            service.GetItemFieldsAsync(itemVersionUri.ItemUri.ItemId.ToString(), itemVersionUri.Language.Name, itemVersionUri.Version.ToString(), true, itemVersionUri.ItemUri.DatabaseName.Name, GetCredentials(), callback);
            return true;
        }

        protected override XmlElement ExecuteGetItemPath(DatabaseName databaseName, string itemPath)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(itemPath, nameof(itemPath));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.GetItemFields(itemPath, LanguageManager.CurrentLanguage.Name, Version.Latest.ToString(), false, databaseName.Name, GetCredentials());
        }

        protected override void ExecuteGetTemplatesAsync(DatabaseName databaseName, EventHandler<GetTemplatesCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var service = DataService;
            if (service == null)
            {
                return;
            }

            service.GetTemplatesAsync(databaseName.Name, GetCredentials(), callback);
        }

        protected override void ExecuteGetXmlAsync(ItemUri itemUri, bool deep, EventHandler<GetXMLCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var service = DataService;
            if (service == null)
            {
                return;
            }

            service.GetXMLAsync(itemUri.ItemId.ToString(), deep, itemUri.DatabaseName.Name, GetCredentials(), callback);
        }

        protected override XmlElement ExecuteMove(ItemUri itemUri, ItemId newParentId)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newParentId, nameof(newParentId));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.MoveTo(itemUri.ItemId.ToString(), newParentId.ToString(), itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecutePasteXml(ItemUri itemUri, string xml, bool changeIds)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(xml, nameof(xml));

            var service = DataService;
            if (service == null)
            {
                return;
            }

            service.InsertXMLAsync(itemUri.ItemId.ToString(), xml, changeIds, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteRemoveVersion(ItemVersionUri uri)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.RemoveVersion(uri.ItemUri.ItemId.ToString(), uri.Language.Name, uri.Version.ToString(), uri.ItemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteRename(ItemUri itemUri, string newName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.Rename(itemUri.ItemId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteSave(string xml, DatabaseName databaseName)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));

            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.Save(xml, databaseName.Name, GetCredentials());
        }

        protected override ServiceEndpoint GetEndpoint()
        {
            var service = DataService;
            if (service == null)
            {
                return null;
            }

            return service.Endpoint;
        }

        protected override string GetServiceUrl()
        {
            return "/sitecore/shell/webservice/service2.asmx";
        }

        [NotNull]
        private SitecoreWebService2SoapClient CreateDataService()
        {
            var result = GetDataService();

            result.AddVersionCompleted += (sender, args) => ((EventHandler<AddVersionCompletedEventArgs>)args.UserState)(this, args);
            result.CopyToCompleted += (sender, args) => ((EventHandler<CopyToCompletedEventArgs>)args.UserState)(this, args);
            result.ExecuteCompleted += (sender, args) => ((EventHandler<ExecuteCompletedEventArgs>)args.UserState)(this, args);
            result.GetChildrenCompleted += (sender, args) => ((EventHandler<GetChildrenCompletedEventArgs>)args.UserState)(this, args);
            result.GetItemFieldsCompleted += (sender, args) => ((EventHandler<GetItemFieldsCompletedEventArgs>)args.UserState)(this, args);
            result.GetTemplatesCompleted += (sender, args) => ((EventHandler<GetTemplatesCompletedEventArgs>)args.UserState)(this, args);
            result.GetXMLCompleted += (sender, args) => ((EventHandler<GetXMLCompletedEventArgs>)args.UserState)(this, args);

            return result;
        }

        [NotNull]
        private Credentials GetCredentials()
        {
            var credentials = new Credentials
            {
                UserName = Connection.UserName,
                Password = Connection.Password
            };

            if (Connection.UseWindowsAuth)
            {
                credentials.CustomData = "windowsauth";
            }

            return credentials;
        }

        [NotNull]
        private SitecoreWebService2SoapClient GetDataService()
        {
            var hostName = GetHostName();
            var binding = Connection.GetBinding();

            var endpointAddress = new EndpointAddress(hostName + GetServiceUrl());

            return new SitecoreWebService2SoapClient(binding, endpointAddress);
        }

        private void InitializeDataService()
        {
            if (Connection.AutomaticallyUpdate && !string.IsNullOrEmpty(Connection.WebRootPath))
            {
                UpdateServerComponentsManager.AutomaticUpdate(Connection.WebRootPath);
            }

            dataService = CreateDataService();
            var busy = true;

            var aspxAuthCookieBehavior = new AspxAuthCookieBehavior(GetHostName());
            dataService.Endpoint.Behaviors.Add(aspxAuthCookieBehavior);

            AppHost.Statusbar.SetText(string.Format(Resources.LoginWindow_ControlLoaded_Connecting_to___0______, Connection.HostName));

            EventHandler<LoginCompletedEventArgs> completed = delegate(object sender, LoginCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    if (DisableLoginErrorMessage)
                    {
                        busy = false;
                        dataService = null;
                        return;
                    }

                    Dispatcher.CurrentDispatcher.Invoke(new Action(delegate
                    {
                        var endPoint = dataService != null ? dataService.Endpoint : null;
                        var pipeline = TroubleshooterPipeline.Run().WithParameters(this, true, e.Error, endPoint);

                        if (pipeline.Retry)
                        {
                            if (dataService == null)
                            {
                                dataService = CreateDataService();
                            }

                            dataService.LoginAsync(GetCredentials());
                            return;
                        }

                        dataService = null;
                        busy = false;
                    }));

                    return;
                }

                if (e.Result == @"Invalid user or password.")
                {
                    Dispatcher.CurrentDispatcher.Invoke(new Action(ShowInvalidUserNameOrPassword));
                    dataService = null;
                    busy = false;
                    return;
                }

                WebServiceVersion = @"0.0.0.0";
                SitecoreVersionString = string.Empty;
                SitecoreVersion = RuntimeVersion.Empty;

                var root = e.Result.ToXElement();
                if (root != null)
                {
                    LoggedInPipeline.Run().WithParameters(this, root);
                }

                busy = false;
            };

            AppHost.DoEvents();

            if (dataService != null)
            {
                dataService.LoginCompleted += completed;

                var dispatcherOperation = Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => dataService.LoginAsync(GetCredentials())));

                if (!AppHost.DoEvents(ref busy))
                {
                    dispatcherOperation.Abort();

                    ServiceEndpoint endPoint = null;
                    if (dataService != null)
                    {
                        endPoint = dataService.Endpoint;
                    }

                    var pipeline = TroubleshooterPipeline.Run().WithParameters(this, true, new TimeoutException("The operation timed out after 2 minutes."), endPoint);
                    if (pipeline.Retry && dataService != null)
                    {
                        dataService.LoginAsync(GetCredentials());
                        return;
                    }

                    dataService = null;
                }
            }

            if (dataService != null)
            {
                dataService.LoginCompleted -= completed;
            }

            if (dataService != null && dataService.InnerChannel.State == CommunicationState.Opened)
            {
                Status = DataServiceStatus.Connected;
            }
            else
            {
                Status = DataServiceStatus.Failed;
            }
        }

        private void ShowInvalidUserNameOrPassword()
        {
            AppHost.MessageBox(Resources.LoginWindow_ShowInvalidUserNameOrPassword_Invalid_user_name_or_password_, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
