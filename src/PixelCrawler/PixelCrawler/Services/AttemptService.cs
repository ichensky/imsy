using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PixelCrawler.Services
{
    public class AttemptService
    {
        static Random rnd = new Random();

        private NLog.Logger _logger;

        public AttemptService(NLog.Logger logger)
        {
            _logger = logger;
        }
        public async Task<K> Operation<K>(Func<Task<K>> func, int maxAttemps, int sleepMinSec=0, int sleepMaxSec=30)
        {
            int attempt = 0;
        begin:
            _logger.Info($"Operation: {func.Method.Name} ; Attempt: {attempt}");
            K result = default(K);
            try
            {
                result = await func();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                if (attempt<maxAttemps)
                {
                    attempt++;
                    var x = attempt * 10;
                    var r = rnd.Next(sleepMinSec+x,sleepMaxSec+x);
                    await Task.Delay(r);
                    goto begin;
                }
                throw;
            }
            
            return result;
        }
    }
}
