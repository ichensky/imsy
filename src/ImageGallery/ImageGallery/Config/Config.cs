using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Config
{
    public class Config
    {
        public string AppName { get; set; }
        public string Domain { get; set; }
        public string JsonDbPath { get; set; }
        public string DbPath { get; set; }
        public string LogPath { get; set; }
    }
}
