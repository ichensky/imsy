using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelCrawler.Helpers;
using NLog;

namespace PixelCrawler.Services
{
    public class MSVisionService 
    {
        private readonly object _lock=new object();
        private readonly List<VisualFeatureTypes> _features =
                        ((VisualFeatureTypes[]) Enum.GetValues(typeof(VisualFeatureTypes))).ToList();
        private readonly List<Details> _details =
                        ((Details[]) Enum.GetValues(typeof(Details))).ToList();

        private int _curKey =-1;
        private NLog.Logger _logger;
        private string _keysPath;
        public Dictionary<string, ComputerVisionClient> ComputerVisionClients;
        public MSVisionService(NLog.Logger logger, string keysPath) {
            this._logger = logger;
            this._keysPath = keysPath;
            
        }
        private KeyValuePair<string, ComputerVisionClient> NextClient() {
            lock (_lock)
            {
                if (ComputerVisionClients == null)
                {
                    var keys = File.ReadLines(_keysPath)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Where(x => x[0] != '#');
                    ComputerVisionClients = keys.Select(x =>
                    new KeyValuePair<string, ComputerVisionClient>(x,
            new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(x),
                new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = "https://westcentralus.api.cognitive.microsoft.com"
            })).ToDictionary(x=>x.Key,x=>x.Value);
                }
                if (ComputerVisionClients.Count==0)
                {
                    _logger.Error("No valid keys.");
                    throw new Exception("No valid keys.");
                }

                _curKey++;
                if (_curKey >= ComputerVisionClients.Count)
                {
                    _curKey = 0;
                }
                return ComputerVisionClients.ElementAt(_curKey);
            }
        }

        public async Task<ImageAnalysis> Meta(string url) {

            var _url = url;
            var client = NextClient();
            var key = client.Key;
            try
            {
                return await client.Value.AnalyzeImageAsync(_url, _features,_details);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                if (ex.Message == "Operation returned an invalid status code 'BadRequest'")
                {
                    return null;
                }
                if (ex.Message == "Operation returned an invalid status code 'Forbidden'")
                {
                    lock (_lock)
                    {
                        if (ComputerVisionClients.ContainsKey(key))
                        {
                            _logger.Warn($"Removing key: {key} ...");
                            ComputerVisionClients.Remove(key);
                        }
                    }

                    await Task.Delay(60*1000);
                    return await Meta(_url);
                }
               
                throw ex;
            }
        }

    }
}
