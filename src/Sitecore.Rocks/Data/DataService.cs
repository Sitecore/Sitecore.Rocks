// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.ServiceModel.Description;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Net.Pipelines.Troubleshooter;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.Sites.Connections;
using Sitecore.Rocks.UI.UpdateServerComponents;

namespace Sitecore.Rocks.Data
{
    public delegate void UpdateCompleted([NotNull, Localizable(false)] string result, ref byte[] file, long serverTimestamp, long serverFileSize);

    public delegate void GetValueCompleted<T>(T value);

    public delegate void GetItemsCompleted<T>([NotNull] IEnumerable<T> items);

    public delegate void GetFieldTypesCompleted([NotNull] IEnumerable<FieldTypeHeader> fieldTypes, IEnumerable<FieldValidationHeader> validations);

    public delegate void GetLinksCompleted([NotNull] string name, IEnumerable<ItemHeader> references, IEnumerable<ItemHeader> referrers);

    public delegate void CommitCompleted([NotNull] string result, long serverTimestamp, long serverFileSize);

    public delegate void ExecuteCompleted([NotNull] string response, [NotNull] ExecuteResult executeResult);

    public delegate void ExecuteStateCompleted([NotNull] string response, [NotNull] ExecuteResult executeResult, [CanBeNull] object state);

    public enum DataServiceStatus
    {
        NotConnected,

        Connected,

        Failed
    }

    public abstract class DataService
    {
        private static readonly DataService empty = new EmptyDataService();

        private DataServiceStatus status;

        protected DataService()
        {
            WebServiceVersion = string.Empty;
            SitecoreVersionString = string.Empty;
            SitecoreVersion = RuntimeVersion.Empty;
        }

        public DataServiceCapabilities Capabilities { get; protected set; }

        [NotNull]
        public Connection Connection { get; set; }

        [NotNull, Obsolete]
        public SiteCredentials Credentials { get; private set; }

        public bool DisableLoginErrorMessage { get; set; }

        [NotNull]
        public static DataService Empty
        {
            get { return empty; }
        }

        public DataServiceFeatureCapabilities FeatureCapabilities { get; protected set; }

        public bool HasWebSite { get; protected set; }

        [NotNull, Obsolete]
        public string Server { get; private set; }

        [NotNull]
        public RuntimeVersion SitecoreVersion { get; protected internal set; }

        [NotNull]
        public string SitecoreVersionString { get; protected internal set; }

        public DataServiceStatus Status
        {
            get { return status; }

            set
            {
                var previousStatus = status;
                status = value;

                Notifications.RaiseDataServiceStatusChanged(this, status, previousStatus);
            }
        }

        [CanBeNull]
        public string WebServiceVersion { get; internal set; }

        [NotNull]
        public abstract ItemUri AddFromMaster([NotNull] ItemUri parentUri, [NotNull] ItemUri templateUri, [NotNull] string newName);

        [NotNull]
        public abstract ItemUri AddFromTemplate([NotNull] ItemUri parentUri, [NotNull] ItemUri templateUri, [NotNull] string newName);

        public abstract void AddVersion([NotNull] ItemVersionUri uri, [NotNull] GetValueCompleted<Version> callback);

        public abstract bool CanExecuteAsync([Localizable(false), NotNull] string typeName);

        public abstract bool CheckDataService();

