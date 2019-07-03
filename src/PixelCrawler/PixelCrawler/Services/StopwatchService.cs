using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace PixelCrawler.Services
{
    public class StopwatchService
    {
        private NLog.Logger _logger;

        public StopwatchService(NLog.Logger logger) {
            _logger = logger;
        }
        public async Task<K> Operation<K>(Func<Task<K>> func) {

            var sw = Stopwatch.StartNew();
            K result = default(K);
            try
            {
                result = await func();
                sw.Stop();
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.Error(ex);
                throw;
            }
            finally
            {
                _logger.Info($"`{func.Method.Name}` executed for: {sw.ElapsedMilliseconds} milliseconds");
            }
            return result;
        }
    }
}
