// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Extensions.StringExtensions;
using Sitecore.IO;
using Sitecore.Text;
using Sitecore.Web;

namespace Sitecore.Rocks.Server.Requests.UI.LogViewer
{
    public class GetLog : IComparer<FileInfo>
    {
        private string nextLine;

        public char[] Categories { get; set; }

        public string[] ExcludeFilter { get; set; }

        public string[] IncludeFilter { get; set; }

        public int LineNumber { get; set; }

        public int MaxItems { get; set; }

        public string Url { get; set; }

        public int Compare([CanBeNull] FileInfo x, [CanBeNull] FileInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return 1;
            }

            if (y == null)
            {
                return -1;
            }

            return -x.CreationTimeUtc.CompareTo(y.CreationTimeUtc);
        }

        [NotNull]
        public string Execute(int maxItems, [NotNull] string categories, [NotNull] string includeFilter, [NotNull] string excludeFilter)
        {
            Assert.ArgumentNotNull(categories, nameof(categories));
            Assert.ArgumentNotNull(includeFilter, nameof(includeFilter));
            Assert.ArgumentNotNull(excludeFilter, nameof(excludeFilter));

            MaxItems = maxItems;
            Url = WebUtil.GetServerUrl();

            if (!string.IsNullOrEmpty(categories))
            {
                Categories = categories.ToCharArray();
            }

            if (!string.IsNullOrEmpty(excludeFilter))
            {
                ExcludeFilter = excludeFilter.Split(',');
            }

            if (!string.IsNullOrEmpty(includeFilter))
            {
                IncludeFilter = includeFilter.Split(',');
            }

            var items = ReadItems();

            var writer = new StringWriter();

            var output = new XmlTextWriter(writer);

            RenderRss(output, items);

            output.Flush();

            return writer.ToString();
        }

        [NotNull]
        private static string Clean([CanBeNull] string s)
        {
            if (s == null || s.Length <= 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                sb.Append(char.IsControl(c) ? ' ' : c);
            }

            return sb.ToString();
        }

