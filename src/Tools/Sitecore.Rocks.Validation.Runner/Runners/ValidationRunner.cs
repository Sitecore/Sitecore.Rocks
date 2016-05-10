// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Sitecore.Extensions.StringExtensions;
using Sitecore.Extensions.XElementExtensions;

namespace Sitecore.Runners
{
    /*                                        
  <Target Name="AfterBuild">
    <Exec Command="$(MSBuildProjectDirectory)\sitecorerocks\Sitecore.Rocks.Validation.Runner.exe" WorkingDirectory="$(MSBuildProjectDirectory)\sitecorerocks\" />
  </Target>
  */

    public class ValidationRunner
    {
        public XElement Config { get; private set; }

        public string ConfigFileName { get; set; }

        private string[] CommandLineArgs
        {
            get
            {
                return CommandLineToArgs(CommandLineArguments);
            }
        }

        private string CommandLineArguments
        {
            get
            {
                return Environment.CommandLine;
            }
        }

        private Connection Connection { get; set; }

        public void Run()
        {
            ConfigFileName = "Sitecore.Rocks.Validation.Runner.config.xml";

            LoadConfig();
            Validate();
        }

        private string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(@"foo.exe " + commandLine, out argc);
            if (argv == IntPtr.Zero)
            {
                return new string[0];
            }

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        [DllImport(@"shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string commandLine, out int argsCount);

        private void FormatResponse(string response)
        {
            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements())
            {
                var severity = element.GetAttributeValue("severity");
                var title = element.GetElementValue("title");
                var problem = element.GetElementValue("problem");
                var solution = element.GetElementValue("solution");
                var itemPath = element.GetAttributeValue("itempath");

                var error = severity == "error" ? "error" : "warning";

                Console.WriteLine("Sitecore Rocks Validation : Validation {1} : {0}: {2}. {3} {4}", itemPath, error, title, problem, solution);
            }
        }

        private void LoadConfig()
        {
            ParseCommandLine();
            ReadConfigFile();
            LoadConnection();
        }

        private void LoadConnection()
        {
            var element = Config.Element("binding");
            if (element == null)
            {
                throw new InvalidOperationException("Config section 'Connection' is missing");
            }

            Connection = new Connection();
            Connection.Load(element);
        }

        private void ParseCommandLine()
        {
            var commandLine = CommandLineArgs;

            for (var n = 2; n < commandLine.Length; n++)
            {
                ConfigFileName = commandLine[n];
            }
        }

        private void ParseSettings(Settings settings, XElement element)
        {
            settings.ContextName = element.GetAttributeValue("Name", "Site") ?? "Site";
            settings.RootItemPath = element.GetAttributeValue("ItemPathOrId", "/") ?? "/";
            settings.ProcessSiteValidations = string.Compare(element.GetAttributeValue("ProcessSiteValidations"), "True", StringComparison.InvariantCultureIgnoreCase) == 0;

            var databasesAndLanguages = element.GetAttributeValue("DatabaseName", "master") ?? "master";

            var languages = element.GetAttributeValue("Languages", "en") ?? "en";
            foreach (var languageName in languages.Split(','))
            {
                if (string.IsNullOrWhiteSpace(languageName))
                {
                    continue;
                }

                databasesAndLanguages += '^';
                databasesAndLanguages += languageName.Trim();
            }

            settings.DatabaseAndLanguages = databasesAndLanguages;

            var inactiveValidation = string.Empty;
            foreach (var ignoreElement in element.Elements("Ignore"))
            {
                inactiveValidation += "[" + ignoreElement.Value + "]";
            }

            settings.InactiveValidations = inactiveValidation;
        }

        private void ReadConfigFile()
        {
            if (!File.Exists(ConfigFileName))
            {
                throw new FileNotFoundException("File not found: ", ConfigFileName);
            }

            var doc = XDocument.Load(ConfigFileName);
            var root = doc.Root;
            if (root == null)
            {
                throw new InvalidOperationException("Config file is not valid: " + ConfigFileName);
            }

            Config = root;
        }

        private void Validate()
        {
            foreach (var section in Config.Elements("Validate"))
            {
                var response = Validate(section);

                FormatResponse(response);
            }
        }

        private string Validate(XElement contextElement)
        {
            var settings = new Settings();
            ParseSettings(settings, contextElement);

            var url = Connection.HostName + "/sitecore/shell/WebService/Sitecore.Rocks.Validation.ashx";
            if (url.IndexOf("://", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                url = "http://" + url;
            }

            using (var webClient = new WebClient())
            {
                var formData = new NameValueCollection();
                formData["u"] = Connection.UserName;
                formData["p"] = Connection.Password;
                formData["t"] = Connection.ReceiveTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                formData["c"] = settings.ContextName;
                formData["d"] = settings.DatabaseAndLanguages;
                formData["v"] = settings.InactiveValidations;
                formData["i"] = settings.RootItemPath;
                formData["s"] = settings.ProcessSiteValidations ? "1" : "0";

                var responseBytes = webClient.UploadValues(url, "POST", formData);
                return Encoding.UTF8.GetString(responseBytes);
            }
        }
    }
}
