// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Globalization;
using System.IO;
using Sitecore.Caching;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.WriteItemHeader
{
    [Pipeline(typeof(WriteItemHeaderPipeline), 5000)]
    public class SerializationStatus : PipelineProcessor<WriteItemHeaderPipeline>
    {
        private static readonly Cache Cache;

        static SerializationStatus()
        {
            Cache = new Cache("SerializationStatus", 5000);
        }

        protected override void Process(WriteItemHeaderPipeline pipeline)
        {
            var status = 0;
            try
            {
                var reference = new ItemReference(pipeline.Item);
                var path = PathUtils.GetFilePath(reference.ToString());

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

            var cacheEntry = Cache.GetEntry(fileName, true);
            if (cacheEntry != null)
            {
                var cacheRecord = cacheEntry.Data as CacheRecord;
                if (cacheRecord != null)
                {
                    if (cacheRecord.LastWrite == lastWrite)
                    {
                        revision = cacheRecord.Revision;
                    }
                }
            }

            if (revision == null)
            {
                revision = GetRevision(fileName);
                if (string.IsNullOrEmpty(revision))
                {
                    return 3;
                }

                Cache.Add(fileName, new CacheRecord(lastWrite, revision), 1);
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
