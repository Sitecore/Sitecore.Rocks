// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Sites
{
    public class Site
    {
        public delegate void RequestCompleted([NotNull] string response);

        private SiteCredentials _credentials;

        private System.Version _version;

        static Site()
        {
            Empty = new Site(new Connection());
        }

        [Obsolete]
        public Site()
        {
            Connection = new Connection();
            _credentials = new SiteCredentials();
        }

        public Site([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            Connection = connection;
            _credentials = new SiteCredentials(connection.UserName, connection.Password);
        }

        public bool AutomaticallyUpdate => Connection.AutomaticallyUpdate;

        public bool CanExecute => (DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute;

        [NotNull]
        public Connection Connection { get; }

        public SiteCredentials Credentials
        {
            get
            {
                if (_credentials.UserName != Connection.UserName || _credentials.Password != Connection.Password)
                {
                    _credentials = new SiteCredentials(Connection.UserName, Connection.Password);
                }

                return _credentials;
            }
        }

        [NotNull]
        public DataService DataService => DataServiceManager.GetInstance(Connection);

        [NotNull]
        public string DataServiceName => Connection.DataServiceName;

        [NotNull]
        public static Site Empty { get; }

        public string HostName => Connection.HostName;

        public string Name
        {
            get
            {
                var result = Connection.HostName;
                /*
        if (!string.IsNullOrEmpty(this.Connection.UserName))
        {
          result += @" (" + this.Connection.UserName + @")";
        }
        */

                return result;
            }
        }

        [NotNull, Obsolete(@"Use HostName")]
        public string Server => Connection.HostName;

        public System.Version SitecoreVersion
        {
            get
            {
                if (_version != null)
                {
                    return _version;
                }

                var v = DataService.SitecoreVersionString;
                if (string.IsNullOrEmpty(v))
                {
                    return Sites.SitecoreVersion.EmptyVersion;
                }

                var s = string.Empty;

                var parts = v.Split('.');
                if (parts.Length < 2 || parts.Length > 4)
                {
                    s = "0.0.0.0.";
                }
                else
                {
                    foreach (var part in parts)
                    {
                        int p;
                        if (int.TryParse(part, out p))
                        {
                            s += p + ".";
                        }
                        else
                        {
                            s += "0.";
                        }
                    }
                }

                s = s.Left(s.Length - 1);

                _version = new System.Version(s);
                return _version;
            }
        }

        public string UserName => Connection.UserName;

        public string WebRootPath => Connection.WebRootPath;

        public void ChangeConnection([NotNull] string hostName, [NotNull] string userName, [NotNull] string password, [NotNull] string dataServiceName, [NotNull] string webRootPath, [NotNull] string description, bool automaticallyUpdate, bool useWindowsAuth)
        {
            Assert.ArgumentNotNull(hostName, nameof(hostName));
            Assert.ArgumentNotNull(userName, nameof(userName));
            Assert.ArgumentNotNull(password, nameof(password));
            Assert.ArgumentNotNull(dataServiceName, nameof(dataServiceName));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));
            Assert.ArgumentNotNull(description, nameof(description));

            Connection.HostName = hostName;
            Connection.UserName = userName;
            Connection.Password = password;
            Connection.UseWindowsAuth = useWindowsAuth;
            Connection.DataServiceName = dataServiceName;
            Connection.WebRootPath = webRootPath;
            Connection.Description = description;
            Connection.AutomaticallyUpdate = automaticallyUpdate;
        }

        public bool Equals([CanBeNull] Site other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.DataServiceName, DataServiceName) && Equals(other.Name, Name) && Equals(other.HostName, HostName) && Equals(other.Credentials, Credentials);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(Site))
            {
                return false;
            }

            return Equals((Site)obj);
        }

        public void Execute([Localizable(false)] string typeName, RequestCompleted completed, [Localizable(false)] params object[] parameters)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(completed, nameof(completed));
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                completed(response);
            };

            DataService.ExecuteAsync(typeName, c, parameters);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = DataServiceName.GetHashCode();
                result = (result * 397) ^ Name.GetHashCode();
                result = (result * 397) ^ WebRootPath.GetHashCode();
                result = (result * 397) ^ HostName.GetHashCode();
                result = (result * 397) ^ Credentials.GetHashCode();
                return result;
            }
        }

        public string GetHost()
        {
            if (string.IsNullOrEmpty(HostName))
            {
                return string.Empty;
            }

            var server = HostName;
            if (server.IndexOf(@"://", StringComparison.Ordinal) < 0)
            {
                server = @"http://" + server;
            }

            return server;
        }

        [CanBeNull]
        public BaseTreeViewItem GetTreeViewItem()
        {
            return DataService.GetTreeViewItem(this);
        }

        [Obsolete]
        public void Initialize([NotNull] string name, [Localizable(false), NotNull] string server, [NotNull, Localizable(false)] string dataServiceName, [NotNull] SiteCredentials credentials, [NotNull, Localizable(false)] string webRootPath)
        {
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(server, nameof(server));
            Assert.ArgumentNotNull(dataServiceName, nameof(dataServiceName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            ChangeConnection(server, credentials.UserName, credentials.Password, dataServiceName, webRootPath, name, false, false);
        }

        public bool IsSitecoreVersion(System.Version minVersion, bool trueIfEmptyVersion = true)
        {
            Assert.ArgumentNotNull(minVersion, nameof(minVersion));

            if (SitecoreVersion == Sites.SitecoreVersion.EmptyVersion)
            {
                return trueIfEmptyVersion;
            }

            return SitecoreVersion >= minVersion;
        }

        public bool IsSitecoreVersion(System.Version minVersion, System.Version maxVersion, bool trueIfEmptyVersion = true)
        {
            Assert.ArgumentNotNull(minVersion, nameof(minVersion));
            Assert.ArgumentNotNull(maxVersion, nameof(maxVersion));

            if (SitecoreVersion == Sites.SitecoreVersion.EmptyVersion)
            {
                return trueIfEmptyVersion;
            }

            return SitecoreVersion >= minVersion && SitecoreVersion <= maxVersion;
        }

        public static bool operator ==([CanBeNull] Site left, [CanBeNull] Site right)
        {
            return Equals(left, right);
        }

        public static bool operator !=([CanBeNull] Site left, [CanBeNull] Site right)
        {
            return !Equals(left, right);
        }

        [Obsolete]
        public void Rename([NotNull] string newName)
        {
            Assert.ArgumentNotNull(newName, nameof(newName));

            /* this.Name = newName; */
        }

        public void SetDataServiceName([Localizable(false), NotNull] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            Connection.DataServiceName = name;
        }

        public void SetWebRootPath([NotNull] string webRootPath)
        {
            Assert.ArgumentNotNull(webRootPath, nameof(webRootPath));

            Connection.WebRootPath = webRootPath;
        }
    }
}
