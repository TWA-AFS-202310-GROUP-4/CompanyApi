using CompanyApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();
        private static List<Employee> employees= new List<Employee>();

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
            employees.Clear();
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

        [HttpGet]
        public ActionResult<List<Company>> GetPageBySizeIndex([FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            if (pageIndex == null || pageSize == null)
            {
                return Ok(companies);
            }

            int start = (int)(pageSize * pageIndex);
            if (start + pageSize > companies.Count)
            {
                return Ok(new());
            }
            else
            {
                return Ok(companies.Skip(start).Take((int)pageSize).ToList());
            }
        }

        [HttpPut("{id}")]
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

        [HttpPost("employees")]
        public ActionResult<Employee> CreateEmployee([FromBody] EmployeeCreate employeeCreate)
        {
            var company = companies.Find(company => company.Id.Equals(employeeCreate.CompanyId));
            if (company == null)
            {
                return NotFound();
            }
            else
            {
                var _employee = employees.Find(e => e.Name == employeeCreate.Name);
                if (_employee != null) {
                    return BadRequest();
                }
                var employee = new Employee()
                {
                    Name = employeeCreate.Name,
                    Position = employeeCreate.Position,
                    CompanyId = employeeCreate.CompanyId
                };
                employees.Add(employee);

                return Created("", employee);
            }
        }

        [HttpDelete("employees/{id}")]
        public ActionResult DeleteEmployee(string id) 
        {
            var e = employees.Find(e => e.Id == id);
            if (e == null)
            {
                return NotFound();
            }
            employees.Remove(e);

            return NoContent();
        }
    }
}
