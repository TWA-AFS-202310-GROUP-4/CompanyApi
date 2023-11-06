using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpGet]
        public ActionResult<List<Company>> GetPartialAll([FromQuery] int? pageSize, [FromQuery] int? pageIndex = 1)
        {
            if (pageSize > 0 && pageIndex > 0)
            {
                var pagedCompanies = companies.Skip((int)((pageIndex - 1) * pageSize)).Take((int)pageSize).ToList();
                return pagedCompanies.Any() ? pagedCompanies : NotFound();
            }
            
            return companies;
        }

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            var company = companies.FirstOrDefault(cp => cp.Id == id);

            if (company != null)
            {
                return company;
            }

            return NotFound();
        }


        [HttpPut("{id}")]
        public ActionResult UpdateCompany(string id, [FromBody] CreateCompanyRequest updatedCompany)
        {
            var company = companies.FirstOrDefault(cp => cp.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            company.Name = updatedCompany.Name;

            return NoContent();
        }

        [HttpPost]
        public ActionResult<Company> Create(CreateCompanyRequest request)
        {
            if (companies.Exists(company => company.Name.Equals(request.Name)))
            {
                return BadRequest();
            }
            Company companyCreated = new Company(request.Name);
            companies.Add(companyCreated);
            return StatusCode(StatusCodes.Status201Created, companyCreated);
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
