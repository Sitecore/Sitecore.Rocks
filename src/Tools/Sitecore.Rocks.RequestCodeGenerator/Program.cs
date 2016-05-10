// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sitecore.Rocks.RequestCodeGenerator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var generator = new RequestGenerator();

            var assemblyFileName = args[0];
            var targetFileName = args[1];

            var writeAccessor = true;

            for (var n = 2; n < args.Length; n++)
            {
                switch (args[n])
                {
                    case "/i":
                        writeAccessor = false;
                        break;
                }
            }

            generator.Write(assemblyFileName, targetFileName, writeAccessor);
        }

        public class Request
        {
            public Request(string name, string[] parameters)
            {
                Name = name;
                Parameters = parameters;
            }

            public string Name { get; }

            public string[] Parameters { get; }
        }

        public class RequestGenerator
        {
            public void Write(string assemblyFileName, string targetFileName, bool writeAccessor)
            {
                var requestNamespaces = new List<RequestNamespace>();

                GetRequests(requestNamespaces, assemblyFileName);

                requestNamespaces = requestNamespaces.OrderBy(r => r.Name).ToList();

                var output = new StringWriter();

                WriteRequests(output, requestNamespaces, writeAccessor);

                File.WriteAllText(targetFileName, output.ToString(), Encoding.UTF8);
            }

            private void GetRequests(List<RequestNamespace> requestNamespaces, string fileName)
            {
                var assembly = Assembly.LoadFrom(fileName);

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.FullName.StartsWith("Sitecore.Rocks.Server.Requests."))
                    {
                        continue;
                    }

                    var methodInfo = type.GetMethod("Execute");
                    if (methodInfo == null)
                    {
                        continue;
                    }

                    ProcessMethod(requestNamespaces, type, methodInfo);
                }
            }

            private void ProcessMethod(List<RequestNamespace> requestNamespaces, Type type, MethodInfo methodInfo)
            {
                var name = string.Empty;
                var nameSpace = type.FullName;

                var n = nameSpace.LastIndexOf('.');
                if (n >= 0)
                {
                    name = nameSpace.Substring(n + 1);
                    nameSpace = nameSpace.Substring(0, n);
                }

                if (nameSpace == "Sitecore.Rocks.Server.Requests")
                {
                    nameSpace = string.Empty;
                }
                else if (nameSpace.StartsWith("Sitecore.Rocks.Server.Requests."))
                {
                    nameSpace = nameSpace.Substring(31);
                }

                var requestNamespace = requestNamespaces.FirstOrDefault(r => r.Name == nameSpace);
                if (requestNamespace == null)
                {
                    requestNamespace = new RequestNamespace(nameSpace);
                    requestNamespaces.Add(requestNamespace);
                }

                var parameters = new List<string>();
                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    parameters.Add(parameterInfo.Name);
                }

                var request = new Request(name, parameters.ToArray());
                requestNamespace.Requests.Add(request);
            }

            private void WriteRequest(StringWriter output, RequestNamespace requestNamespace, Request request)
            {
                var databaseNameParameter = request.Parameters.FirstOrDefault(p => string.Compare(p, "database", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(p, "databaseName", StringComparison.InvariantCultureIgnoreCase) == 0);
                var itemIdParameter = request.Parameters.FirstOrDefault(p => string.Compare(p, "id", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(p, "itemId", StringComparison.InvariantCultureIgnoreCase) == 0);

                var hasDatabaseUri = !string.IsNullOrEmpty(databaseNameParameter);
                var hasItemUri = !string.IsNullOrEmpty(databaseNameParameter) && !string.IsNullOrEmpty(itemIdParameter);
                var replacing = hasDatabaseUri || hasItemUri;
                var first = true;

                var name = request.Name;
                if (!string.IsNullOrEmpty(requestNamespace.Name))
                {
                    name = requestNamespace.Name + "." + name;
                }

                output.WriteLine("      [System.CodeDom.Compiler.GeneratedCodeAttribute(\"SitecoreRocks\", \"1.0.0.0\")]");
                output.Write("      public void {0}(", request.Name);

                if (!replacing)
                {
                    output.Write("[NotNull] Site site");
                    first = false;
                }

                var replaced = false;
                foreach (var parameter in request.Parameters)
                {
                    if ((parameter != databaseNameParameter && parameter != itemIdParameter) || !replacing)
                    {
                        if (!first)
                        {
                            output.Write(", ");
                        }
                        else
                        {
                            first = false;
                        }

                        output.Write("[NotNull] string {0}", parameter);
                        continue;
                    }

                    if (!replaced)
                    {
                        if (!first)
                        {
                            output.Write(", ");
                        }
                        else
                        {
                            first = false;
                        }

                        if (hasItemUri)
                        {
                            output.Write("[NotNull] ItemUri itemUri");
                            replaced = true;
                        }
                        else
                        {
                            output.Write("[NotNull] DatabaseUri databaseUri");
                            replaced = true;
                        }
                    }
                }

                output.WriteLine(", [NotNull] ExecuteCompleted completed)");
                output.WriteLine("      {");

                output.Write("        ");

                if (replacing)
                {
                    if (hasItemUri)
                    {
                        output.Write("itemUri.Site");
                    }
                    else
                    {
                        output.Write("databaseUri.Site");
                    }
                }
                else
                {
                    output.Write("site");
                }

                output.Write(".DataService.ExecuteAsync(");
                output.Write("\"{0}\"", name);
                output.Write(", completed");

                foreach (var parameter in request.Parameters)
                {
                    if ((parameter != databaseNameParameter && parameter != itemIdParameter) || !replacing)
                    {
                        output.Write(", {0}", parameter);
                        continue;
                    }

                    if (parameter == itemIdParameter)
                    {
                        output.Write(", itemUri.ItemId.ToString()");
                        continue;
                    }

                    if (parameter == databaseNameParameter)
                    {
                        if (hasItemUri)
                        {
                            output.Write(", itemUri.DatabaseUri.DatabaseName.ToString()");
                        }
                        else
                        {
                            output.Write(", databaseUri.DatabaseName.ToString()");
                        }
                    }
                }

                output.WriteLine(");");

                output.WriteLine("      }");
                output.WriteLine();
            }

            private void WriteRequestNamespace(StringWriter output, RequestNamespace requestNamespace, bool writeAccessor)
            {
                if (!string.IsNullOrEmpty(requestNamespace.Name))
                {
                    var propertyName = requestNamespace.Name;
                    var n = propertyName.LastIndexOf('.');
                    if (n >= 0)
                    {
                        propertyName = propertyName.Substring(n + 1);
                    }

                    var className = propertyName + "Requests";
                    var variableName = className.ToLowerInvariant();

                    if (writeAccessor)
                    {
                        output.WriteLine("    private {0} {1};", className, variableName);
                        output.WriteLine();
                        output.WriteLine("    [NotNull]");
                        output.WriteLine("    public {2} {0} {{ get {{ return this.{1} ?? (this.{1} = new {2}()); }}}}", propertyName, variableName, className);
                        output.WriteLine();

                        output.WriteLine("    [System.CodeDom.Compiler.GeneratedCodeAttribute(\"SitecoreRocks\", \"1.0.0.0\")]");
                    }

                    output.WriteLine("    public partial class {0}", className);
                    output.WriteLine("    {");

                    foreach (var request in requestNamespace.Requests)
                    {
                        WriteRequest(output, requestNamespace, request);
                    }

                    output.WriteLine("    }");
                    output.WriteLine();
                }
                else
                {
                    foreach (var request in requestNamespace.Requests)
                    {
                        WriteRequest(output, requestNamespace, request);
                    }
                }
            }

            private void WriteRequests(StringWriter output, List<RequestNamespace> requestNamespaces, bool writeAccessor)
            {
                output.WriteLine("//------------------------------------------------------------------------------");
                output.WriteLine("// <auto-generated>");
                output.WriteLine("//     This code was generated by a tool.");
                output.WriteLine("//");
                output.WriteLine("//     Changes to this file may cause incorrect behavior and will be lost if");
                output.WriteLine("//     the code is regenerated.");
                output.WriteLine("// </auto-generated>");
                output.WriteLine("//------------------------------------------------------------------------------");
                output.WriteLine();
                output.WriteLine("#pragma warning disable 1591");
                output.WriteLine();

                output.WriteLine("namespace Sitecore.VisualStudio.Shell.Environment");
                output.WriteLine("{");
                output.WriteLine("  using Sitecore.VisualStudio.Annotations;");
                output.WriteLine("  using Sitecore.VisualStudio.Data;");
                output.WriteLine("  using Sitecore.VisualStudio.Sites;");
                output.WriteLine();

                output.WriteLine();

                output.WriteLine("  public partial class ServerHost ");
                output.WriteLine("  {");

                foreach (var requestNamespace in requestNamespaces.OrderBy(r => r.Name))
                {
                    WriteRequestNamespace(output, requestNamespace, writeAccessor);
                }

                output.WriteLine("  }");
                output.WriteLine();

                output.WriteLine("}");
                output.WriteLine();
                output.WriteLine("#pragma warning restore 1591");
            }
        }

        public class RequestNamespace
        {
            public RequestNamespace(string name)
            {
                Name = name;
                Requests = new List<Request>();
            }

            public string Name { get; }

            public List<Request> Requests { get; }
        }
    }
}
