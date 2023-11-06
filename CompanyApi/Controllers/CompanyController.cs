using System.Linq;
using System.Xml.Linq;
using CompanyApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();
        private static List<Employee> employees = new List<Employee>();


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

        [HttpPost("employees")]
        public ActionResult<Employee> CreateEmployee([FromBody] CreateEmployeeRequest employee)
        {
            var company = companies.FirstOrDefault(cp => cp.Name.Equals(employee.CompanyName));
            if (company == null)
            {
                return NotFound();
            }
            else
            {
                var searchRet = employees.FirstOrDefault(ep => ep.Name == employee.Name);
                if (searchRet != null)
                {
                    return BadRequest();
                }
                var employeeNew = new Employee(employee.Name, employee.Title, employee.CompanyName);

                employees.Add(employeeNew);

                return Created("", employeeNew);
            }
        }

        [HttpDelete("employees/{id}")]
        public ActionResult DeleteEmployeeById(string id)
        {
            var searchRet = employees.FirstOrDefault(ep => ep.Id == id);
            if (searchRet == null)
            {
                return NotFound();
            }
            employees.Remove(searchRet);

            return NoContent();
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
            employees.Clear();
        }
    }
}
