using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>() 
        {
            new Company("Google1"),
            new Company("Google2"),
            new Company("Google3"),
            new Company("Google4"),
            new Company("Google5"),
            new Company("Google6"),
        };

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

        [HttpGet]
        public ActionResult<List<Company>> GetAll()
        {
            return StatusCode(StatusCodes.Status200OK, companies);
        }

        [HttpGet("{id}")]

        public ActionResult<Company> Get(string id)
        {
            Company company = companies.Find(company =>company.Id == id);
            if (company != null)
            {
                return StatusCode(StatusCodes.Status200OK, company);

            }
            return NotFound();


        }

        [HttpGet("page")]
        public ActionResult<List<Company>> GetPage(int pageSize, int pageIndex)
        {
           var responseCompany = companies.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
           return Ok(responseCompany);
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }


    }
}
