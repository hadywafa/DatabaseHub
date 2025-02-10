using EF_AdvectureWorks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Controllers
{
    public class AdventureWorksController : AppControllerBase<AdventureWorksController>
    {
        private readonly AdventureWorks2012Context _context;

        public AdventureWorksController(
            AdventureWorks2012Context context,
            ILogger<AdventureWorksController> logger
        )
            : base(logger)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get_Q1()
        {
            var query = await _context.Employee.OrderBy(x => x.JobTitle).Take(10).ToListAsync();
            return Ok(query);
        }

        //Q3 => easy
        [HttpGet]
        public async Task<IActionResult> Get_Q3()
        {
            _logger.LogError("a7a");
            var query = await _context.Person
                .Select(
                    x =>
                        new
                        {
                            firstname = x.FirstName,
                            lastname = x.LastName,
                            employee_id = x.BusinessEntityId
                        }
                )
                .OrderBy(x => x.lastname)
                .Take(10)
                .ToListAsync();
            return Ok(query);
        }
    }
}
