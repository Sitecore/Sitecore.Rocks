using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Abstractions.Jobs;


namespace Sitecore.Rocks.Server.VersionSpecific
{
    public static class Services
    {
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;
        private static IJobManager _jobManager = null;

        public static IJobManager JobManager
        { 
            get
            {
                Assert.AreEqual(_initialized, true, "Services have not been initialized");
                Assert.IsNotNull(_jobManager, $"{nameof(JobManager)} has not been initialized");
                return _jobManager;
            }
        }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            lock (_lockObject)
            {
                // double check
                if (_initialized)
                {
                    return;
                }
                var assembly = GetVersionSpecificAssembly();
                LoadServices(assembly);
                _initialized = true;
            }
        }

        private static Assembly GetVersionSpecificAssembly()
        {
            var assemblyMatcher = new AssemblyMatcher();
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var currentVersion = Sitecore.Configuration.About.GetVersionNumber(false);
            var resource = assemblyMatcher.MatchResource(resources, currentVersion);
            if (resource == null)
            {
                throw new Exception($"Unable to find version specific services for Sitecore {currentVersion}.");
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource.ResourceName))
            {
                var assemblyData = new byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

        private static void LoadServices(Assembly assembly)
        {
            var types = assembly.GetTypes();

            // This process could be further abstracted when we have more version-specific services
            var jobRunnerType = types.FirstOrDefault(type => typeof(IJobManager).IsAssignableFrom(type));
            if (jobRunnerType == null)
            {
                throw new Exception($"Unable to find service {nameof(IJobManager)} in {assembly.FullName}");
            }
            _jobManager = Activator.CreateInstance(jobRunnerType) as IJobManager;
            if (_jobManager == null)
            {
                throw new Exception($"Unable to initialize service {jobRunnerType.FullName}");
            }
        }
    }
}