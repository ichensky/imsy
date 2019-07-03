using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Error
{
    public class ErrorResponce
    {
        [JsonProperty("errorcode")]
        public ErrorCode ErrorCode { get; set; }
    }
}
