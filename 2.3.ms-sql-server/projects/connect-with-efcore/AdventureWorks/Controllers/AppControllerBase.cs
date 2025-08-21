using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public abstract class AppControllerBase<T> : ControllerBase
    {
        internal readonly ILogger<T> _logger;

        public AppControllerBase(ILogger<T> logger)
        {
            _logger = logger;
        }
    }
}