        private bool Eof([NotNull] StreamReader reader)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));

            return reader.EndOfStream && string.IsNullOrEmpty(nextLine);
        }

        private bool FilterItem([NotNull] GetLogItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            if (Categories != null)
            {
                var found = false;

                foreach (var text in Categories)
                {
                    if (item.Category.StartsWith(text.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            if (IncludeFilter != null)
            {
                var f = false;

                foreach (var text in IncludeFilter)
                {
                    if (item.Title.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        f = true;
                        break;
                    }
                }

                if (!f)
                {
                    return false;
                }
            }

            if (ExcludeFilter != null)
            {
                foreach (var text in ExcludeFilter)
                {
                    if (item.Title.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void GetCategory([NotNull] GetLogItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var title = item.Title;

            if (title.StartsWith("AUDIT"))
            {
                var n = title.IndexOf("):", StringComparison.Ordinal);
                if (n >= 0)
                {
                    item.UserName = title.Substring(7, n - 7);
                    item.Title = title.Substring(n + 2);
                }
                else
                {
                    item.Title = title.Substring(6);
                }

                item.Category = "Audit";

                return;
            }

            if (title.StartsWith("DEBUG:"))
            {
                item.Title = title.Substring(7);
                item.Category = "Debug";

                return;
            }

            switch (item.Category.ToUpperInvariant())
            {
                case "INFO":
                    item.Category = "Information";
                    return;
                case "WARN":
                    item.Category = "Warning";
                    return;
                case "ERROR":
                    item.Category = "Error";
                    return;
                case "FATAL":
                    item.Category = "Fatal Error";
                    return;
                case "DEBUG":
                    item.Category = "Debug";
                    return;
            }
        }

        [NotNull]
        private List<GetLogItem> ReadFile([NotNull] FileInfo file, int maxItems)
        {
            Debug.ArgumentNotNull(file, nameof(file));

            var result = new List<GetLogItem>();

            var fileTime = file.CreationTimeUtc;
            var date = new DateTime(fileTime.Year, fileTime.Month, fileTime.Day);

            LineNumber = 0;

            var reader = new StreamReader(new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.ASCII);

            while (!Eof(reader))
            {
                var item = ReadItem(reader, file, date);
                if (item == null)
                {
                    continue;
                }

                if (!FilterItem(item))
                {
                    continue;
                }

                result.Insert(0, item);
                while (result.Count > maxItems)
                {
                    result.RemoveAt(result.Count - 1);
                }
            }

            return result;
        }

        [CanBeNull]
        private GetLogItem ReadItem([NotNull] StreamReader reader, [NotNull] FileInfo file, DateTime date)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Debug.ArgumentNotNull(file, nameof(file));

            if (Eof(reader))
            {
                return null;
            }

            var line = ReadLine(reader);

            if (line == null)
            {
                return null;
            }

            if (line.StartsWith("ManagedPoolThread"))
            {
                return ReadManagedPoolThread(reader, file, date, line);
            }

            if (line.EndsWith("INFO  **************************************************"))
            {
                return ReadSitecoreShutDown(reader, file, date);
            }

            if (line.IndexOf("**********************************************************************", StringComparison.Ordinal) >= 0)
            {
                return ReadSitecoreStarted(reader, file, date, line);
            }

            var n = line.IndexOf(' ');
            if (n > 0)
            {
                var threadString = line.Left(n);

                if (MainUtil.GetInt(threadString, -1) >= 0)
                {
                    return ReadThreadItem(reader, file, date, line);
                }
            }

            return null;
        }

        [NotNull]
        private List<GetLogItem> ReadItems()
        {
            var result = new List<GetLogItem>();
            var maxItems = MaxItems;

            var directory = new DirectoryInfo(FileUtil.MapPath(Settings.LogFolder));
            var files = new List<FileInfo>(directory.GetFiles("log.*"));

            files.Sort(this);

            foreach (var file in files)
            {
                var items = ReadFile(file, maxItems);

                maxItems -= items.Count;

                result.AddRange(items);

                if (maxItems <= 0)
                {
                    break;
                }
            }

            return result;
        }

        [CanBeNull]
        private string ReadLine([NotNull] StreamReader reader)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));

            if (nextLine != null)
            {
                var result = nextLine;

                nextLine = null;

                return result;
            }

            if (reader.EndOfStream)
            {
                return null;
            }

            LineNumber++;

            return (reader.ReadLine() ?? string.Empty).Trim();
        }

        [NotNull]
        private GetLogItem ReadManagedPoolThread([NotNull] StreamReader reader, [NotNull] FileInfo file, DateTime date, [NotNull] string line)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(line, nameof(line));

            var result = new GetLogItem
            {
                LineNumber = LineNumber,
                Guid = file.Name + "#" + LineNumber,
                File = file
            };

            var pos = 0;
            string pool;
            string poolNumber;
            string time;
            string category;

            TextParser.ReadToken(line, ' ', ref pos, out pool);
            TextParser.ReadToken(line, ' ', ref pos, out poolNumber);
            TextParser.ReadToken(line, ' ', ref pos, out time);
            TextParser.ReadToken(line, ' ', ref pos, out category);

            var title = line.Mid(pos).Trim();

            result.Title = title;
            result.Link = Url;
            result.Description = title;
            result.PubDate = date + DateUtil.ParseTimeSpan(time);
            result.Category = category;

            GetCategory(result);

            if (result.Category == "Error")
            {
                result.Description = title + ReadMore(reader);
            }

            return result;
        }

        [NotNull]
        private string ReadMore([NotNull] StreamReader reader)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));

            var line = ReadLine(reader);
            if (line == null)
            {
                return string.Empty;
            }

            if (line.StartsWith("Exception"))
            {
                return ReadUntilBlankLine(reader, line);
            }

            nextLine = line;

            return string.Empty;
        }

        [NotNull]
        private GetLogItem ReadSitecoreShutDown([NotNull] StreamReader reader, [NotNull] FileInfo file, DateTime date)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Debug.ArgumentNotNull(file, nameof(file));

            var result = new GetLogItem
            {
                LineNumber = LineNumber,
                Guid = file.Name + "#" + LineNumber,
                File = file
            };

            var line = ReadLine(reader);

            var pos = 0;
            string thread;
            string time;
            string category;

            TextParser.ReadToken(line, ' ', ref pos, out thread);
            TextParser.ReadToken(line, ' ', ref pos, out time);
            TextParser.ReadToken(line, ' ', ref pos, out category);

            var title = line.Mid(pos).Trim();

            result.Title = title;
            result.Link = Url;
            result.Description = title;
            result.PubDate = date + DateUtil.ParseTimeSpan(time);
            result.Category = category;

            GetCategory(result);

            line = ReadLine(reader);
            if (line != null)
            {
                result.Description += "\n\n" + ReadUntilBlankLine(reader, line);
            }

            return result;
        }

        [NotNull]
        private GetLogItem ReadSitecoreStarted([NotNull] StreamReader reader, [NotNull] FileInfo file, DateTime date, [NotNull] string line)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(line, nameof(line));

            var result = new GetLogItem
            {
                LineNumber = LineNumber,
                Guid = file.Name + "#" + LineNumber,
                File = file
            };

            var pos = 0;
            string threadNumber;
            string time;
            string category;

            var text = new StringBuilder();

            TextParser.ReadToken(line, ' ', ref pos, out threadNumber);
            TextParser.ReadToken(line, ' ', ref pos, out time);
            TextParser.ReadToken(line, ' ', ref pos, out category);

            ReadLine(reader);

            var l = ReadLine(reader);
            while (l != null && l.IndexOf("**********************************************************************", StringComparison.Ordinal) < 0)
            {
                pos = 0;
                TextParser.ReadToken(l, ' ', ref pos, out threadNumber);
                TextParser.ReadToken(l, ' ', ref pos, out time);
                TextParser.ReadToken(l, ' ', ref pos, out category);

                text.Append(l.Mid(pos));
                text.Append("\n");

                l = ReadLine(reader);
            }

            if (l != null)
            {
                ReadLine(reader);
            }

            result.Title = "Sitecore Started";
            result.Link = Url;
            result.Description = text.ToString();
            result.PubDate = date + DateUtil.ParseTimeSpan(time);
            result.Category = category;

            GetCategory(result);

            return result;
        }

        [CanBeNull]
        private GetLogItem ReadThreadItem([NotNull] StreamReader reader, [NotNull] FileInfo file, DateTime date, [NotNull] string line)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Debug.ArgumentNotNull(file, nameof(file));
            Debug.ArgumentNotNull(line, nameof(line));

            var result = new GetLogItem
            {
                LineNumber = LineNumber,
                Guid = file.Name + "#" + LineNumber,
                File = file
            };

            var pos = 0;
            int threadNumber;
            string time;
            string category;

            TextParser.ReadInt(line, ' ', ref pos, out threadNumber);
            TextParser.ReadToken(line, ' ', ref pos, out time);
            TextParser.ReadToken(line, ' ', ref pos, out category);

            var title = line.Mid(pos).Trim();
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }

            var description = ReadMore(reader);

            result.Title = title;
            result.Link = Url;
            result.Description = title + description;
            result.PubDate = date + DateUtil.ParseTimeSpan(time);
            result.Category = category;

            GetCategory(result);

            return result;
        }

        [NotNull]
        private string ReadUntilBlankLine([NotNull] StreamReader reader, [NotNull] string line)
        {
            Debug.ArgumentNotNull(reader, nameof(reader));
            Debug.ArgumentNotNull(line, nameof(line));

            var builder = new StringBuilder(line);

            var l = ReadLine(reader);
            while (!string.IsNullOrEmpty(l))
            {
                builder.Append("\n");
                builder.Append(l);

                l = ReadLine(reader);
            }

            return builder.ToString();
        }

        private void RenderItem([NotNull] XmlTextWriter output, [NotNull] GetLogItem item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var description = string.Format("<div>{1}</div><div style=\"font-size:11px;margin-top:8px\"><i>{0}</i><span style=\"color:#999999\">, {2}, line {3}</span></div>", item.Category, item.Description, item.File.Name, item.LineNumber);

            description = description.Replace("\n", "<br />");

            output.WriteStartElement("item");

            output.WriteElementString("title", item.Title);
            output.WriteElementString("link", item.Link);
            output.WriteElementString("description", Clean(description));
            output.WriteElementString("pubDate", DateUtil.ToIsoDate(item.PubDate));
            output.WriteElementString("guid", item.Guid);
            output.WriteElementString("category", item.Category);

            output.WriteEndElement();
        }

        private void RenderItems([NotNull] XmlTextWriter output, [NotNull] List<GetLogItem> items)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var rssItem in items)
            {
                RenderItem(output, rssItem);
            }
        }

        private void RenderRss([NotNull] XmlTextWriter output, [NotNull] List<GetLogItem> items)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(items, nameof(items));

            output.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
            output.WriteStartElement("rss");
            output.WriteAttributeString("version", "2.0");

            output.WriteStartElement("channel");

            output.WriteElementString("title", "Sitecore Log RSS Feed");
            output.WriteElementString("link", Url);
            output.WriteElementString("description", "Shows log information from Sitecore");
            output.WriteElementString("generator", "Sitecore");
            output.WriteElementString("language", "en-us");

            RenderItems(output, items);

            output.WriteEndElement();
            output.WriteEndElement();
        }
    }
}
