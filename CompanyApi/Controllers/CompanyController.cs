using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();
        private static Dictionary<string,List<string>> company2Employees = new ();

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
        /*
        [HttpGet]
        public ActionResult<List<Company>> Get()
        {
            return StatusCode(StatusCodes.Status200OK, companies);
        }*/

        [HttpGet("{id}")]
        public ActionResult<Company> GetByName(string id)
        {
            if (companies.Exists(company => company.Id.Equals(id)))
            {

                return Ok(companies.Find(company => company.Id.Equals(id)));
            }

            return StatusCode(StatusCodes.Status404NotFound);
        }

        
        [HttpGet]
        public ActionResult<List<Company>> GetByPageSize([FromQuery]int? pageSize, [FromQuery]int? pageIndex)
        {
            if (pageSize == null || pageIndex == null)
            {
                return Ok(companies);
            }

           List<Company> returnedCompanies = companies.Skip(((int)pageIndex-1) * (int)pageSize).Take((int)pageSize).ToList();
           return Ok(returnedCompanies);

        }

        [HttpPut("{id}")]
        public ActionResult<Company> PutCompanyById(string id, CreateCompanyRequest request)
        {
            var index = companies.FindIndex(company => company.Id.Equals(id));
            if (index != -1)
            {
                companies[index].Name = request.Name;

                return StatusCode(StatusCodes.Status204NoContent);
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpPost("{id}")]
        public ActionResult<Employee> CreateEmployee(string id, Employee request)
        {
            if (company2Employees.ContainsKey(id))
            {
                if (company2Employees[id].Exists(employee => employee.Equals(request.Id)))
                {
                    return BadRequest();
                }
                company2Employees[id].Add(request.Id);
            }

            List<string> employees = new List<string>();
            employees.Add(request.Id);
            company2Employees.Add(id, employees);
            return StatusCode(StatusCodes.Status201Created, request);
        }

       
    }
}
