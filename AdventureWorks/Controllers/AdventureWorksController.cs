using EF_AdvectureWorks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdventureWorksController : ControllerBase
    {
        private readonly ILogger<AdventureWorksController> _logger;
        private readonly AdventureWorks2012Context _context;

        public AdventureWorksController(
            ILogger<AdventureWorksController> logger,
            AdventureWorks2012Context context
        )
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "Get_Q1")]
        public async Task<IActionResult> Get_Q1()
        {
            var query = await _context.Employee.OrderBy(x => x.JobTitle).Take(10).ToListAsync();
            return Ok(query);
        }
    }
}
