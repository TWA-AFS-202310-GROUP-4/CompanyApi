using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

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
        public ActionResult<List<Company>> GetAllCompanies()
        {
            return StatusCode(StatusCodes.Status200OK, companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetCompanyById(string id)
        {
            var quiredResult = companies.FirstOrDefault(company => company.Id.Equals(id));
            if (quiredResult != null)
            {
                return StatusCode(StatusCodes.Status200OK, quiredResult);
            }

            return NotFound();
        }

        [Route("pagination"), HttpGet]
        public ActionResult<List<Company>> GetCompaniesWithPagination([FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            int pageSizeInt = Int32.Parse(pageSize);
            int pageNumberInt = Int32.Parse(pageNumber);
            int startIndex = pageSizeInt * (pageNumberInt - 1);
            var queriedCompanies = companies.GetRange(startIndex, startIndex + pageSizeInt - 1); 
            
            return StatusCode(StatusCodes.Status200OK, queriedCompanies);
        }

        [HttpPut]
        public ActionResult<Company> UpdateCompany(Company newCompanyInfo) 
        {
            int index = companies.FindIndex(company => company.Id.Equals(newCompanyInfo.Id));
            companies[index].Name = newCompanyInfo.Name;
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
