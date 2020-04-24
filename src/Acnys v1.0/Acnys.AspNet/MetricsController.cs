using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Acnys.AspNet
{
    //[Route("metrics")]
    //[ApiController]
    public class MetricsController : ControllerBase
    {
        // GET /metrics
        [HttpGet]
        public async Task<ActionResult<string>> GetMetrics()
        {
            var s = new MemoryStream();
            await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(s);
            s.Position = 0;
            var reader = new StreamReader(s);
            var ret = await reader.ReadToEndAsync();
            return ret;
        }
    }
}