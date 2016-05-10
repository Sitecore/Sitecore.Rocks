// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests
{
    public class RequestHandler
    {
        public RequestHandler([NotNull] string typeName, [NotNull] object[] parameters)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            TypeName = typeName;
            Parameters = parameters;
        }

        [NotNull]
        public object[] Parameters { get; }

        [NotNull]
        public string TypeName { get; }

        [NotNull]
        public string Execute()
        {
            var output = new StringWriter();

            try
            {
                var typeName = TypeName;
                var assemblyName = string.Empty;

                var n = typeName.IndexOf(",", StringComparison.Ordinal);
                if (n >= 0)
                {
                    assemblyName = typeName.Mid(n + 1).Trim();
                    typeName = typeName.Left(n);

                    if (!assemblyName.StartsWith("/bin/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        assemblyName = "/bin/" + assemblyName;
                    }

                    if (!assemblyName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    {
                        assemblyName += ".dll";
                    }
                }

                if (!typeName.StartsWith("Sitecore.Rocks.Server.Requests"))
                {
                    throw new Exception("Can only execute request handlers in the Sitecore.Rocks.Server.Requests namespace.");
                }

                Type type;

                if (!string.IsNullOrEmpty(assemblyName))
                {
                    var assembly = Assembly.LoadFile(FileUtil.MapPath(assemblyName));
                    if (assembly == null)
                    {
                        throw new Exception(string.Format("Cannot find assembly '{0}'.", assemblyName));
                    }

                    type = assembly.GetType(typeName);
                }
                else
                {
                    type = Type.GetType(typeName) ?? GetTypeFromAssemblies(typeName);
                }

                if (type == null)
                {
                    throw new Exception(string.Format("Cannot find type '{0}'.", typeName));
                }

                var instance = Activator.CreateInstance(type);

                var methodInfo = type.GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
                if (methodInfo == null)
                {
                    throw new Exception(string.Format("Cannot find method 'Execute' in type '{0}'.", type.FullName));
                }

                var result = methodInfo.Invoke(instance, Parameters) as string ?? string.Empty;

                output.WriteLine(result);
            }
            catch (Exception ex)
            {
                output.WriteLine("***ERROR***");
                output.WriteLine(ex.Message);

                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    output.WriteLine();
                    output.WriteLine(innerException.Message);

                    innerException = innerException.InnerException;
                }
            }

            return output.ToString();
        }

        [CanBeNull]
        private Type GetTypeFromAssemblies([NotNull] string typeName)
        {
            var folder = FileUtil.MapPath("/bin");

            foreach (var fileName in Directory.GetFiles(folder, "Sitecore.Rocks.Server.*.dll"))
            {
                var fileInfo = new FileInfo(fileName);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                {
                    continue;
                }

                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(fileName);
                }
                catch
                {
                    continue;
                }

                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
