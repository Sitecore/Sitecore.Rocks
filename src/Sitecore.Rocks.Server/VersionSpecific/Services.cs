using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Abstractions.Jobs;
using Sitecore.Rocks.Server.Abstractions.Serialization;
using Sitecore.Rocks.Server.Abstractions.Validators;


namespace Sitecore.Rocks.Server.VersionSpecific
{
    public static class Services
    {
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;
        private static IJobManager _jobManager = null;
        private static ISerializationService _serializationService = null;
        private static ILegacyIndexingValidator _legacyIndexingValidator = null;

        public static IJobManager JobManager
        { 
            get
            {
                Assert.AreEqual(_initialized, true, "Services have not been initialized");
                Assert.IsNotNull(_jobManager, $"{nameof(JobManager)} has not been initialized");
                return _jobManager;
            }
        }

        public static ISerializationService SerializationService
        {
            get
            {
                Assert.AreEqual(_initialized, true, "Services have not been initialized");
                Assert.IsNotNull(_serializationService, $"{nameof(SerializationService)} has not been initialized");
                return _serializationService;
            }
        }

        public static ILegacyIndexingValidator LegacyIndexingValidator
        {
            get
            {
                Assert.AreEqual(_initialized, true, "Services have not been initialized");
                Assert.IsNotNull(_legacyIndexingValidator, $"{nameof(LegacyIndexingValidator)} has not been initialized");
                return _legacyIndexingValidator;
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

            TService GetType<TService>() where TService : class
            {
                var serviceType = types.FirstOrDefault(type => typeof(TService).IsAssignableFrom(type));
                if (serviceType == null)
                {
                    throw new Exception($"Unable to find service {nameof(TService)} in {assembly.FullName}");
                }
                var service = Activator.CreateInstance(serviceType) as TService;
                if (service == null)
                {
                    throw new Exception($"Unable to initialize service {serviceType.FullName}");
                }

                return service;
            }

            _jobManager = GetType<IJobManager>();
            _serializationService = GetType<ISerializationService>();
            _legacyIndexingValidator = GetType<ILegacyIndexingValidator>();
        }

    }
}