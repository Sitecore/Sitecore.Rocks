// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects.ToolboxItems
{
    public abstract class HtmlToolboxItemHandler : ToolboxItemHandler
    {
        [NotNull]
        public static string ConvertToClipboardFormat([NotNull] string htmlFragment, [NotNull] string title, [CanBeNull] Uri sourceUrl)
        {
            Assert.ArgumentNotNull(htmlFragment, nameof(htmlFragment));
            Assert.ArgumentNotNull(title, nameof(title));

            var sb = new StringBuilder();

            const string Header = @"Format:HTML Format
Version:1.0
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
StartSelection:<<<<<<<3
EndSelection:<<<<<<<4
";

            var pre = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<HTML><HEAD><TITLE>" + title + @"</TITLE></HEAD><BODY><!--StartFragment-->";

            const string Post = @"<!--EndFragment--></BODY></HTML>";

            sb.Append(Header);
            if (sourceUrl != null)
            {
                sb.AppendFormat(@"SourceURL:{0}", sourceUrl);
            }

            var startHtml = sb.Length;

            sb.Append(pre);
            var fragmentStart = sb.Length;

            sb.Append(htmlFragment);
            var fragmentEnd = sb.Length;

            sb.Append(Post);
            var endHtml = sb.Length;

            sb.Replace(@"<<<<<<<1", To8DigitString(startHtml));
            sb.Replace(@"<<<<<<<2", To8DigitString(endHtml));
            sb.Replace(@"<<<<<<<3", To8DigitString(fragmentStart));
            sb.Replace(@"<<<<<<<4", To8DigitString(fragmentEnd));

            return sb.ToString();
        }

        protected void AddHtml([NotNull] string name, [NotNull, Localizable(false)] string html)
        {
            Debug.ArgumentNotNull(name, nameof(name));
            Debug.ArgumentNotNull(html, nameof(html));

            // ReSharper disable once SuspiciousTypeConversion.Global
            var toolbox = SitecorePackage.Instance.GetService<SVsToolbox>() as IVsToolbox;
            if (toolbox == null)
            {
                return;
            }

            var data = new OleDataObject();

            data.SetText(ConvertToClipboardFormat(html, name, null), TextDataFormat.Html);

            var itemInfo = GetItemInfo(name);

            try
            {
                toolbox.AddItem(data, itemInfo, Resources.HtmlToolboxItemHandler_AddHtml_Sitecore);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        [NotNull]
        private TBXITEMINFO[] GetItemInfo([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            var result = new TBXITEMINFO[1];

            result[0].bstrText = name;
            result[0].dwFlags = (uint)__TBXITEMINFOFLAGS.TBXIF_DONTPERSIST;

            return result;
        }

        [NotNull]
        private static string To8DigitString(int x)
        {
            return string.Format(@"{0,8}", x);
        }
    }
}
