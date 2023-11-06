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

        [HttpGet]
        public ActionResult<List<Company>> GetCompaniesWithPagination([FromQuery] string? pageSize, [FromQuery] string? pageNumber)
        {
            int pageSizeInt = Int32.Parse(pageSize);
            int pageNumberInt = Int32.Parse(pageNumber);
            int startIndex = pageSizeInt * (pageNumberInt - 1);
            List<Company> queriedCompanies = new List<Company>();
            for (int i = startIndex; i < startIndex + pageSizeInt; i++)
            {
                queriedCompanies.Add(companies[i]);
            }
            
            
            return StatusCode(StatusCodes.Status200OK, queriedCompanies);
        }

        [HttpPut]
        public ActionResult<Company> UpdateCompany(Company newCompanyInfo) 
        {
            int index = companies.FindIndex(company => company.Id.Equals(newCompanyInfo.Id));
            if (index >= 0)
            {
                companies[index].Name = newCompanyInfo.Name;
                return StatusCode(StatusCodes.Status204NoContent);
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
           
        }

        [HttpPut("{companyId}")]
        public ActionResult<Company> AddEmployeeToACompany([FromRoute] string companyId, Employee newEmployee)
        {
            int index = companies.FindIndex(company => company.Id.Equals(companyId));
            if (index >= 0)
            {
               companies[index].Employees.Add(newEmployee);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }


        [HttpDelete("{companyId}/employees/{employeeId}")]
        public ActionResult<Company> DeleteAnEmployeeOfAnCompany([FromRoute] string? companyId, [FromRoute] string? employeeId)
        {
            int companyIndex = companies.FindIndex(company => company.Id.Equals(companyId));
            if (companyIndex >= 0)
            {
                var employees = companies[companyIndex].Employees;
                var employeeIndex = employees.FindIndex(Employee => Employee.Id.Equals(employeeId));
                if (employeeIndex >= 0)
                {
                    employees.RemoveAt(companyIndex);
                    return StatusCode(StatusCodes.Status204NoContent);
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
    }
}
