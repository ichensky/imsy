using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelCrawler.Helpers
{
    public static class HashHelper
    {
        public static string GetStringSha1Hash(this byte[] arr)
        {
            using (var sha = new System.Security.Cryptography.SHA1Managed())
            {
                var hash = sha.ComputeHash(arr);
                return string.Concat(hash.Select(x => x.ToString("X2")));
            }
        }
        public static string GetStringMd5Hash(this byte[] arr)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(arr);
                return string.Concat(hash.Select(x => x.ToString("X2")));
            }
        }
    }
}