        public static bool CheckFileSize([NotNull] string fileName, [NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(connection, nameof(connection));

            var info = new FileInfo(fileName);

            var maxLength = connection.GetBinding().MaxReceivedMessageSize;

            if (info.Length <= maxLength)
            {
                return true;
            }

            AppHost.MessageBox(string.Format(Resources.MediaManager_Upload_The_file___0___is_too_big__more_than__1___and_cannot_be_uploaded_, Path.GetFileName(fileName), maxLength.ToString(@"#,##0")), Resources.MediaManager_Upload_Upload, MessageBoxButton.OK, MessageBoxImage.Information);

            return false;
        }

        [NotNull]
        public abstract ItemUri Copy([NotNull] ItemUri itemUri, [NotNull] ItemId newParentId, [NotNull] string newName);

        public abstract bool Delete([NotNull] ItemUri itemUri);

        [NotNull]
        public abstract ItemUri Duplicate([NotNull] ItemUri itemUri, [NotNull] string newName);

        public abstract void ExecuteAsync([NotNull, Localizable(false)] string typeName, [NotNull] ExecuteCompleted executeCompleted, [NotNull, Localizable(false)] params object[] parameters);

        public abstract void ExecuteAsync([NotNull, Localizable(false)] string typeName, [CanBeNull] object state, [NotNull] ExecuteStateCompleted executeCompleted, [NotNull, Localizable(false)] params object[] parameters);

        public abstract bool GetChildrenAsync([NotNull] ItemUri itemUri, [NotNull] GetItemsCompleted<ItemHeader> getChildrenCallback);

        [NotNull]
        public abstract IEnumerable<DatabaseInfo> GetDatabases();

        [NotNull]
        public abstract Item GetItemFields([NotNull] ItemVersionUri uri);

        public abstract bool GetItemFieldsAsync([NotNull] ItemVersionUri uri, [NotNull] GetValueCompleted<Item> getItemFieldsCallback);

        [NotNull]
        public abstract IEnumerable<ItemPath> GetItemPath([NotNull] DatabaseUri databaseUri, [NotNull] string itemPath);

        public abstract void GetItemXmlAsync([NotNull] ItemUri itemUri, bool deep, [NotNull] GetValueCompleted<string> getItemXmlCallback);

        [NotNull]
        public abstract IEnumerable<ItemHeader> GetRootItems([NotNull] DatabaseUri databaseUri);

        [CanBeNull]
        public abstract Control GetSiteEditorControl();

        public abstract void GetTemplatesAsync([NotNull] DatabaseUri databaseUri, [NotNull] GetItemsCompleted<TemplateHeader> getTemplatesCallback);

        [CanBeNull]
        public abstract BaseTreeViewItem GetTreeViewItem([NotNull] Site site);

        public static bool HandleExecute([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Assert.ArgumentNotNull(response, nameof(response));
            Assert.ArgumentNotNull(executeResult, nameof(executeResult));

            return HandleExecute(response, executeResult, false);
        }

        public static bool HandleExecute([NotNull] string response, [NotNull] ExecuteResult executeResult, bool silent)
        {
            Assert.ArgumentNotNull(response, nameof(response));
            Assert.ArgumentNotNull(executeResult, nameof(executeResult));

            if (executeResult.Cancelled)
            {
                return false;
            }

            if (executeResult.Error != null)
            {
                TroubleshooterPipeline.Run().WithParameters(executeResult.DataService, false, executeResult.Error, executeResult.DataService.GetEndpoint(), silent);
                return false;
            }

            if (!response.Contains(@"***ERROR***"))
            {
                return true;
            }

            var message = response.Mid(12).Trim();
            AppHost.Usage.ReportServerError(message);

            if (message == @"The system cannot find the file specified. (Exception from HRESULT: 0x80070002)")
            {
                if (silent)
                {
                    return false;
                }

                if (AppHost.MessageBox(Resources.DataService_HandleExecute_MissingFile, Resources.Error, MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK)
                {
                    return false;
                }

                var d = new UpdateServerComponentsDialog();
                d.Initialize(executeResult.DataService, string.Empty, string.Empty, null, null);
                AppHost.Shell.ShowDialog(d);

                return false;
            }

            if (!silent)
            {
                AppHost.MessageBox(message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        [Obsolete]
        public virtual void Initialize([NotNull] string server, [NotNull] SiteCredentials credentials)
        {
            Assert.ArgumentNotNull(server, nameof(server));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            Server = server;
            Credentials = credentials;
        }

        public virtual void Initialize([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            Connection = connection;
        }

        public abstract bool Move([NotNull] ItemUri itemUri, [NotNull] ItemId newParentId);

        public abstract void PasteXml([NotNull] ItemUri itemUri, [NotNull] string xml, bool changeIds);

        [NotNull]
        public abstract Version RemoveVersion([NotNull] ItemVersionUri uri);

        public abstract bool Rename([NotNull] ItemUri itemUri, [NotNull] string newName);

        public abstract void ResetDataService();

        public abstract bool Save([NotNull] DatabaseName databaseName, [NotNull] List<Field> fields);

        public abstract bool TestConnection([NotNull] string physicalPath);

        [CanBeNull]
        protected virtual ServiceEndpoint GetEndpoint()
        {
            return null;
        }
    }
}
