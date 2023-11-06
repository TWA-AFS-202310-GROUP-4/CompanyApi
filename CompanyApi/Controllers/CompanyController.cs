using CompanyApi.DTO;
using CompanyApi.Request;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

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
        public ActionResult<List<Company>> GetAllCompanies([FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            if (pageSize != null && pageSize != null)
            {
                int pages = (int)Math.Ceiling((double)companies.Count / (double)pageSize);
                if (pageSize <= 0 || pageIndex < 1 || pages < pageIndex)
                {
                    return BadRequest();
                }

                int startIndex = (int)((pageIndex - 1) * pageSize);
                int companiesToTake = (int)((pageSize < (companies.Count - startIndex)) ? pageSize : companies.Count);
                List<Company> filterdCompanies = companies.Skip((int)((pages - 1) * pageSize)).Take(companiesToTake).ToList();
                return Ok(filterdCompanies);
            }
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetCompanyById(string id)
        {
            var index = companies.FindIndex(company => company.Id == id);
            if (index == -1)
            {
                return NoContent();
            }
            return Ok(companies[index]);
        }

        [HttpPut("{id}")]
        public ActionResult<Company> UpdateCompanyById(string id, [FromBody] UpdateCompanyRequest updateCompanyRequest)
        {
            var index = companies.FindIndex(company => company.Id == id);
            if (index == -1)
            {
                return NoContent();
            }
            companies[index].Name= updateCompanyRequest.Name;
            return Ok(companies[index]);
        }

        [HttpPost("{id}/employees")]
        public ActionResult<Employee> CreateNewEmplpyee(string id,[FromBody] CreateEmployeeRequest createEmployeeRequest)
        {
            var companyIndex=companies.FindIndex((company) => company.Id == id);
            if (companyIndex == -1)
            {
                return NotFound();
            }
            string emloyeeName = createEmployeeRequest.Name;
            Employee newEmployee = new (id,emloyeeName);

            companies[companyIndex].EmployeesList.Add(newEmployee);

            return Created("",newEmployee);
           
        }

        [HttpDelete("{companyId}/employees/{employeeId}")]
        public ActionResult<Employee> DeleteSpecificEmplpyee(string companyId, string employeeId)
        {
            var companyIndex = companies.FindIndex((company) => company.Id == companyId);
            if (companyIndex == -1)
            {
                return NotFound();
            }
            var employeeIndex = companies[companyIndex].EmployeesList.FindIndex(employee => employee.EmployeeId == employeeId);
            if (employeeIndex == -1)
            {
                return NotFound();
            }
            companies[companyIndex].EmployeesList.RemoveAt(employeeIndex);
            return NoContent();

        }
    }
}
