using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Library.Database.Helpers
{
    public static class HtmlHelper
    {
        public static MarkupString HtmlEncodeMultiline(string textWithLineBreaks)
        {
            var encodedLines = (textWithLineBreaks ?? string.Empty)
                .Split(new char[] { '\r', '\n' })
                .Select(line => HttpUtility.HtmlEncode(line))
                .ToArray();

            return (MarkupString)string.Join("<br />", encodedLines);
        }

        public static MarkupString HtmlDecodeMultiline(string textWithLineBreaks)
        {
            var encodedLines = (textWithLineBreaks ?? string.Empty)
                .Split("<br />")
                .Select(line => HttpUtility.HtmlEncode(line))
                .ToArray();

            return (MarkupString)string.Join("\r\n", encodedLines);
        }
    }
}
