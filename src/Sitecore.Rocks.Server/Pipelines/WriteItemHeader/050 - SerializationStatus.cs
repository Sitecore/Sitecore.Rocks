// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Caching;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 5000)]
    public class SerializationStatus : PipelineProcessor<WriteItemHeaderPipeline>
    {
        private static readonly MemoryCache Cache = new MemoryCache("serialization");

        protected override void Process(WriteItemHeaderPipeline pipeline)
        {
            var status = 0;
            try
            {
                var pathResolver = VersionSpecific.Services.SerializationService;
                var path = pathResolver.GetPath(pipeline.Item.Uri.ToString());

                if (FileUtil.FileExists(path))
                {
                    status = GetStatus(pipeline.Item, path);
                }
            }
            catch
            {
                status = 3;
            }

            pipeline.Output.WriteAttributeString("serializationstatus", status.ToString(CultureInfo.InvariantCulture));
        }

        [CanBeNull]
        private string GetRevision([NotNull] string fileName)                                                                                                                                                    
        {
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            var mode = 0;
            var count = 0;

            try
            {
                using (var stream = new StreamReader(fileName))
                {
                    while (!stream.EndOfStream)
                    {
                        var line = stream.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        switch (mode)
                        {
                            case 0:
                                if (line.StartsWith("field: {8CDC337E-A112-42FB-BBB4-4143751E123F}", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    mode = 1;
                                }

                                break;
                            case 1:
                                count++;
                                if (count == 4)
                                {
                                    return line;
                                }

                                break;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private int GetStatus([NotNull] Item item, [NotNull] string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists)
            {
                return 0;
            }

            string revision = null;
            var lastWrite = fileInfo.LastWriteTimeUtc;

            var cacheRecord = Cache.Get(fileName) as CacheRecord;
            if (cacheRecord != null)
            {
                if (cacheRecord.LastWrite == lastWrite)
                {
                    revision = cacheRecord.Revision;
                }
            }

            if (revision == null)
            {
                revision = GetRevision(fileName);
                if (string.IsNullOrEmpty(revision))
                {
                    return 3;
                }

                var expires = DateTime.Now.AddHours(4);
                Cache.Set(fileName, new CacheRecord(lastWrite, revision), expires);
            }

            return revision != item.Statistics.Revision ? 2 : 1;
        }

        public class CacheRecord
        {
            public CacheRecord(DateTime lastWrite, [NotNull] string revision)
            {
                Assert.ArgumentNotNull(revision, nameof(revision));

                LastWrite = lastWrite;
                Revision = revision;
            }

            public DateTime LastWrite { get; }

            [NotNull]
            public string Revision { get; }
        }
    }
}
