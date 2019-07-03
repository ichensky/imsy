using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Config
{
    public class MemoryCacheKeys
    {
        public static string Logo { get => nameof(Logo); }
        public static string MostPopularQueries { get => nameof(MostPopularQueries); }
    }
}
