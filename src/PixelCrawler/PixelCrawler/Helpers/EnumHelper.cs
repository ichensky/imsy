using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelCrawler.Helpers
{
    public static class EnumHelper
    {
        public static List<T> EnumToList<T>(this T t) 
            => ((T[])Enum.GetValues(typeof(T))).ToList();
    }
}
