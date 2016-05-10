// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Net.Pipelines.Troubleshooter;
using Sitecore.Rocks.NewWebService;
using Sitecore.Rocks.SitecoreWebService;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.Data.DataServices
{
    [DataService("Good Old Web Service", Priority = 2000)]
    public class OldWebService : WebDataService
    {
        private VisualSitecoreServiceSoapClient dataService;

        public OldWebService()
        {
            HasWebSite = true;
            FeatureCapabilities = DataServiceFeatureCapabilities.CopyPasteItemXml;
        }

        [NotNull]
        protected VisualSitecoreServiceSoapClient DataService
        {
            get
            {
                if (dataService == null)
                {
                    var server = GetHostName();

                    var binding = Connection.GetBinding();
                    /*
          var transport = BasicHttpSecurityMode.None;
          if (server.IndexOf(@"https://", StringComparison.OrdinalIgnoreCase) >= 0)
          {
            transport = BasicHttpSecurityMode.Transport;
          }

          var binding = new BasicHttpBinding(transport)
          {
            MaxReceivedMessageSize = 1024 * 1024 * 16, 
            ReaderQuotas =
              {
                MaxStringContentLength = 1024 * 1024 * 16
              }, 
          };
          */

                    dataService = new VisualSitecoreServiceSoapClient(binding, new EndpointAddress(server + GetServiceUrl()));

                    dataService.GetXMLCompleted += GetXmlCompleted;

                    dataService.GetChildrenCompleted += delegate(object sender, SitecoreWebService.GetChildrenCompletedEventArgs args)
                    {
                        var results = args.Error != null ? new object[0] : new object[]
                        {
                            args.Result
                        };

                        var a = new NewWebService.GetChildrenCompletedEventArgs(results, args.Error, args.Cancelled, null);
                        ((EventHandler<NewWebService.GetChildrenCompletedEventArgs>)args.UserState)(this, a);
                    };

                    dataService.GetItemFieldsCompleted += delegate(object sender, SitecoreWebService.GetItemFieldsCompletedEventArgs args)
                    {
                        var results = args.Error != null ? new object[0] : new object[]
                        {
                            args.Result
                        };
                        var a = new NewWebService.GetItemFieldsCompletedEventArgs(results, args.Error, args.Cancelled, null);
                        ((EventHandler<NewWebService.GetItemFieldsCompletedEventArgs>)args.UserState)(this, a);
                    };

                    dataService.GetXMLCompleted += delegate(object sender, SitecoreWebService.GetXMLCompletedEventArgs args)
                    {
                        var results = args.Error != null ? new object[0] : new object[]
                        {
                            args.Result
                        };
                        var a = new NewWebService.GetXMLCompletedEventArgs(results, args.Error, args.Cancelled, null);
                        ((EventHandler<NewWebService.GetXMLCompletedEventArgs>)args.UserState)(this, a);
                    };

                    dataService.AddVersionCompleted += delegate(object sender, SitecoreWebService.AddVersionCompletedEventArgs args)
                    {
                        var results = args.Error != null ? new object[0] : new object[]
                        {
                            args.Result
                        };
                        var a = new NewWebService.AddVersionCompletedEventArgs(results, args.Error, args.Cancelled, null);
                        ((EventHandler<NewWebService.AddVersionCompletedEventArgs>)args.UserState)(this, a);
                    };

                    dataService.CopyToCompleted += delegate(object sender, SitecoreWebService.CopyToCompletedEventArgs args)
                    {
                        var results = args.Error != null ? new object[0] : new object[]
                        {
                            args.Result
                        };
                        var a = new NewWebService.CopyToCompletedEventArgs(results, args.Error, args.Cancelled, null);
                        ((EventHandler<NewWebService.CopyToCompletedEventArgs>)args.UserState)(this, a);
                    };

                    Status = DataServiceStatus.Connected;
                }

                return dataService;
            }
        }

        public override bool CanExecuteAsync(string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            return false;
        }

        public bool InstallHardRockWebService(bool showMessage = true)
        {
            if (showMessage)
            {
                if (AppHost.MessageBox(Resources.OldWebService_InstallHardRockWebService_, Resources.Information, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            UpdateServerComponentsDialog.Execute(this, Connection.HostName, Connection.WebRootPath, null);

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
                try
                {
                    var xmlElement = DataService.GetDatabases(GetCredentials());

                    if (string.IsNullOrEmpty(xmlElement.InnerXml))
                    {
                        AppHost.MessageBox(Resources.OldWebService_TestConnection_, Resources.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    var pipeline = TroubleshooterPipeline.Run().WithParameters(this, true, ex, DataService.Endpoint);
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
            return true;
        }

        protected override XmlElement ExecuteAddFromMaster(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Debug.ArgumentNotNull(parentUri, nameof(parentUri));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            return DataService.AddFromMaster(parentUri.ItemId.ToString(), templateUri.ItemId.ToString(), newName, templateUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteAddFromTemplate(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Debug.ArgumentNotNull(parentUri, nameof(parentUri));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            return DataService.AddFromTemplate(parentUri.ItemId.ToString(), templateUri.ItemId.ToString(), newName, templateUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecuteAddVersionAsync(ItemVersionUri uri, EventHandler<NewWebService.AddVersionCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            DataService.AddVersionAsync(uri.ItemId.ToString(), uri.Language.Name, uri.ItemUri.DatabaseName.Name, GetCredentials(), callback);
        }

        [NotNull]
        protected override XmlElement ExecuteCopyTo(ItemUri itemUri, ItemId newParentId, string newName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newParentId, nameof(newParentId));
            Debug.ArgumentNotNull(newName, nameof(newName));

            return DataService.CopyTo(itemUri.ItemId.ToString(), newParentId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecuteCopyToAsync(ItemUri itemUri, ItemId newParentId, string newName, EventHandler<NewWebService.CopyToCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newParentId, nameof(newParentId));
            Debug.ArgumentNotNull(newName, nameof(newName));
            Debug.ArgumentNotNull(callback, nameof(callback));

            DataService.CopyToAsync(itemUri.ItemId.ToString(), newParentId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials(), callback);
        }

        protected override XmlElement ExecuteDelete(ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            return DataService.Delete(itemUri.ItemId.ToString(), true, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteDuplicate(ItemUri itemUri, string newName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            return DataService.Duplicate(itemUri.ItemId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecuteExecuteAsync(string typeName, object[] parameters, EventHandler<ExecuteCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(typeName, nameof(typeName));
            Debug.ArgumentNotNull(parameters, nameof(parameters));
            Debug.ArgumentNotNull(callback, nameof(callback));

            // AppHost.MessageBox("This operation is not supported by the Good Old data provider.\n\nPlease change the connection to use the Hard Rock data provider.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            var results = new object[]
            {
                string.Empty
            };

            var args = new ExecuteCompletedEventArgs(results, null, false, null);

            callback(this, args);
        }

        protected override XmlElement ExecuteGetChildren(ItemUri itemUri)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));

            return DataService.GetChildren(itemUri.ItemId.ToString(), itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override bool ExecuteGetChildrenAsync(ItemUri itemUri, EventHandler<NewWebService.GetChildrenCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            DataService.GetChildrenAsync(itemUri.ItemId.ToString(), itemUri.DatabaseName.Name, GetCredentials(), callback);
            return true;
        }

        protected override XmlElement ExecuteGetDatabases()
        {
            return DataService.GetDatabases(GetCredentials());
        }

        [NotNull]
        protected override XmlElement ExecuteGetItemFields(ItemVersionUri uri)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));

            return DataService.GetItemFields(uri.ItemUri.ItemId.ToString(), uri.Language.Name, uri.Version.ToString(), true, uri.ItemUri.DatabaseName.Name, GetCredentials());
        }

        protected override bool ExecuteGetItemFieldsAsync(ItemVersionUri itemVersionUri, EventHandler<NewWebService.GetItemFieldsCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            DataService.GetItemFieldsAsync(itemVersionUri.ItemUri.ItemId.ToString(), itemVersionUri.Language.Name, itemVersionUri.Version.ToString(), true, itemVersionUri.ItemUri.DatabaseName.Name, GetCredentials(), callback);
            return true;
        }

        protected override XmlElement ExecuteGetItemPath(DatabaseName databaseName, string itemPath)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(itemPath, nameof(itemPath));

            return DataService.GetItemFields(itemPath, LanguageManager.CurrentLanguage.Name, Version.Latest.ToString(), false, databaseName.Name, GetCredentials());
        }

        protected override void ExecuteGetTemplatesAsync(DatabaseName databaseName, EventHandler<NewWebService.GetTemplatesCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var result = DataService.GetTemplates(databaseName.Name, GetCredentials());

            var xmlElements = new object[]
            {
                result
            };

            var args = new NewWebService.GetTemplatesCompletedEventArgs(xmlElements, null, false, null);

            callback(this, args);
        }

        protected override void ExecuteGetXmlAsync(ItemUri itemUri, bool deep, EventHandler<NewWebService.GetXMLCompletedEventArgs> callback)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(callback, nameof(callback));

            DataService.GetXMLAsync(itemUri.ItemId.ToString(), deep, itemUri.DatabaseName.Name, GetCredentials(), callback);
        }

        protected override XmlElement ExecuteMove(ItemUri itemUri, ItemId newParentId)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newParentId, nameof(newParentId));

            return DataService.MoveTo(itemUri.ItemId.ToString(), newParentId.ToString(), itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override void ExecutePasteXml(ItemUri itemUri, string xml, bool changeIds)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(xml, nameof(xml));

            InstallHardRockWebService();
        }

        protected override XmlElement ExecuteRemoveVersion(ItemVersionUri uri)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));

            return DataService.RemoveVersion(uri.ItemUri.ItemId.ToString(), uri.Language.Name, uri.Version.ToString(), uri.ItemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteRename(ItemUri itemUri, string newName)
        {
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(newName, nameof(newName));

            return DataService.Rename(itemUri.ItemId.ToString(), newName, itemUri.DatabaseName.Name, GetCredentials());
        }

        protected override XmlElement ExecuteSave(string xml, DatabaseName databaseName)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));

            return DataService.Save(xml, databaseName.Name, GetCredentials());
        }

        protected override ServiceEndpoint GetEndpoint()
        {
            return DataService.Endpoint;
        }

        protected override string GetServiceUrl()
        {
            return "/sitecore/shell/webservice/service.asmx";
        }

        [NotNull]
        private SitecoreWebService.Credentials GetCredentials()
        {
            return new SitecoreWebService.Credentials
            {
                UserName = Connection.UserName,
                Password = Connection.Password
            };
        }

        private void GetXmlCompleted([NotNull] object sender, [NotNull] SitecoreWebService.GetXMLCompletedEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(args, nameof(args));

            var callback = args.UserState as EventHandler<SitecoreWebService.GetXMLCompletedEventArgs>;
            if (callback == null)
            {
                return;
            }

            callback(this, args);
        }
    }
}
