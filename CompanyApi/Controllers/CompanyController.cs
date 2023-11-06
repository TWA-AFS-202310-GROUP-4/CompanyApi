using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

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

        [HttpGet]
        public ActionResult<List<Company>> GetAll()
        {
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetById(string id)
        {
            var company = companies.Find(company => company.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(company);
            }
        }

        [HttpGet("{pageSize}&{pageIndex}")]
        public ActionResult<List<Company>> GetPageBySizeIndex(int pageSize, int pageIndex)
        {
            int start = pageSize * pageIndex;
            if (start + pageSize > companies.Count)
            {
                return Ok(new());
            }
            else
            {
                return Ok(companies.Skip(start).Take(pageSize).ToList());
            }
        }

        [HttpGet("{id}")]
        public ActionResult UpdateById(string id, [FromBody] CreateCompanyRequest companyToUpdate)
        {
            var company = companies.Find(company => company.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            else
            {
                company.Name = companyToUpdate.Name;
                return NoContent();
            }
        }
    }
}
