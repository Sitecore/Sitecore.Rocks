// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Web.Security;
using System.Web.Services;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility;
using Sitecore.Rocks.Server.Pipelines.Login;
using Sitecore.Rocks.Server.Requests;
using Sitecore.Rocks.Server.Requests.Items;
using Sitecore.Rocks.Server.Requests.Templates;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
using Sitecore.SecurityModel;
using Sitecore.Xml;

namespace Sitecore.Visual
{
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1), ToolboxItem(false), WebService(Name = "Sitecore Web Service 2", Namespace = "http://sitecore.net/visual/")]
    public class Service2 : Service
    {
        /// <summary>
        /// The username of the authenticated / validated Windows Auth user.
        /// </summary>
        protected string WindowsAuthUser => Context.Request.ServerVariables["LOGON_USER"];

        public Service2()
        {
			try
			{
                Sitecore.Rocks.Server.VersionSpecific.Services.Initialize();
                ExtensibilityLoader.Initialize();
			}
            catch (Exception e)
			{
				Log.Error("Error initializing Sitecore Rocks server components", e, this);
				var reflectionException = e as System.Reflection.ReflectionTypeLoadException;
				if (reflectionException != null)
				{
					foreach (var loaderException in reflectionException.LoaderExceptions)
					{
						Log.Error("Loader exception", loaderException, this);
					}
				}
				throw new Exception("Error initializing Sitecore Rocks server components, see server logs for details.");
			}
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument CopyTo([NotNull] string id, [NotNull] string newParent, [NotNull] string name, [NotNull] string databaseName, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(newParent, nameof(newParent));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return GetXmlDocument(new CopyTo().Execute(id, newParent, name, databaseName));
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument Duplicate([NotNull] string id, [NotNull] string name, [NotNull] string databaseName, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(name, nameof(name));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return GetXmlDocument(new Duplicate().Execute(id, name, databaseName));
        }

        [NotNull, WebMethod(EnableSession = true)]
        public string Execute([NotNull] string typeName, [NotNull] object[] parameters, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(parameters, nameof(parameters));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return new RequestHandler(typeName, parameters).Execute();
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument GetChildren([NotNull] string id, [NotNull] string databaseName, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return GetXmlDocument(new GetChildren().Execute(id, databaseName));
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument GetDatabases([NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            var packet = new Packet();
            try
            {
                foreach (var databaseName in Factory.GetDatabaseNames())
                {
                    var connectionString = string.Empty;

                    try
                    {
                        var settings = ConfigurationManager.ConnectionStrings[databaseName];
                        if (settings != null)
                        {
                            connectionString = settings.ConnectionString ?? string.Empty;
                        }
                    }
                    catch
                    {
                    }

                    packet.AddElement("database", databaseName, "connectionstring", connectionString);
                }
            }
            catch
            {
            }

            return packet.XmlDocument;
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument GetItemFields([NotNull] string id, [NotNull] string language, [NotNull] string version, bool allFields, [NotNull] string databaseName, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(language, nameof(language));
            Assert.ArgumentNotNull(version, nameof(version));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return GetXmlDocument(new GetItemFields().Execute(id, language, version, allFields, databaseName));
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument GetTemplates([NotNull] string databaseName, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return GetXmlDocument(new GetTemplates().Execute(databaseName, "false"));
        }

        [NotNull, WebMethod(EnableSession = true)]
        public string Login([NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            var login = DoLogin(credentials, out string message);
            if (!login)
            {

                output.WriteStartElement("error");
                output.WriteString(message);
                output.WriteEndElement();
            }
            else
            {
                output.WriteStartElement("login");
                LoginPipeline.Run().WithParameters(output);
                output.WriteEndElement();
            }

            return writer.ToString();
        }

        [NotNull, WebMethod(EnableSession = true)]
        public new XmlDocument Save([NotNull] string xml, [NotNull] string databaseName, [NotNull] Credentials credentials)
        {
            Assert.ArgumentNotNull(xml, nameof(xml));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            LoginUser(credentials);

            return GetXmlDocument(new Save().Execute(xml, databaseName));
        }

        protected void LoginUser([NotNull] Credentials credentials)
        {
            string message = null;
            if (!DoLogin(credentials, out message))
            {
                throw new Exception(message);
            }
        }

        protected bool DoLogin([NotNull] Credentials credentials, out string message)
        {
            Assert.ArgumentNotNull(credentials, nameof(credentials));

            bool login;
            message = null;

            if (!SecurityModel.License.LicenseManager.HasContentManager && !SecurityModel.License.LicenseManager.HasExpress)
            {
                message = "A required license is missing";
                return false;
            }

            // Log out if different user
            if (Sitecore.Context.IsLoggedIn && !Sitecore.Context.User.Name.Equals(credentials.UserName, StringComparison.OrdinalIgnoreCase))
            {
                AuthenticationManager.Logout();
            }

            if (credentials.CustomData == "windowsauth" && credentials.UserName == WindowsAuthUser)
            {
                // Note: AD Domain must match Sitecore domain name
                login = AuthenticationManager.Login(credentials.UserName);
            }
            else
            {
                login = AuthenticationManager.Login(credentials.UserName, credentials.Password);
            }

            if (!login)
            {
                message = "Invalid user or password.";
                return false;
            }

            var user = Sitecore.Security.Accounts.User.Current;
            const string requiredRole = "sitecore\\Sitecore Client Developing";
            if (!user.IsInRole(requiredRole) && !user.IsAdministrator)
            {
                message = $"User {user.Name} must be an admin or a member of {requiredRole} to use Sitecore Rocks.";
                Log.Warn(message, this);
                AuthenticationManager.Logout();
                return false;
            }

            return true;
        }

        [NotNull]
        private XmlDocument GetXmlDocument([NotNull] string xml)
        {
            Assert.ArgumentNotNull(xml, nameof(xml));

            var result = new XmlDocument();

            if (!string.IsNullOrEmpty(xml))
            {
                result.LoadXml(xml);
            }

            return result;
        }
    }
}
