using System;
using System.Collections.Generic;
using System.Text;

namespace PixelCrawler.Helpers
{
    public static class ParserHelper
    {
        public static string SubStrBeginEnd(string str, string beginWord, string endWord, ref int startIndex)
        {
            var begin = str.IndexOf(beginWord, startIndex);
            if (begin >= 0)
            {
                var subStrBegin = begin + beginWord.Length;
                var end = str.IndexOf(endWord, subStrBegin);
                if (end >= 0)
                {
                    var subStr = str.Substring(subStrBegin, end - subStrBegin);
                    startIndex = end + endWord.Length;
                    return subStr;
                }
            }
            startIndex = str.Length;
            return null;
        }

    }
}
