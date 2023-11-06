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
        public ActionResult<List<Company>> Get()
        {
            return StatusCode(StatusCodes.Status200OK, companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetByName(string id)
        {
            if (companies.Exists(company => company.Id.Equals(id)))
            {

                return Ok(companies.Find(company => company.Id.Equals(id)));
            }

            return StatusCode(StatusCodes.Status404NotFound);
        }

        /*
        [HttpGet]
        public ActionResult<List<Company>> GetByPageSize(string pageSize, string pageIndex)
        {
            int size = Int32.Parse(pageSize);
            int index = Int32.Parse(pageIndex);
            int total = companies.Count;
           int pageTotalNum = total / size;
           pageTotalNum = pageTotalNum % size == 0? pageTotalNum: pageTotalNum++;
           int startIndex = index * size;
           int endIndex = Math.Min(startIndex + size, total);
           List<Company> returnedCompanies = companies.GetRange(startIndex, endIndex);
           return Ok(returnedCompanies);

        }*/

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
    }
}
